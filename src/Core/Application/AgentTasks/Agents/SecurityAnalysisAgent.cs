using Microsoft.Extensions.Logging;
using TutorCopiloto.Services;

namespace TutorCopiloto.Services.AgentTasks.Agents
{
    /// <summary>
    /// Agente para análise de segurança
    /// </summary>
    public class SecurityAnalysisAgent : BaseAgentTask
    {
        private readonly IAIService _aiService;

        public SecurityAnalysisAgent(IAIService aiService, ILogger<SecurityAnalysisAgent> logger) : base(logger)
        {
            _aiService = aiService;
        }

        public override string AgentName => "Security Analysis Agent";
        public override string Description => "Analisa vulnerabilidades de segurança, credenciais expostas e configurações inseguras";
        public override int Priority => 2; // Alta prioridade para segurança

        protected override async Task<bool> CanExecuteInternalAsync(string repositoryPath, RepositoryAnalysisContext context)
        {
            // Sempre pode executar se houver arquivos
            var files = Directory.GetFiles(repositoryPath, "*", SearchOption.AllDirectories);
            return files.Any() && await _aiService.IsAvailableAsync();
        }

        protected override async Task<AgentTaskResult> ExecuteInternalAsync(string repositoryPath, RepositoryAnalysisContext context)
        {
            var result = new AgentTaskResult
            {
                Summary = "Análise de segurança concluída"
            };

            // Análise de configurações
            await AnalyzeConfigurationSecurity(repositoryPath, result);

            // Análise de dependências
            await AnalyzeDependencySecurity(repositoryPath, result);

            // Análise de credenciais expostas
            await AnalyzeExposedCredentials(repositoryPath, result);

            // Análise de arquivos sensíveis
            await AnalyzeSensitiveFiles(repositoryPath, result);

            // Análise de código para padrões inseguros
            await AnalyzeCodeSecurityPatterns(repositoryPath, result, context);

            // Adiciona recomendações gerais de segurança
            AddSecurityRecommendations(result);

            result.Summary = $"Análise de segurança concluída. Encontrados {result.Findings.Count} problemas de segurança.";
            return result;
        }

        private async Task AnalyzeConfigurationSecurity(string repositoryPath, AgentTaskResult result)
        {
            // Verifica arquivos de configuração
            var configFiles = new[]
            {
                "appsettings.json", "appsettings.*.json", "web.config", "app.config",
                ".env", ".env.*", "config.json", "config.js", "package.json"
            };

            foreach (var pattern in configFiles)
            {
                var files = FindFiles(repositoryPath, pattern);
                foreach (var file in files)
                {
                    await AnalyzeConfigFile(file, repositoryPath, result);
                }
            }
        }

