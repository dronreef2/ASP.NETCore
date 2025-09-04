using Microsoft.Extensions.Logging;
using TutorCopiloto.Services;

namespace TutorCopiloto.Services.AgentTasks.Agents
{
    /// <summary>
    /// Agente para análise e melhoria de documentação
    /// </summary>
    public class DocumentationAgent : BaseAgentTask
    {
        private readonly IAIService _aiService;

        public DocumentationAgent(IAIService aiService, ILogger<DocumentationAgent> logger) : base(logger)
        {
            _aiService = aiService;
        }

        public override string AgentName => "Documentation Agent";
        public override string Description => "Analisa e sugere melhorias na documentação do projeto";
        public override int Priority => 5; // Menor prioridade

        protected override async Task<bool> CanExecuteInternalAsync(string repositoryPath, RepositoryAnalysisContext context)
        {
            // Sempre pode executar
            return await Task.FromResult(true);
        }

        protected override async Task<AgentTaskResult> ExecuteInternalAsync(string repositoryPath, RepositoryAnalysisContext context)
        {
            var result = new AgentTaskResult
            {
                Summary = "Análise de documentação concluída"
            };

            // Verifica presença de arquivos de documentação essenciais
            await CheckEssentialDocumentation(repositoryPath, result);

            // Analisa qualidade do README
            await AnalyzeReadmeQuality(repositoryPath, result);

            // Analisa comentários no código
            await AnalyzeCodeDocumentation(repositoryPath, result);

            // Verifica documentação da API
            await AnalyzeApiDocumentation(repositoryPath, result);

            // Analisa arquivos de configuração
            await AnalyzeConfigurationDocumentation(repositoryPath, result);

            // Adiciona recomendações gerais
            AddDocumentationRecommendations(result);

            result.Summary = $"Análise de documentação concluída. Encontrados {result.Findings.Count} problemas de documentação.";
            return result;
        }

        private async Task CheckEssentialDocumentation(string repositoryPath, AgentTaskResult result)
        {
            var essentialFiles = new Dictionary<string, string>
            {
                ["README.md"] = "Arquivo README principal",
                ["CONTRIBUTING.md"] = "Guia de contribuição",
                ["LICENSE"] = "Arquivo de licença",
                ["CHANGELOG.md"] = "Log de mudanças",
                ["docs/"] = "Diretório de documentação"
            };

            foreach (var (fileName, description) in essentialFiles)
            {
                var exists = fileName.EndsWith("/") 
                    ? Directory.Exists(Path.Combine(repositoryPath, fileName))
                    : File.Exists(Path.Combine(repositoryPath, fileName)) || 
                      File.Exists(Path.Combine(repositoryPath, fileName.ToLower()));

                if (!exists)
                {
                    var severity = fileName == "README.md" ? "High" : "Medium";
                    result.Findings.Add(CreateFinding(
                        "Documentation", severity, $"{description} ausente",
                        $"O arquivo {fileName} não foi encontrado. Este arquivo é importante para a documentação do projeto",
                        fileName, 0, "DOC_MISSING_001", true));
                }
            }
        }

        private async Task AnalyzeReadmeQuality(string repositoryPath, AgentTaskResult result)
        {
            var readmeFiles = new[] { "README.md", "readme.md", "README.txt", "readme.txt" };
            
            foreach (var readmeFile in readmeFiles)
            {
                var readmePath = Path.Combine(repositoryPath, readmeFile);
                if (File.Exists(readmePath))
                {
                    await AnalyzeReadmeFile(readmePath, repositoryPath, result);
                    break;
                }
            }
        }

