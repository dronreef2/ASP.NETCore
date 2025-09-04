using Microsoft.Extensions.Logging;
using TutorCopiloto.Services;

namespace TutorCopiloto.Services.AgentTasks.Agents
{
    /// <summary>
    /// Agente para revisão de código usando IA
    /// </summary>
    public class CodeReviewAgent : BaseAgentTask
    {
        private readonly IAIService _aiService;

        public CodeReviewAgent(IAIService aiService, ILogger<CodeReviewAgent> logger) : base(logger)
        {
            _aiService = aiService;
        }

        public override string AgentName => "Code Review Agent";
        public override string Description => "Analisa código fonte e identifica problemas de qualidade, bugs potenciais e oportunidades de melhoria";
        public override int Priority => 1; // Alta prioridade

        protected override async Task<bool> CanExecuteInternalAsync(string repositoryPath, RepositoryAnalysisContext context)
        {
            // Verifica se há arquivos de código para analisar
            var codeFiles = GetCodeFiles(repositoryPath);
            return codeFiles.Any() && await _aiService.IsAvailableAsync();
        }

        protected override async Task<AgentTaskResult> ExecuteInternalAsync(string repositoryPath, RepositoryAnalysisContext context)
        {
            var result = new AgentTaskResult
            {
                Summary = "Análise de código concluída"
            };

            var codeFiles = GetCodeFiles(repositoryPath);
            var analyzedFiles = 0;
            var maxFilesToAnalyze = context.Options.MaxFindingsPerAgent / 10; // Limita arquivos analisados

            foreach (var file in codeFiles.Take(maxFilesToAnalyze))
            {
                try
                {
                    await AnalyzeCodeFile(file, repositoryPath, result, context);
                    analyzedFiles++;
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Erro ao analisar arquivo {File}", file);
                    result.Findings.Add(CreateFinding(
                        "Error", "Low", $"Erro na análise do arquivo {GetRelativePath(repositoryPath, file)}",
                        $"Não foi possível analisar o arquivo: {ex.Message}", GetRelativePath(repositoryPath, file)));
                }
            }

            // Adiciona recomendações gerais
            AddGeneralRecommendations(result, codeFiles.Count, analyzedFiles);

            result.Summary = $"Analisados {analyzedFiles} de {codeFiles.Count} arquivos de código. " +
                           $"Encontrados {result.Findings.Count} problemas.";

            return result;
        }

        private async Task AnalyzeCodeFile(string filePath, string repositoryPath, AgentTaskResult result, RepositoryAnalysisContext context)
        {
            var content = await File.ReadAllTextAsync(filePath);
            var relativePath = GetRelativePath(repositoryPath, filePath);
            var extension = Path.GetExtension(filePath).ToLower();

            // Análise baseada em padrões conhecidos
            AnalyzeCommonIssues(content, relativePath, result);

            // Análise com IA se o arquivo não for muito grande
            if (content.Length < 10000) // Limita a 10KB para evitar tokens excessivos
            {
                await AnalyzeWithAI(content, relativePath, result, extension);
            }
        }

        private void AnalyzeCommonIssues(string content, string relativePath, AgentTaskResult result)
        {
            var lines = content.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var lineNumber = i + 1;

                // Verifica problemas comuns
                CheckForCommonIssues(line, lineNumber, relativePath, result);
            }
        }

        private void CheckForCommonIssues(string line, int lineNumber, string relativePath, AgentTaskResult result)
        {
            // Console.WriteLine usage (should use logging)
            if (line.Contains("Console.WriteLine", StringComparison.OrdinalIgnoreCase))
            {
                result.Findings.Add(CreateFinding(
                    "Code Quality", "Medium", "Uso de Console.WriteLine detectado",
                    "Considere usar um sistema de logging estruturado ao invés de Console.WriteLine",
                    relativePath, lineNumber, "LOGGING_001", true));
            }

            // Hardcoded strings/secrets
            if (line.Contains("password", StringComparison.OrdinalIgnoreCase) && 
                (line.Contains("=") || line.Contains(":")))
            {
                result.Findings.Add(CreateFinding(
                    "Security", "High", "Possível credencial hardcoded",
                    "Detectado possível senha ou credencial hardcoded no código",
                    relativePath, lineNumber, "SECURITY_001"));
            }

            // TODO comments
            if (line.TrimStart().StartsWith("// TODO", StringComparison.OrdinalIgnoreCase))
            {
                result.Findings.Add(CreateFinding(
                    "Maintenance", "Low", "Comentário TODO encontrado",
                    "Considere implementar ou remover comentários TODO pendentes",
                    relativePath, lineNumber, "MAINT_001"));
            }

            // Long lines
            if (line.Length > 120)
            {
                result.Findings.Add(CreateFinding(
                    "Code Style", "Low", "Linha muito longa",
                    $"Linha com {line.Length} caracteres excede o limite recomendado de 120",
                    relativePath, lineNumber, "STYLE_001", true));
            }

            // Exception catching without logging
            if (line.Contains("catch") && line.Contains("Exception"))
            {
                result.Findings.Add(CreateFinding(
                    "Error Handling", "Medium", "Tratamento de exceção sem logging",
                    "Considere adicionar logging adequado no tratamento de exceções",
                    relativePath, lineNumber, "ERROR_001"));
            }
        }