        private async Task AnalyzeConfigFile(string filePath, string repositoryPath, AgentTaskResult result)
        {
            try
            {
                var content = await File.ReadAllTextAsync(filePath);
                var relativePath = GetRelativePath(repositoryPath, filePath);
                var lines = content.Split('\n');

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    var lineNumber = i + 1;

                    // Verifica credenciais hardcoded
                    if (ContainsCredentialPattern(line))
                    {
                        result.Findings.Add(CreateFinding(
                            "Security", "High", "Possível credencial em arquivo de configuração",
                            "Detectada possível credencial ou chave de API em arquivo de configuração",
                            relativePath, lineNumber, "SEC_CONFIG_001"));
                    }

                    // Verifica configurações inseguras
                    if (ContainsInsecureConfig(line))
                    {
                        result.Findings.Add(CreateFinding(
                            "Security", "Medium", "Configuração potencialmente insegura",
                            "Detectada configuração que pode representar um risco de segurança",
                            relativePath, lineNumber, "SEC_CONFIG_002"));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Erro ao analisar arquivo de configuração {File}", filePath);
            }
        }

        private async Task AnalyzeDependencySecurity(string repositoryPath, AgentTaskResult result)
        {
            // Verifica package.json para dependências JavaScript/Node
            var packageJsonFiles = FindFiles(repositoryPath, "package.json");
            foreach (var file in packageJsonFiles)
            {
                await AnalyzePackageJson(file, repositoryPath, result);
            }

            // Verifica arquivos .csproj para dependências .NET
            var csprojFiles = FindFiles(repositoryPath, "*.csproj");
            foreach (var file in csprojFiles)
            {
                await AnalyzeCsProj(file, repositoryPath, result);
            }

            // Verifica requirements.txt para dependências Python
            var reqFiles = FindFiles(repositoryPath, "requirements.txt");
            foreach (var file in reqFiles)
            {
                await AnalyzeRequirementsTxt(file, repositoryPath, result);
            }
        }

        private async Task AnalyzePackageJson(string filePath, string repositoryPath, AgentTaskResult result)
        {
            try
            {
                var content = await File.ReadAllTextAsync(filePath);
                var relativePath = GetRelativePath(repositoryPath, filePath);

                // Verifica se há versões específicas (não usando ^, ~, latest)
                if (content.Contains("\"latest\"") || content.Contains("\"*\""))
                {
                    result.Findings.Add(CreateFinding(
                        "Security", "Medium", "Versões de dependência não fixadas",
                        "Usar 'latest' ou '*' para versões pode introduzir vulnerabilidades não testadas",
                        relativePath, 0, "SEC_DEP_001"));
                }

                // Verifica dependências conhecidamente problemáticas
                var problematicPackages = new[] { "lodash", "moment", "request" };
                foreach (var package in problematicPackages)
                {
                    if (content.Contains($"\"{package}\""))
                    {
                        result.Findings.Add(CreateFinding(
                            "Security", "Low", $"Dependência {package} pode ter vulnerabilidades",
                            $"O pacote {package} teve vulnerabilidades conhecidas. Considere alternativas mais seguras",
                            relativePath, 0, "SEC_DEP_002"));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Erro ao analisar package.json {File}", filePath);
            }
        }

        private async Task AnalyzeCsProj(string filePath, string repositoryPath, AgentTaskResult result)
        {
            try
            {
                var content = await File.ReadAllTextAsync(filePath);
                var relativePath = GetRelativePath(repositoryPath, filePath);

                // Verifica versões antigas do .NET
                if (content.Contains("netcoreapp2.") || content.Contains("netcoreapp3.0"))
                {
                    result.Findings.Add(CreateFinding(
                        "Security", "Medium", "Versão antiga do .NET detectada",
                        "Versões antigas do .NET Core podem conter vulnerabilidades. Considere atualizar",
                        relativePath, 0, "SEC_NET_001"));
                }

                // Verifica referências inseguras
                if (content.Contains("AllowUnsafeBlocks"))
                {
                    result.Findings.Add(CreateFinding(
                        "Security", "High", "Blocos de código inseguro habilitados",
                        "AllowUnsafeBlocks pode introduzir vulnerabilidades de segurança",
                        relativePath, 0, "SEC_NET_002"));
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Erro ao analisar .csproj {File}", filePath);
            }
        }

        private async Task AnalyzeRequirementsTxt(string filePath, string repositoryPath, AgentTaskResult result)
        {
            try
            {
                var content = await File.ReadAllTextAsync(filePath);
                var relativePath = GetRelativePath(repositoryPath, filePath);

                // Verifica se há versões específicas
                var lines = content.Split('\n');
                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line) && !line.Contains("==") && !line.Contains(">="))
                    {
                        result.Findings.Add(CreateFinding(
                            "Security", "Medium", "Versão de dependência Python não fixada",
                            $"Dependência sem versão específica: {line.Trim()}",
                            relativePath, 0, "SEC_PY_001"));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Erro ao analisar requirements.txt {File}", filePath);
            }
        }

        private async Task AnalyzeExposedCredentials(string repositoryPath, AgentTaskResult result)
        {
            var sensitiveFiles = FindFiles(repositoryPath, "*", true)
                .Where(f => Path.GetFileName(f).ToLower().Contains("secret") ||
                           Path.GetFileName(f).ToLower().Contains("key") ||
                           Path.GetFileName(f).ToLower().Contains("password") ||
                           Path.GetFileName(f).ToLower().Contains("token"))
                .ToList();

            foreach (var file in sensitiveFiles)
            {
                var relativePath = GetRelativePath(repositoryPath, file);
                result.Findings.Add(CreateFinding(
                    "Security", "High", "Arquivo com nome sensível detectado",
                    $"Arquivo com nome que sugere conteúdo sensível: {relativePath}",
                    relativePath, 0, "SEC_FILE_001"));
            }
        }

        private async Task AnalyzeSensitiveFiles(string repositoryPath, AgentTaskResult result)
        {
            var sensitivePatterns = new[]
            {
                "*.pem", "*.key", "*.p12", "*.pfx", "*.jks",
                ".env", ".env.*", "*.properties", "id_rsa*"
            };

            foreach (var pattern in sensitivePatterns)
            {
                var files = FindFiles(repositoryPath, pattern);
                foreach (var file in files)
                {
                    var relativePath = GetRelativePath(repositoryPath, file);
                    result.Findings.Add(CreateFinding(
                        "Security", "Critical", "Arquivo potencialmente sensível encontrado",
                        $"Arquivo que pode conter credenciais ou chaves privadas: {relativePath}",
                        relativePath, 0, "SEC_SENSITIVE_001"));
                }
            }
        }

        private async Task AnalyzeCodeSecurityPatterns(string repositoryPath, AgentTaskResult result, RepositoryAnalysisContext context)
        {
            var codeFiles = FindFiles(repositoryPath, "*.cs");
            foreach (var file in codeFiles.Take(20)) // Limita análise
            {
                await AnalyzeCodeFileForSecurity(file, repositoryPath, result);
            }
        }

        private async Task AnalyzeCodeFileForSecurity(string filePath, string repositoryPath, AgentTaskResult result)
        {
            try
            {
                var content = await File.ReadAllTextAsync(filePath);
                var relativePath = GetRelativePath(repositoryPath, filePath);
                var lines = content.Split('\n');

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    var lineNumber = i + 1;

                    CheckSecurityPatterns(line, lineNumber, relativePath, result);
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Erro ao analisar arquivo de código {File}", filePath);
            }
        }

        private void CheckSecurityPatterns(string line, int lineNumber, string relativePath, AgentTaskResult result)
        {
            // SQL Injection patterns
            if (line.Contains("CommandText") && line.Contains("+"))
            {
                result.Findings.Add(CreateFinding(
                    "Security", "High", "Possível vulnerabilidade de SQL Injection",
                    "Concatenação de strings em comandos SQL pode levar a SQL injection",
                    relativePath, lineNumber, "SEC_SQL_001"));
            }

            // XSS patterns
            if (line.Contains("Html.Raw") && !line.Contains("HttpUtility.HtmlEncode"))
            {
                result.Findings.Add(CreateFinding(
                    "Security", "High", "Possível vulnerabilidade XSS",
                    "Uso de Html.Raw sem encoding pode levar a XSS",
                    relativePath, lineNumber, "SEC_XSS_001"));
            }

            // Weak encryption
            if (line.Contains("MD5") || line.Contains("SHA1"))
            {
                result.Findings.Add(CreateFinding(
                    "Security", "Medium", "Algoritmo de hash fraco",
                    "MD5 e SHA1 são considerados criptograficamente fracos",
                    relativePath, lineNumber, "SEC_CRYPTO_001"));
            }

            // Insecure random
            if (line.Contains("new Random()"))
            {
                result.Findings.Add(CreateFinding(
                    "Security", "Medium", "Gerador de números aleatórios inseguro",
                    "Random() não é criptograficamente seguro. Use RNGCryptoServiceProvider",
                    relativePath, lineNumber, "SEC_RANDOM_001"));
            }
        }

        private void AddSecurityRecommendations(AgentTaskResult result)
        {
            result.Recommendations.Add(CreateRecommendation(
                "Security", "High", "Implementar análise de segurança automatizada",
                "Configure ferramentas como OWASP Dependency Check, Snyk ou GitHub Security Advisories",
                new List<string>
                {
                    "Configurar GitHub Dependabot",
                    "Implementar OWASP Dependency Check",
                    "Configurar análise de segurança no CI/CD",
                    "Implementar revisão de segurança obrigatória"
                }));

            result.Recommendations.Add(CreateRecommendation(
                "Security", "Medium", "Estabelecer práticas de segurança",
                "Implemente políticas e práticas de segurança para desenvolvimento",
                new List<string>
                {
                    "Criar política de senhas e credenciais",
                    "Implementar rotação de chaves/tokens",
                    "Configurar autenticação multifator",
                    "Estabelecer processo de resposta a incidentes"
                }));
        }

        private bool ContainsCredentialPattern(string line)
        {
            var credentialPatterns = new[]
            {
                "password", "secret", "key", "token", "api_key", "private_key",
                "access_token", "refresh_token", "client_secret"
            };

            return credentialPatterns.Any(pattern =>
                line.Contains(pattern, StringComparison.OrdinalIgnoreCase) &&
                (line.Contains("=") || line.Contains(":")) &&
                !line.TrimStart().StartsWith("//") &&
                !line.TrimStart().StartsWith("*"));
        }

        private bool ContainsInsecureConfig(string line)
        {
            var insecurePatterns = new[]
            {
                "ssl.*false", "https.*false", "secure.*false",
                "validate.*false", "verify.*false"
            };

            return insecurePatterns.Any(pattern =>
                System.Text.RegularExpressions.Regex.IsMatch(line, pattern, 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase));
        }

        public override TimeSpan EstimateExecutionTime()
        {
            return TimeSpan.FromMinutes(8);
        }
    }
}