        private async Task AnalyzeReadmeFile(string readmePath, string repositoryPath, AgentTaskResult result)
        {
            try
            {
                var content = await File.ReadAllTextAsync(readmePath);
                var relativePath = GetRelativePath(repositoryPath, readmePath);

                // Verifica tamanho mínimo
                if (content.Length < 200)
                {
                    result.Findings.Add(CreateFinding(
                        "Documentation", "Medium", "README muito curto",
                        "O arquivo README é muito curto e pode não fornecer informações adequadas sobre o projeto",
                        relativePath, 0, "DOC_README_001", true));
                }

                // Verifica seções essenciais
                var essentialSections = new Dictionary<string, string>
                {
                    ["## Instalação"] = "Instruções de instalação",
                    ["## Como usar"] = "Instruções de uso",
                    ["## Contribuindo"] = "Informações sobre contribuição",
                    ["## Licença"] = "Informações de licença"
                };

                foreach (var (section, description) in essentialSections)
                {
                    if (!content.Contains(section, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Findings.Add(CreateFinding(
                            "Documentation", "Low", $"Seção {description} ausente no README",
                            $"O README não contém a seção '{section}' que ajudaria os usuários",
                            relativePath, 0, "DOC_README_002", true));
                    }
                }

                // Verifica se há exemplos de código
                if (!content.Contains("```") && !content.Contains("    "))
                {
                    result.Findings.Add(CreateFinding(
                        "Documentation", "Medium", "README sem exemplos de código",
                        "O README não contém exemplos de código que ajudariam os usuários a entender como usar o projeto",
                        relativePath, 0, "DOC_README_003", true));
                }

                // Usa IA para análise mais detalhada se disponível
                if (await _aiService.IsAvailableAsync())
                {
                    await AnalyzeReadmeWithAI(content, relativePath, result);
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Erro ao analisar README {File}", readmePath);
            }
        }

        private async Task AnalyzeReadmeWithAI(string content, string relativePath, AgentTaskResult result)
        {
            try
            {
                var prompt = $@"
Analise o seguinte README e sugira melhorias:

```markdown
{content}
```

Avalie:
1. Clareza e organização
2. Completude das informações
3. Qualidade dos exemplos
4. Facilidade de uso para novos usuários
5. Informações técnicas necessárias

Forneça sugestões específicas de melhoria em formato de lista.";

                var aiResponse = await _aiService.GetChatResponseAsync(prompt, "readme-analysis");
                
                if (!string.IsNullOrEmpty(aiResponse))
                {
                    result.Findings.Add(CreateFinding(
                        "Documentation", "Low", "Sugestões de melhoria do README pela IA",
                        $"A IA identificou possíveis melhorias: {aiResponse}",
                        relativePath, 0, "DOC_AI_001"));
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Erro na análise do README com IA");
            }
        }

        private async Task AnalyzeCodeDocumentation(string repositoryPath, AgentTaskResult result)
        {
            var codeFiles = FindFiles(repositoryPath, "*.cs").Take(20); // Limita análise
            
            foreach (var file in codeFiles)
            {
                await AnalyzeCodeFileDocumentation(file, repositoryPath, result);
            }
        }

        private async Task AnalyzeCodeFileDocumentation(string filePath, string repositoryPath, AgentTaskResult result)
        {
            try
            {
                var content = await File.ReadAllTextAsync(filePath);
                var relativePath = GetRelativePath(repositoryPath, filePath);
                var lines = content.Split('\n');

                var publicMethods = 0;
                var documentedMethods = 0;
                var publicClasses = 0;
                var documentedClasses = 0;

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    var previousLine = i > 0 ? lines[i - 1].Trim() : "";

                    // Conta classes públicas
                    if (line.Contains("public class") || line.Contains("public interface"))
                    {
                        publicClasses++;
                        if (previousLine.StartsWith("///"))
                        {
                            documentedClasses++;
                        }
                    }

                    // Conta métodos públicos
                    if (line.Contains("public") && (line.Contains("void") || line.Contains("Task") || line.Contains("string") || line.Contains("int")))
                    {
                        publicMethods++;
                        if (previousLine.StartsWith("///") || HasXmlDocumentation(lines, i))
                        {
                            documentedMethods++;
                        }
                    }
                }

                // Calcula taxa de documentação
                if (publicClasses > 0)
                {
                    var classDocRate = (double)documentedClasses / publicClasses;
                    if (classDocRate < 0.5)
                    {
                        result.Findings.Add(CreateFinding(
                            "Documentation", "Medium", "Documentação insuficiente de classes",
                            $"Apenas {documentedClasses} de {publicClasses} classes públicas estão documentadas ({classDocRate:P0})",
                            relativePath, 0, "DOC_CODE_001"));
                    }
                }

                if (publicMethods > 0)
                {
                    var methodDocRate = (double)documentedMethods / publicMethods;
                    if (methodDocRate < 0.3)
                    {
                        result.Findings.Add(CreateFinding(
                            "Documentation", "Medium", "Documentação insuficiente de métodos",
                            $"Apenas {documentedMethods} de {publicMethods} métodos públicos estão documentados ({methodDocRate:P0})",
                            relativePath, 0, "DOC_CODE_002"));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Erro ao analisar documentação do código {File}", filePath);
            }
        }

        private bool HasXmlDocumentation(string[] lines, int methodLineIndex)
        {
            // Verifica se há documentação XML nas linhas anteriores
            for (int i = methodLineIndex - 1; i >= 0 && i >= methodLineIndex - 5; i--)
            {
                if (lines[i].Trim().StartsWith("///"))
                    return true;
                if (!string.IsNullOrWhiteSpace(lines[i]) && !lines[i].Trim().StartsWith("//"))
                    break;
            }
            return false;
        }

        private async Task AnalyzeApiDocumentation(string repositoryPath, AgentTaskResult result)
        {
            // Verifica se há controladores de API
            var controllerFiles = FindFiles(repositoryPath, "*Controller.cs");
            
            if (controllerFiles.Any())
            {
                // Verifica se há documentação Swagger/OpenAPI
                var hasSwaggerConfig = await CheckForSwaggerConfiguration(repositoryPath);
                
                if (!hasSwaggerConfig)
                {
                    result.Findings.Add(CreateFinding(
                        "Documentation", "Medium", "Documentação de API ausente",
                        "Projeto contém controladores de API mas não tem configuração Swagger/OpenAPI",
                        "", 0, "DOC_API_001", true));
                }

                foreach (var controller in controllerFiles.Take(5))
                {
                    await AnalyzeControllerDocumentation(controller, repositoryPath, result);
                }
            }
        }

        private async Task<bool> CheckForSwaggerConfiguration(string repositoryPath)
        {
            var configFiles = FindFiles(repositoryPath, "*.cs");
            
            foreach (var file in configFiles)
            {
                var content = await File.ReadAllTextAsync(file);
                if (content.Contains("AddSwaggerGen") || content.Contains("UseSwagger"))
                {
                    return true;
                }
            }
            
            return false;
        }

        private async Task AnalyzeControllerDocumentation(string filePath, string repositoryPath, AgentTaskResult result)
        {
            try
            {
                var content = await File.ReadAllTextAsync(filePath);
                var relativePath = GetRelativePath(repositoryPath, filePath);

                // Verifica atributos de documentação
                var hasProducesResponseType = content.Contains("ProducesResponseType");
                var hasApiDocumentation = content.Contains("/// <summary>");

                if (!hasProducesResponseType)
                {
                    result.Findings.Add(CreateFinding(
                        "Documentation", "Low", "Controlador sem atributos de resposta",
                        "Controlador não usa ProducesResponseType para documentar respostas da API",
                        relativePath, 0, "DOC_API_002", true));
                }

                if (!hasApiDocumentation)
                {
                    result.Findings.Add(CreateFinding(
                        "Documentation", "Low", "Controlador sem documentação XML",
                        "Controlador não possui comentários XML para documentação automática",
                        relativePath, 0, "DOC_API_003", true));
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Erro ao analisar controlador {File}", filePath);
            }
        }

        private async Task AnalyzeConfigurationDocumentation(string repositoryPath, AgentTaskResult result)
        {
            var configFiles = new[] { "appsettings.json", "docker-compose.yml", "Dockerfile" };
            
            foreach (var configFile in configFiles)
            {
                var configPath = Path.Combine(repositoryPath, configFile);
                if (File.Exists(configPath))
                {
                    await AnalyzeConfigFile(configPath, repositoryPath, result);
                }
            }
        }

        private async Task AnalyzeConfigFile(string filePath, string repositoryPath, AgentTaskResult result)
        {
            try
            {
                var content = await File.ReadAllTextAsync(filePath);
                var relativePath = GetRelativePath(repositoryPath, filePath);

                // Verifica se há comentários explicativos
                var hasComments = content.Contains("//") || content.Contains("#");
                
                if (!hasComments && content.Length > 100)
                {
                    result.Findings.Add(CreateFinding(
                        "Documentation", "Low", "Arquivo de configuração sem comentários",
                        "Arquivo de configuração não possui comentários explicativos",
                        relativePath, 0, "DOC_CONFIG_001", true));
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Erro ao analisar arquivo de configuração {File}", filePath);
            }
        }

        private void AddDocumentationRecommendations(AgentTaskResult result)
        {
            result.Recommendations.Add(CreateRecommendation(
                "Documentation", "High", "Melhorar documentação do projeto",
                "Estabeleça um padrão consistente de documentação para o projeto",
                new List<string>
                {
                    "Criar template de README abrangente",
                    "Estabelecer padrões de comentários no código",
                    "Configurar geração automática de documentação",
                    "Implementar revisão de documentação no processo de PR"
                }));

            result.Recommendations.Add(CreateRecommendation(
                "Documentation", "Medium", "Automatizar documentação de API",
                "Configure ferramentas para gerar documentação de API automaticamente",
                new List<string>
                {
                    "Configurar Swagger/OpenAPI",
                    "Adicionar atributos de documentação nos controladores",
                    "Gerar documentação da API automaticamente no CI/CD",
                    "Criar exemplos de uso da API"
                }));

            result.Recommendations.Add(CreateRecommendation(
                "Documentation", "Low", "Melhorar documentação inline",
                "Aumente a qualidade dos comentários no código",
                new List<string>
                {
                    "Documentar classes e métodos públicos",
                    "Adicionar comentários em lógica complexa",
                    "Usar XML documentation para geração automática",
                    "Revisar e atualizar comentários regularmente"
                }));
        }

        public override TimeSpan EstimateExecutionTime()
        {
            return TimeSpan.FromMinutes(6);
        }
    }
}