        private async Task AnalyzeWithAI(string content, string relativePath, AgentTaskResult result, string extension)
        {
            try
            {
                var language = GetLanguageFromExtension(extension);
                var prompt = $@"
Analise o seguinte código {language} e identifique problemas:

Arquivo: {relativePath}
```{language}
{content}
```

Procure por:
1. Bugs potenciais
2. Problemas de performance
3. Violações de boas práticas
4. Questões de segurança
5. Oportunidades de refatoração

Para cada problema encontrado, forneça:
- Tipo do problema
- Gravidade (Critical/High/Medium/Low)
- Linha aproximada
- Descrição
- Sugestão de correção

Limite a 5 problemas mais importantes.";

                var aiResponse = await _aiService.GetChatResponseAsync(prompt, "code-review");
                
                if (!string.IsNullOrEmpty(aiResponse))
                {
                    ParseAIResponse(aiResponse, relativePath, result);
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Erro na análise com IA para arquivo {File}", relativePath);
            }
        }

        private void ParseAIResponse(string aiResponse, string relativePath, AgentTaskResult result)
        {
            // Análise simples da resposta da IA
            var lines = aiResponse.Split('\n');
            
            foreach (var line in lines)
            {
                if (line.Contains("Critical", StringComparison.OrdinalIgnoreCase) ||
                    line.Contains("High", StringComparison.OrdinalIgnoreCase) ||
                    line.Contains("Medium", StringComparison.OrdinalIgnoreCase))
                {
                    var severity = ExtractSeverity(line);
                    var description = line.Trim();

                    result.Findings.Add(CreateFinding(
                        "AI Analysis", severity, "Problema identificado pela IA",
                        description, relativePath, 0, "AI_001"));
                }
            }
        }

        private string ExtractSeverity(string line)
        {
            if (line.Contains("Critical", StringComparison.OrdinalIgnoreCase)) return "Critical";
            if (line.Contains("High", StringComparison.OrdinalIgnoreCase)) return "High";
            if (line.Contains("Medium", StringComparison.OrdinalIgnoreCase)) return "Medium";
            return "Low";
        }

        private void AddGeneralRecommendations(AgentTaskResult result, int totalFiles, int analyzedFiles)
        {
            result.Recommendations.Add(CreateRecommendation(
                "Code Quality", "Medium", "Implementar análise estática contínua",
                "Configure ferramentas de análise estática como SonarQube, CodeQL ou Roslyn Analyzers",
                new List<string> 
                { 
                    "Configurar SonarQube ou similar",
                    "Adicionar analyzers no projeto",
                    "Configurar CI/CD para análise automática"
                }));

            if (analyzedFiles < totalFiles)
            {
                result.Recommendations.Add(CreateRecommendation(
                    "Analysis Coverage", "Low", "Análise limitada de arquivos",
                    $"Apenas {analyzedFiles} de {totalFiles} arquivos foram analisados devido a limitações de recursos",
                    new List<string> 
                    { 
                        "Considere aumentar o limite de análise",
                        "Configure análise incremental para arquivos modificados"
                    }));
            }
        }

        private List<string> GetCodeFiles(string repositoryPath)
        {
            var codeExtensions = new[] { "*.cs", "*.js", "*.ts", "*.py", "*.java", "*.cpp", "*.c", "*.h", "*.php", "*.rb", "*.go" };
            var codeFiles = new List<string>();

            foreach (var extension in codeExtensions)
            {
                codeFiles.AddRange(FindFiles(repositoryPath, extension));
            }

            // Exclui arquivos gerados ou de terceiros
            return codeFiles
                .Where(f => !f.Contains("node_modules") && 
                           !f.Contains("bin/") && 
                           !f.Contains("obj/") &&
                           !f.Contains(".min."))
                .OrderBy(f => f)
                .ToList();
        }

        private string GetLanguageFromExtension(string extension)
        {
            return extension.ToLower() switch
            {
                ".cs" => "csharp",
                ".js" => "javascript",
                ".ts" => "typescript",
                ".py" => "python",
                ".java" => "java",
                ".cpp" or ".c" or ".h" => "cpp",
                ".php" => "php",
                ".rb" => "ruby",
                ".go" => "go",
                _ => "text"
            };
        }

        public override TimeSpan EstimateExecutionTime()
        {
            return TimeSpan.FromMinutes(10); // Análise de código pode demorar mais
        }
    }
}