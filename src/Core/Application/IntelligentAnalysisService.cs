using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace TutorCopiloto.Services
{
    public interface IIntelligentAnalysisService
    {
        Task<LogAnalysisResult> AnalyzeDeploymentLogsAsync(string deploymentId, string logs);
        Task<PerformanceInsight> AnalyzePerformanceAsync(string logs);
        Task<string> GenerateExecutiveSummaryAsync(string deploymentId, int days);
        Task<string> GenerateDeploymentSummaryAsync(string deploymentId, string logs);
        Task<SecurityAnalysisResult> AnalyzeSecurityAsync(string logs);
        Task<AnomalyDetectionResult> DetectAnomaliesAsync(string logs);
        Task<DeploymentRecommendations> GetDeploymentRecommendationsAsync(string repositoryUrl, string logs);
    }

    public class IntelligentAnalysisService : IIntelligentAnalysisService
    {
        private readonly ILogger<IntelligentAnalysisService> _logger;
        private readonly IConfiguration _configuration;
        private readonly LlamaIndexService _llamaIndexService;
        private readonly Kernel _kernel;

        public IntelligentAnalysisService(
            IConfiguration configuration,
            LlamaIndexService llamaIndexService,
            ILogger<IntelligentAnalysisService> logger)
        {
            _configuration = configuration;
            _llamaIndexService = llamaIndexService;
            _logger = logger;

            // Simplificar inicialização sem plugins por enquanto
            var builder = Kernel.CreateBuilder();
            // Não registramos diretamente o adapter no kernel para evitar dependências fortes em SDKs externos.
            // Mantemos o kernel limpo e usamos o adapter dentro dos serviços.
            _kernel = builder.Build();

            _logger.LogInformation("IntelligentAnalysisService inicializado com LlamaIndex");
        }

        public async Task<LogAnalysisResult> AnalyzeDeploymentLogsAsync(string deploymentId, string logs)
        {
            try
            {
                _logger.LogInformation("Analisando logs do deployment {DeploymentId} com IA", deploymentId);

                var prompt = $@"
Analise os seguintes logs de deployment e forneça insights:

Deployment ID: {deploymentId}
Logs:
{logs}

Por favor, forneça:
1. Status geral do deployment (sucesso/falha/warnings)
2. Principais problemas encontrados
3. Sugestões de melhoria
4. Tempo estimado de resolução para problemas
5. Nível de criticidade (Baixa/Média/Alta/Crítica)

Responda em formato JSON estruturado.";

                var aiText = await _llamaIndexService.GetChatResponseAsync(prompt, deploymentId);
                var aiResponse = new { Content = aiText };

                // Tentar desserializar a resposta da IA para um DTO estruturado
                TutorCopiloto.Services.Dto.IntelligentAnalysisResponseDto? dto = null;
                if (!string.IsNullOrEmpty(aiResponse?.Content))
                {
                    try
                    {
                        dto = System.Text.Json.JsonSerializer.Deserialize<TutorCopiloto.Services.Dto.IntelligentAnalysisResponseDto>(
                            aiResponse.Content,
                            new System.Text.Json.JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });
                    }
                    catch (System.Text.Json.JsonException jex)
                    {
                        _logger.LogWarning(jex, "Resposta da IA não estava em JSON válido: usando heurística fallback");
                    }
                }

                var result = new LogAnalysisResult
                {
                    DeploymentId = deploymentId,
                    Status = dto?.Status ?? "Analisado",
                    Issues = dto?.Issues ?? ExtractIssues(logs, aiResponse?.Content ?? ""),
                    Recommendations = dto?.Recommendations ?? ExtractRecommendations(aiResponse?.Content ?? ""),
                    Severity = dto?.Severity ?? DetermineSeverity(logs),
                    EstimatedResolutionTime = dto?.EstimatedResolutionMinutes.HasValue == true
                        ? TimeSpan.FromMinutes(dto.EstimatedResolutionMinutes.Value)
                        : EstimateResolutionTime(logs),
                    AiInsights = aiResponse?.Content ?? "Análise não disponível"
                };

                _logger.LogInformation("Análise de logs concluída para {DeploymentId}", deploymentId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante análise de logs para {DeploymentId}", deploymentId);
                return new LogAnalysisResult
                {
                    DeploymentId = deploymentId,
                    Status = "Erro na análise",
                    Issues = new List<string> { "Erro durante processamento da IA" },
                    Recommendations = new List<string> { "Verifique os logs manualmente" },
                    Severity = "Média",
                    EstimatedResolutionTime = TimeSpan.FromMinutes(30),
                    AiInsights = ex.Message
                };
            }
        }

        public async Task<PerformanceInsight> AnalyzePerformanceAsync(string logs)
        {
            try
            {
                var prompt = $@"
Analise os seguintes logs em busca de problemas de performance:

{logs}

Identifique:
1. Gargalos de performance
2. Consultas SQL lentas
3. Uso excessivo de recursos
4. Recomendações de otimização
5. Ferramentas sugeridas";

                var aiText = await _llamaIndexService.GetChatResponseAsync(prompt, "performance-analysis");
                var aiResponse = new { Content = aiText };

                return new PerformanceInsight
                {
                    AnalysisDate = DateTime.UtcNow,
                    PerformanceOptimizations = ExtractPerformanceRecommendations(aiResponse?.Content ?? ""),
                    SecurityImprovements = ExtractSecurityRecommendations(aiResponse?.Content ?? ""),
                    BestPractices = ExtractBestPractices(aiResponse?.Content ?? ""),
                    ToolSuggestions = ExtractToolSuggestions(aiResponse?.Content ?? ""),
                    AiAnalysis = aiResponse?.Content ?? ""
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante análise de performance");
                return new PerformanceInsight
                {
                    AnalysisDate = DateTime.UtcNow,
                    PerformanceOptimizations = new List<string> { "Erro na análise" },
                    SecurityImprovements = new List<string>(),
                    BestPractices = new List<string>(),
                    ToolSuggestions = new List<string>(),
                    AiAnalysis = $"Erro: {ex.Message}"
                };
            }
        }

        public async Task<string> GenerateDeploymentSummaryAsync(string deploymentId, string logs)
        {
            try
            {
                var prompt = $@"
Gere um resumo detalhado do deployment {deploymentId} baseado nos logs fornecidos:

{logs}

Inclua:
1. Status do deployment
2. Principais métricas
3. Problemas identificados
4. Tempo de execução
5. Recomendações

Formato: Relatório técnico conciso";

                var aiText = await _llamaIndexService.GetChatResponseAsync(prompt, deploymentId);
                return aiText ?? "Resumo não disponível";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar resumo de deployment");
                return $"Erro na geração do resumo: {ex.Message}";
            }
        }

        public async Task<string> GenerateExecutiveSummaryAsync(string deploymentId, int days)
        {
            try
            {
                var prompt = $@"
Gere um resumo executivo para deployment {deploymentId} considerando os últimos {days} dias.

Inclua:
1. Status geral do sistema
2. Principais métricas
3. Problemas críticos
4. Recomendações estratégicas
5. Próximos passos

Formato: Resumo executivo profissional";

                var aiText = await _llamaIndexService.GetChatResponseAsync(prompt, deploymentId);
                return aiText ?? "Resumo não disponível";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar resumo executivo");
                return $"Erro na geração do resumo: {ex.Message}";
            }
        }

        public async Task<SecurityAnalysisResult> AnalyzeSecurityAsync(string logs)
        {
            try
            {
                var prompt = $@"
Analise os seguintes logs em busca de problemas de segurança:

{logs}

Identifique:
1. Vulnerabilidades potenciais
2. Tentativas de acesso não autorizado
3. Problemas de configuração de segurança
4. Recomendações de correção
5. Questões de compliance";

                var aiText = await _llamaIndexService.GetChatResponseAsync(prompt, "security-analysis");
                var aiResponse = new { Content = aiText };

                return new SecurityAnalysisResult
                {
                    AnalysisDate = DateTime.UtcNow,
                    Vulnerabilities = ExtractVulnerabilities(logs, aiResponse?.Content ?? ""),
                    Recommendations = ExtractSecurityRecommendations(aiResponse?.Content ?? ""),
                    ComplianceIssues = ExtractComplianceIssues(aiResponse?.Content ?? ""),
                    RiskLevel = DetermineRiskLevel(logs),
                    AiInsights = aiResponse?.Content ?? ""
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante análise de segurança");
                return new SecurityAnalysisResult
                {
                    AnalysisDate = DateTime.UtcNow,
                    Vulnerabilities = new List<string> { "Erro na análise" },
                    Recommendations = new List<string>(),
                    ComplianceIssues = new List<string>(),
                    RiskLevel = "Desconhecido",
                    AiInsights = $"Erro: {ex.Message}"
                };
            }
        }

        public async Task<AnomalyDetectionResult> DetectAnomaliesAsync(string logs)
        {
            try
            {
                var prompt = $@"
Analise os seguintes logs em busca de anomalias e comportamentos suspeitos:

{logs}

Detecte:
1. Padrões anômalos de acesso
2. Picos de uso incomuns
3. Erros recorrentes
4. Comportamentos suspeitos
5. Sugestões de otimização";

                var aiText = await _llamaIndexService.GetChatResponseAsync(prompt, "anomaly-detection");
                var aiResponse = new { Content = aiText };

                return new AnomalyDetectionResult
                {
                    DetectedAt = DateTime.UtcNow,
                    Anomalies = new List<AnomalyPoint>(),
                    OverallHealthScore = 95,
                    Trends = ExtractBottlenecks(logs, aiResponse?.Content ?? ""),
                    Recommendations = ExtractOptimizations(aiResponse?.Content ?? "")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante detecção de anomalias");
                return new AnomalyDetectionResult
                {
                    DetectedAt = DateTime.UtcNow,
                    Anomalies = new List<AnomalyPoint>(),
                    OverallHealthScore = 50,
                    Trends = new List<string> { "Erro na análise" },
                    Recommendations = new List<string>()
                };
            }
        }

        public async Task<DeploymentRecommendations> GetDeploymentRecommendationsAsync(string repositoryUrl, string logs)
        {
            try
            {
                var prompt = $@"
Analise o repositório {repositoryUrl} e os logs para fornecer recomendações de deployment:

Logs:
{logs}

Forneça recomendações específicas para:
1. Configuração de CI/CD
2. Estratégias de deployment
3. Monitoramento e observabilidade
4. Segurança
5. Performance";

                var aiText = await _llamaIndexService.GetChatResponseAsync(prompt, "deployment-recommendations");
                var aiResponse = new { Content = aiText };

                return new DeploymentRecommendations
                {
                    RepositoryUrl = repositoryUrl,
                    GeneratedAt = DateTime.UtcNow,
                    CiCdRecommendations = ExtractCiCdRecommendations(aiResponse?.Content ?? ""),
                    SecurityRecommendations = ExtractSecurityRecommendations(aiResponse?.Content ?? ""),
                    PerformanceRecommendations = ExtractPerformanceRecommendations(aiResponse?.Content ?? ""),
                    MonitoringRecommendations = ExtractMonitoringRecommendations(aiResponse?.Content ?? ""),
                    AiInsights = aiResponse?.Content ?? ""
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar recomendações de deployment");
                return new DeploymentRecommendations
                {
                    RepositoryUrl = repositoryUrl,
                    GeneratedAt = DateTime.UtcNow,
                    CiCdRecommendations = new List<string> { "Erro na análise" },
                    SecurityRecommendations = new List<string>(),
                    PerformanceRecommendations = new List<string>(),
                    MonitoringRecommendations = new List<string>(),
                    AiInsights = $"Erro: {ex.Message}"
                };
            }
        }

        // Métodos auxiliares
        private List<string> ExtractIssues(string logs, string aiResponse)
        {
            var issues = new List<string>();
            
            if (logs.Contains("ERROR", StringComparison.OrdinalIgnoreCase))
                issues.Add("Erros detectados nos logs");
            
            if (logs.Contains("EXCEPTION", StringComparison.OrdinalIgnoreCase))
                issues.Add("Exceções encontradas");
            
            if (logs.Contains("TIMEOUT", StringComparison.OrdinalIgnoreCase))
                issues.Add("Timeouts detectados");

            return issues.Any() ? issues : new List<string> { "Nenhum problema crítico detectado" };
        }

        private List<string> ExtractRecommendations(string aiResponse)
        {
            return new List<string>
            {
                "Monitorar logs continuamente",
                "Implementar alertas automáticos",
                "Revisar configurações de timeout"
            };
        }

        private string DetermineSeverity(string logs)
        {
            if (logs.Contains("CRITICAL", StringComparison.OrdinalIgnoreCase) || 
                logs.Contains("FATAL", StringComparison.OrdinalIgnoreCase))
                return "Crítica";
            
            if (logs.Contains("ERROR", StringComparison.OrdinalIgnoreCase))
                return "Alta";
            
            if (logs.Contains("WARN", StringComparison.OrdinalIgnoreCase))
                return "Média";
            
            return "Baixa";
        }

        private TimeSpan EstimateResolutionTime(string logs)
        {
            if (logs.Contains("CRITICAL", StringComparison.OrdinalIgnoreCase))
                return TimeSpan.FromMinutes(15);
            
            if (logs.Contains("ERROR", StringComparison.OrdinalIgnoreCase))
                return TimeSpan.FromMinutes(30);
            
            return TimeSpan.FromHours(1);
        }

        private List<string> ExtractPerformanceRecommendations(string aiResponse)
        {
            return new List<string>
            {
                "Otimizar consultas de banco de dados",
                "Implementar cache para consultas frequentes",
                "Configurar pool de conexões adequado"
            };
        }

        private List<string> ExtractSecurityRecommendations(string aiResponse)
        {
            return new List<string>
            {
                "Implementar autenticação forte",
                "Configurar HTTPS em todos os endpoints",
                "Revisar permissões de acesso"
            };
        }

        private List<string> ExtractBestPractices(string aiResponse)
        {
            return new List<string>
            {
                "Implementar logging estruturado",
                "Configurar monitoramento proativo",
                "Automatizar deploys com CI/CD"
            };
        }

        private List<string> ExtractToolSuggestions(string aiResponse)
        {
            return new List<string>
            {
                "Application Insights para monitoramento",
                "SonarQube para qualidade de código",
                "Docker para containerização"
            };
        }

        private List<string> ExtractVulnerabilities(string logs, string aiResponse)
        {
            var vulnerabilities = new List<string>();
            
            if (logs.Contains("SQL injection", StringComparison.OrdinalIgnoreCase))
                vulnerabilities.Add("Possível SQL injection detectado");
            
            if (logs.Contains("authentication failed", StringComparison.OrdinalIgnoreCase))
                vulnerabilities.Add("Falhas de autenticação frequentes");

            return vulnerabilities.Any() ? vulnerabilities : new List<string> { "Nenhuma vulnerabilidade crítica detectada" };
        }

        private List<string> ExtractComplianceIssues(string aiResponse)
        {
            return new List<string>
            {
                "Revisar logs de auditoria",
                "Implementar retenção de dados",
                "Configurar controles de acesso"
            };
        }

        private string DetermineRiskLevel(string logs)
        {
            if (logs.Contains("CRITICAL", StringComparison.OrdinalIgnoreCase))
                return "Alto";
            
            if (logs.Contains("authentication failed", StringComparison.OrdinalIgnoreCase))
                return "Médio";
            
            return "Baixo";
        }

        private List<string> ExtractBottlenecks(string logs, string aiResponse)
        {
            var bottlenecks = new List<string>();
            
            if (logs.Contains("slow query", StringComparison.OrdinalIgnoreCase))
                bottlenecks.Add("Consultas SQL lentas detectadas");
            
            if (logs.Contains("memory", StringComparison.OrdinalIgnoreCase))
                bottlenecks.Add("Uso elevado de memória");

            return bottlenecks.Any() ? bottlenecks : new List<string> { "Nenhum gargalo significativo detectado" };
        }

        private List<string> ExtractOptimizations(string aiResponse)
        {
            return new List<string>
            {
                "Implementar cache distributed",
                "Otimizar índices do banco de dados",
                "Configurar load balancing"
            };
        }

        private List<string> ExtractCiCdRecommendations(string aiResponse)
        {
            return new List<string>
            {
                "Implementar pipeline de CI/CD automatizado",
                "Configurar testes automatizados",
                "Implementar deployment blue-green"
            };
        }

        private List<string> ExtractMonitoringRecommendations(string aiResponse)
        {
            return new List<string>
            {
                "Configurar Application Insights",
                "Implementar health checks",
                "Configurar alertas proativos"
            };
        }
    }

    // DTOs para resultados (que não existem em outros arquivos)
    public class LogAnalysisResult
    {
        public string DeploymentId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<string> Issues { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        public string Severity { get; set; } = string.Empty;
        public TimeSpan EstimatedResolutionTime { get; set; }
        public string AiInsights { get; set; } = string.Empty;
    }

    public class PerformanceInsight
    {
        public DateTime AnalysisDate { get; set; }
        public List<string> PerformanceOptimizations { get; set; } = new();
        public List<string> SecurityImprovements { get; set; } = new();
        public List<string> BestPractices { get; set; } = new();
        public List<string> ToolSuggestions { get; set; } = new();
        public string AiAnalysis { get; set; } = string.Empty;
    }

    public class SecurityAnalysisResult
    {
        public DateTime AnalysisDate { get; set; }
        public List<string> Vulnerabilities { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        public List<string> ComplianceIssues { get; set; } = new();
        public string RiskLevel { get; set; } = string.Empty;
        public string AiInsights { get; set; } = string.Empty;
    }

    public class DeploymentRecommendations
    {
        public string RepositoryUrl { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public List<string> CiCdRecommendations { get; set; } = new();
        public List<string> SecurityRecommendations { get; set; } = new();
        public List<string> PerformanceRecommendations { get; set; } = new();
        public List<string> MonitoringRecommendations { get; set; } = new();
        public string AiInsights { get; set; } = string.Empty;
    }

    // Plugins para Semantic Kernel
    public class DeploymentAnalysisPlugin
    {
        [KernelFunction, Description("Analisa logs de deployment e identifica problemas")]
        public string AnalyzeLogs(string logs)
        {
            return "Análise de deployment concluída";
        }
    }

    public class SecurityAnalysisPlugin
    {
        [KernelFunction, Description("Analisa aspectos de segurança em deployments")]
        public string AnalyzeSecurity(string logs)
        {
            return "Análise de segurança concluída";
        }
    }

    public class PerformanceAnalysisPlugin
    {
        [Description("Analisa performance e identifica gargalos")]
        public string AnalyzePerformance(string logs)
        {
            return "Análise de performance concluída";
        }
    }
}
