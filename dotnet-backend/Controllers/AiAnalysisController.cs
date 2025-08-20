using Microsoft.AspNetCore.Mvc;
using TutorCopiloto.Services;

namespace TutorCopiloto.Controllers
{
    [ApiController]
    [Route("api/ai-analysis")]
    public class AiAnalysisController : ControllerBase
    {
        private readonly IIntelligentAnalysisService _intelligentAnalysis;
        private readonly IOnnxInferenceService _onnxInference;
        private readonly IDeploymentService _deploymentService;
        private readonly ILogger<AiAnalysisController> _logger;

        public AiAnalysisController(
            IIntelligentAnalysisService intelligentAnalysis,
            IOnnxInferenceService onnxInference,
            IDeploymentService deploymentService,
            ILogger<AiAnalysisController> logger)
        {
            _intelligentAnalysis = intelligentAnalysis;
            _onnxInference = onnxInference;
            _deploymentService = deploymentService;
            _logger = logger;
        }

        /// <summary>
        /// Analisa logs de deployment usando IA
        /// </summary>
        [HttpPost("analyze-logs/{deploymentId}")]
        public async Task<ActionResult<LogAnalysisResult>> AnalyzeLogs(string deploymentId)
        {
            try
            {
                var deployment = _deploymentService.GetDeployment(deploymentId);
                if (deployment == null)
                {
                    return NotFound($"Deployment {deploymentId} não encontrado");
                }

                var logs = _deploymentService.GetLogs(deploymentId);
                var logsText = string.Join("\n", logs.Select(l => $"[{l.Timestamp:HH:mm:ss}] {l.Message}"));

                var analysis = await _intelligentAnalysis.AnalyzeDeploymentLogsAsync(deploymentId, logsText);
                
                _logger.LogInformation("Análise de logs concluída para deployment {DeploymentId}", deploymentId);
                
                return Ok(analysis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao analisar logs do deployment {DeploymentId}", deploymentId);
                return StatusCode(500, new { message = "Erro interno do servidor", error = ex.Message });
            }
        }

        /// <summary>
        /// Prediz o resultado de um deployment usando ML
        /// </summary>
        [HttpPost("predict-deployment")]
        public async Task<ActionResult<DeploymentPrediction>> PredictDeployment([FromBody] DeploymentFeatures features)
        {
            try
            {
                if (features == null)
                {
                    return BadRequest("Features do deployment são obrigatórias");
                }

                var prediction = await _onnxInference.PredictDeploymentOutcomeAsync(features);
                
                _logger.LogInformation("Predição de deployment gerada com probabilidade de sucesso: {Probability:P}", 
                    prediction.SuccessProbability);
                
                return Ok(prediction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao predizer resultado do deployment");
                return StatusCode(500, new { message = "Erro interno do servidor", error = ex.Message });
            }
        }

        /// <summary>
        /// Gera recomendações para otimização de deployment
        /// </summary>
        [HttpPost("recommendations")]
        public async Task<ActionResult<DeploymentRecommendations>> GetRecommendations([FromBody] RecommendationRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.RepositoryUrl))
                {
                    return BadRequest("URL do repositório é obrigatória");
                }

                // Buscar logs recentes do repositório
                var recentDeployments = _deploymentService.GetDeployments()
                    .Where(d => d.RepositoryUrl == request.RepositoryUrl)
                    .Take(5)
                    .ToList();

                var logs = string.Join("\n", recentDeployments
                    .SelectMany(d => _deploymentService.GetLogs(d.Id))
                    .Select(l => $"[{l.Timestamp:HH:mm:ss}] {l.Message}"));

                var recommendations = await _intelligentAnalysis.GetDeploymentRecommendationsAsync(request.RepositoryUrl, logs);
                
                _logger.LogInformation("Recomendações geradas para repositório {Repository}", request.RepositoryUrl);
                
                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar recomendações para {Repository}", request?.RepositoryUrl);
                return StatusCode(500, new { message = "Erro interno do servidor", error = ex.Message });
            }
        }

        /// <summary>
        /// Detecta anomalias em deployments históricos
        /// </summary>
        [HttpGet("anomalies")]
        public async Task<ActionResult<AnomalyDetectionResult>> DetectAnomalies([FromQuery] int days = 30)
        {
            try
            {
                var deployments = _deploymentService.GetDeployments()
                    .Where(d => d.CreatedAt >= DateTime.UtcNow.AddDays(-days))
                    .ToList();

                var metrics = deployments.Select(d => new DeploymentMetrics
                {
                    Timestamp = d.CreatedAt,
                    DeploymentDuration = d.Duration ?? TimeSpan.FromMinutes(5),
                    CpuUsage = Random.Shared.NextDouble() * 100, // Simulado
                    MemoryUsage = Random.Shared.NextDouble() * 100, // Simulado
                    DiskUsage = Random.Shared.NextDouble() * 100, // Simulado
                    Success = d.Status == DeploymentStatus.Success
                }).ToList();

                var anomalyResult = await _onnxInference.DetectAnomaliesAsync(metrics);
                
                _logger.LogInformation("Detecção de anomalias concluída para {Days} dias - {AnomaliesCount} anomalias encontradas", 
                    days, anomalyResult.Anomalies.Count);
                
                return Ok(anomalyResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao detectar anomalias");
                return StatusCode(500, new { message = "Erro interno do servidor", error = ex.Message });
            }
        }

        /// <summary>
        /// Analisa segurança de um deployment
        /// </summary>
        [HttpPost("security-analysis/{deploymentId}")]
        public async Task<ActionResult<SecurityThreatAssessment>> AnalyzeSecurity(string deploymentId)
        {
            try
            {
                var deployment = _deploymentService.GetDeployment(deploymentId);
                if (deployment == null)
                {
                    return NotFound($"Deployment {deploymentId} não encontrado");
                }

                var logs = _deploymentService.GetLogs(deploymentId);
                var logsText = string.Join("\n", logs.Select(l => $"[{l.Timestamp:HH:mm:ss}] {l.Message}"));

                // Extrair features de segurança dos logs (simulado)
                var securityFeatures = new SecurityFeatures
                {
                    ExposedPorts = ExtractExposedPorts(logsText),
                    HasHttpEndpoints = logsText.Contains("http://"),
                    HasStrongAuthentication = logsText.Contains("authentication") && logsText.Contains("token"),
                    HasEncryption = logsText.Contains("https://") || logsText.Contains("ssl"),
                    InstalledPackages = ExtractPackages(logsText)
                };

                var securityAssessment = await _onnxInference.AssessSecurityThreatsAsync(securityFeatures);
                var intelligentAnalysis = await _intelligentAnalysis.AnalyzeSecurityAsync(logsText);

                // Combinar resultados
                var combinedAssessment = new SecurityThreatAssessment
                {
                    Threats = securityAssessment.Threats.Concat(
                        intelligentAnalysis.Vulnerabilities.Select(v => new SecurityThreat
                        {
                            ThreatType = "AI Detected",
                            Severity = ThreatSeverity.Medium,
                            Description = v,
                            Recommendation = "Revise manualmente",
                            CvssScore = 5.0
                        })).ToList(),
                    OverallRiskScore = 75, // Valor padrão baseado na análise
                    ComplianceStatus = securityAssessment.ComplianceStatus,
                    MitigationPlan = securityAssessment.MitigationPlan
                        .Concat(intelligentAnalysis.Recommendations).ToList(),
                    AssessedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Análise de segurança concluída para deployment {DeploymentId} - Risk Score: {RiskScore}", 
                    deploymentId, combinedAssessment.OverallRiskScore);

                return Ok(combinedAssessment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao analisar segurança do deployment {DeploymentId}", deploymentId);
                return StatusCode(500, new { message = "Erro interno do servidor", error = ex.Message });
            }
        }

        /// <summary>
        /// Otimiza recursos baseado em dados históricos
        /// </summary>
        [HttpPost("optimize-resources")]
        public async Task<ActionResult<ResourceOptimizationResult>> OptimizeResources([FromBody] ResourceUsageData currentUsage)
        {
            try
            {
                if (currentUsage == null)
                {
                    return BadRequest("Dados de uso de recursos são obrigatórios");
                }

                var optimization = await _onnxInference.OptimizeResourcesAsync(currentUsage);
                
                _logger.LogInformation("Otimização de recursos concluída - {OptimizationsCount} otimizações sugeridas", 
                    optimization.Optimizations.Count);
                
                return Ok(optimization);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao otimizar recursos");
                return StatusCode(500, new { message = "Erro interno do servidor", error = ex.Message });
            }
        }

        /// <summary>
        /// Gera resumo executivo de deployments recentes
        /// </summary>
        [HttpGet("executive-summary")]
        public async Task<ActionResult<ExecutiveSummary>> GetExecutiveSummary([FromQuery] int days = 7)
        {
            try
            {
                var recentDeployments = _deploymentService.GetDeployments()
                    .Where(d => d.CreatedAt >= DateTime.UtcNow.AddDays(-days))
                    .ToList();

                var summaryText = await _intelligentAnalysis.GenerateDeploymentSummaryAsync(
                    recentDeployments.FirstOrDefault()?.Id ?? "recent", 
                    $"Análise de {recentDeployments.Count} deployments dos últimos {days} dias");

                // Calcular métricas
                var totalDeployments = recentDeployments.Count;
                var successfulDeployments = recentDeployments.Count(d => d.Status == DeploymentStatus.Success);
                var failedDeployments = recentDeployments.Count(d => d.Status == DeploymentStatus.Failed);
                var avgDuration = recentDeployments.Where(d => d.Duration.HasValue)
                    .Select(d => d.Duration!.Value.TotalMinutes).DefaultIfEmpty(0).Average();

                var summary = new ExecutiveSummary
                {
                    Period = $"Últimos {days} dias",
                    TotalDeployments = totalDeployments,
                    SuccessfulDeployments = successfulDeployments,
                    FailedDeployments = failedDeployments,
                    SuccessRate = totalDeployments > 0 ? (double)successfulDeployments / totalDeployments : 0,
                    AverageDuration = TimeSpan.FromMinutes(avgDuration),
                    AiSummary = summaryText,
                    GeneratedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Resumo executivo gerado para {Days} dias - {Total} deployments", days, totalDeployments);

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar resumo executivo");
                return StatusCode(500, new { message = "Erro interno do servidor", error = ex.Message });
            }
        }

        /// <summary>
        /// Treina modelos ML com dados históricos
        /// </summary>
        [HttpPost("train-models")]
        public async Task<ActionResult> TrainModels([FromQuery] int days = 90)
        {
            try
            {
                var historicalDeployments = _deploymentService.GetDeployments()
                    .Where(d => d.CreatedAt >= DateTime.UtcNow.AddDays(-days))
                    .Select(d => new HistoricalDeployment
                    {
                        Id = d.Id,
                        Timestamp = d.CreatedAt,
                        Success = d.Status == DeploymentStatus.Success,
                        Duration = d.Duration ?? TimeSpan.FromMinutes(5),
                        RepositoryUrl = d.RepositoryUrl,
                        Features = new DeploymentFeatures
                        {
                            RepositorySize = Random.Shared.Next(100, 2000), // Simulado
                            Dependencies = new List<string>(), // Seria extraído dos logs
                            HasTests = Random.Shared.NextDouble() > 0.5,
                            HasDockerfile = Random.Shared.NextDouble() > 0.3,
                            PreviousFailures = Random.Shared.Next(0, 5),
                            LastSuccessfulDeploymentDays = Random.Shared.Next(1, 30)
                        }
                    }).ToList();

                await _onnxInference.TrainModelFromHistoricalDataAsync(historicalDeployments);

                _logger.LogInformation("Treinamento de modelos iniciado com {Count} registros históricos", historicalDeployments.Count);

                return Ok(new { 
                    message = "Treinamento de modelos iniciado com sucesso",
                    trainingDataCount = historicalDeployments.Count,
                    period = $"Últimos {days} dias"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao iniciar treinamento de modelos");
                return StatusCode(500, new { message = "Erro interno do servidor", error = ex.Message });
            }
        }

        // Helper methods
        private List<int> ExtractExposedPorts(string logs)
        {
            var ports = new List<int>();
            var portMatches = System.Text.RegularExpressions.Regex.Matches(logs, @"port\s+(\d+)");
            
            foreach (System.Text.RegularExpressions.Match match in portMatches)
            {
                if (int.TryParse(match.Groups[1].Value, out int port))
                {
                    ports.Add(port);
                }
            }

            // Adicionar portas comuns se não encontradas
            if (!ports.Any())
            {
                ports.AddRange(new[] { 80, 443, 5000, 8080 });
            }

            return ports.Distinct().ToList();
        }

        private List<string> ExtractPackages(string logs)
        {
            var packages = new List<string>();
            var packageMatches = System.Text.RegularExpressions.Regex.Matches(logs, @"Installing\s+([a-zA-Z0-9\.\-_]+)");
            
            foreach (System.Text.RegularExpressions.Match match in packageMatches)
            {
                packages.Add(match.Groups[1].Value);
            }

            return packages.Distinct().ToList();
        }
    }

    // DTOs
    public class RecommendationRequest
    {
        public string RepositoryUrl { get; set; } = string.Empty;
        public string? Branch { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class ExecutiveSummary
    {
        public string Period { get; set; } = string.Empty;
        public int TotalDeployments { get; set; }
        public int SuccessfulDeployments { get; set; }
        public int FailedDeployments { get; set; }
        public double SuccessRate { get; set; }
        public TimeSpan AverageDuration { get; set; }
        public string AiSummary { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
    }
}
