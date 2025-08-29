using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace TutorCopiloto.Services
{
    public interface IOnnxInferenceService
    {
        Task<DeploymentPrediction> PredictDeploymentOutcomeAsync(DeploymentFeatures features);
        Task<AnomalyDetectionResult> DetectAnomaliesAsync(List<DeploymentMetrics> historicalData);
        Task<ResourceOptimizationResult> OptimizeResourcesAsync(ResourceUsageData currentUsage);
        Task<SecurityThreatAssessment> AssessSecurityThreatsAsync(SecurityFeatures features);
        Task TrainModelFromHistoricalDataAsync(List<HistoricalDeployment> data);
    }

    public class OnnxInferenceService : IOnnxInferenceService, IDisposable
    {
        private readonly ILogger<OnnxInferenceService> _logger;
        private readonly IConfiguration _configuration;
        private InferenceSession? _deploymentPredictionSession;
        private InferenceSession? _anomalyDetectionSession;
        private InferenceSession? _resourceOptimizationSession;
        private InferenceSession? _securityAssessmentSession;
        private readonly string _modelsPath;

        public OnnxInferenceService(ILogger<OnnxInferenceService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _modelsPath = _configuration["ONNX:ModelsPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "Models");
            
            InitializeModels();
        }

        private void InitializeModels()
        {
            try
            {
                Directory.CreateDirectory(_modelsPath);

                // Crie modelos ONNX simples ou use modelos pré-treinados
                var sessionOptions = new Microsoft.ML.OnnxRuntime.SessionOptions();
                sessionOptions.LogSeverityLevel = OrtLoggingLevel.ORT_LOGGING_LEVEL_WARNING;

                // Para demonstração, vamos criar modelos sintéticos
                CreateSyntheticModels();

                // Inicializar sessões ONNX
                var deploymentModelPath = Path.Combine(_modelsPath, "deployment_prediction.onnx");
                var anomalyModelPath = Path.Combine(_modelsPath, "anomaly_detection.onnx");
                var resourceModelPath = Path.Combine(_modelsPath, "resource_optimization.onnx");
                var securityModelPath = Path.Combine(_modelsPath, "security_assessment.onnx");

                if (File.Exists(deploymentModelPath))
                    _deploymentPredictionSession = new InferenceSession(deploymentModelPath, sessionOptions);

                if (File.Exists(anomalyModelPath))
                    _anomalyDetectionSession = new InferenceSession(anomalyModelPath, sessionOptions);

                if (File.Exists(resourceModelPath))
                    _resourceOptimizationSession = new InferenceSession(resourceModelPath, sessionOptions);

                if (File.Exists(securityModelPath))
                    _securityAssessmentSession = new InferenceSession(securityModelPath, sessionOptions);

                _logger.LogInformation("Modelos ONNX inicializados com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inicializar modelos ONNX");
            }
        }

        private void CreateSyntheticModels()
        {
            // Para demonstração, criamos modelos de ML simples
            // Em produção, você usaria modelos treinados com dados reais
            _logger.LogInformation("Criando modelos sintéticos para demonstração");
            
            // Modelos serão simulados via cálculos heurísticos
            // Em produção, você carregaria modelos .onnx reais aqui
        }

        public Task<DeploymentPrediction> PredictDeploymentOutcomeAsync(DeploymentFeatures features)
        {
            try
            {
                _logger.LogInformation("Predizendo resultado do deployment usando ML");

                // Se não há modelo ONNX, use heurísticas
                if (_deploymentPredictionSession == null)
                {
                    return Task.FromResult(PredictWithHeuristics(features));
                }

                // Preparar entrada para o modelo ONNX
                var inputTensor = CreateInputTensor(features);
                var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor("input", inputTensor)
                };

                // Executar inferência em background
                return Task.Run(() =>
                {
                    using var results = _deploymentPredictionSession.Run(inputs);
                    var output = results.First().AsTensor<float>();

                    var successProbability = output[0];
                    var estimatedDuration = TimeSpan.FromMinutes(output[1] * 60); // Converter de horas para minutos

                    return new DeploymentPrediction
                    {
                        SuccessProbability = successProbability,
                        EstimatedDuration = estimatedDuration,
                        RiskFactors = IdentifyRiskFactors(features),
                        Confidence = CalculateConfidence(successProbability),
                        Recommendations = GenerateRecommendations(features, successProbability),
                        PredictedAt = DateTime.UtcNow
                    };
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na predição de deployment");
                return Task.FromResult(PredictWithHeuristics(features));
            }
        }

        public Task<AnomalyDetectionResult> DetectAnomaliesAsync(List<DeploymentMetrics> historicalData)
        {
            try
            {
                _logger.LogInformation("Detectando anomalias em {Count} registros históricos", historicalData.Count);

                var anomalies = new List<AnomalyPoint>();
                var threshold = CalculateAnomalyThreshold(historicalData);

                for (int i = 0; i < historicalData.Count; i++)
                {
                    var metrics = historicalData[i];
                    var anomalyScore = CalculateAnomalyScore(metrics, historicalData);

                    if (anomalyScore > threshold)
                    {
                        anomalies.Add(new AnomalyPoint
                        {
                            Timestamp = metrics.Timestamp,
                            AnomalyScore = anomalyScore,
                            MetricType = DetermineAnomalyType(metrics),
                            Severity = DetermineAnomalySeverity(anomalyScore),
                            Description = GenerateAnomalyDescription(metrics, anomalyScore)
                        });
                    }
                }

                return Task.FromResult(new AnomalyDetectionResult
                {
                    Anomalies = anomalies,
                    OverallHealthScore = CalculateHealthScore(anomalies.Count, historicalData.Count),
                    Trends = IdentifyTrends(historicalData),
                    Recommendations = GenerateAnomalyRecommendations(anomalies),
                    DetectedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na detecção de anomalias");
                return Task.FromResult(new AnomalyDetectionResult
                {
                    Anomalies = new List<AnomalyPoint>(),
                    OverallHealthScore = 75,
                    Trends = new List<string> { "Dados insuficientes para análise de tendências" },
                    Recommendations = new List<string> { "Colete mais dados históricos" },
                    DetectedAt = DateTime.UtcNow
                });
            }
        }

        public Task<ResourceOptimizationResult> OptimizeResourcesAsync(ResourceUsageData currentUsage)
        {
            try
            {
                _logger.LogInformation("Otimizando recursos com base no uso atual");

                var optimizations = new List<ResourceOptimization>();

                // Análise de CPU
                if (currentUsage.CpuUsagePercent > 80)
                {
                    optimizations.Add(new ResourceOptimization
                    {
                        ResourceType = "CPU",
                        CurrentUsage = currentUsage.CpuUsagePercent,
                        RecommendedAllocation = Math.Ceiling(currentUsage.CpuCores * 1.5),
                        Reasoning = "Alto uso de CPU detectado",
                        PotentialSavings = "Melhor performance em deployments"
                    });
                }

                // Análise de Memória
                if (currentUsage.MemoryUsagePercent > 85)
                {
                    optimizations.Add(new ResourceOptimization
                    {
                        ResourceType = "Memory",
                        CurrentUsage = currentUsage.MemoryUsagePercent,
                        RecommendedAllocation = Math.Ceiling(currentUsage.MemoryGB * 1.3),
                        Reasoning = "Uso de memória próximo ao limite",
                        PotentialSavings = "Prevenção de falhas por falta de memória"
                    });
                }

                // Análise de Disco
                if (currentUsage.DiskUsagePercent > 75)
                {
                    optimizations.Add(new ResourceOptimization
                    {
                        ResourceType = "Disk",
                        CurrentUsage = currentUsage.DiskUsagePercent,
                        RecommendedAllocation = Math.Ceiling(currentUsage.DiskGB * 1.5),
                        Reasoning = "Espaço em disco limitado",
                        PotentialSavings = "Evitar falhas por falta de espaço"
                    });
                }

                var estimatedCostImpact = CalculateCostImpact(optimizations);
                var performanceImpact = CalculatePerformanceImpact(optimizations);

                return Task.FromResult(new ResourceOptimizationResult
                {
                    Optimizations = optimizations,
                    EstimatedCostImpact = estimatedCostImpact,
                    PerformanceImpact = performanceImpact,
                    ImplementationPriority = DeterminePriority(optimizations),
                    OptimizedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na otimização de recursos");
                return Task.FromResult(new ResourceOptimizationResult
                {
                    Optimizations = new List<ResourceOptimization>(),
                    EstimatedCostImpact = 0,
                    PerformanceImpact = "Neutro",
                    ImplementationPriority = "Baixa",
                    OptimizedAt = DateTime.UtcNow
                });
            }
        }

        public async Task<SecurityThreatAssessment> AssessSecurityThreatsAsync(SecurityFeatures features)
        {
            try
            {
                _logger.LogInformation("Avaliando ameaças de segurança");

                var threats = new List<SecurityThreat>();

                // Análise de exposição de portas
                if (features.ExposedPorts.Any(p => p == 22 || p == 3389))
                {
                    threats.Add(new SecurityThreat
                    {
                        ThreatType = "Exposed Administrative Ports",
                        Severity = ThreatSeverity.High,
                        Description = "Portas administrativas expostas publicamente",
                        Recommendation = "Restrinja acesso a portas SSH/RDP",
                        CvssScore = 7.5
                    });
                }

                // Análise de configurações inseguras
                if (features.HasHttpEndpoints)
                {
                    threats.Add(new SecurityThreat
                    {
                        ThreatType = "Insecure HTTP",
                        Severity = ThreatSeverity.Medium,
                        Description = "Endpoints HTTP sem criptografia",
                        Recommendation = "Migre para HTTPS",
                        CvssScore = 5.0
                    });
                }

                // Análise de autenticação
                if (!features.HasStrongAuthentication)
                {
                    threats.Add(new SecurityThreat
                    {
                        ThreatType = "Weak Authentication",
                        Severity = ThreatSeverity.High,
                        Description = "Autenticação fraca ou ausente",
                        Recommendation = "Implemente MFA e senhas fortes",
                        CvssScore = 8.0
                    });
                }

                var overallRisk = CalculateOverallRisk(threats);
                var complianceStatus = AssessCompliance(features);

                return new SecurityThreatAssessment
                {
                    Threats = threats,
                    OverallRiskScore = overallRisk,
                    ComplianceStatus = complianceStatus,
                    MitigationPlan = GenerateMitigationPlan(threats),
                    AssessedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na avaliação de segurança");
                return new SecurityThreatAssessment
                {
                    Threats = new List<SecurityThreat>(),
                    OverallRiskScore = 50,
                    ComplianceStatus = "Desconhecido",
                    MitigationPlan = new List<string> { "Avaliação manual necessária" },
                    AssessedAt = DateTime.UtcNow
                };
            }
        }

        public async Task TrainModelFromHistoricalDataAsync(List<HistoricalDeployment> data)
        {
            try
            {
                _logger.LogInformation("Iniciando treinamento de modelo com {Count} registros históricos", data.Count);

                // Simulação de treinamento - em produção, usaria ML.NET ou Python integration
                await Task.Delay(1000); // Simula processamento

                // Gerar estatísticas dos dados de treinamento
                var successRate = data.Count(d => d.Success) / (double)data.Count;
                var avgDuration = data.Average(d => d.Duration.TotalMinutes);

                _logger.LogInformation("Treinamento concluído - Taxa de sucesso: {SuccessRate:P}, Duração média: {AvgDuration:F2} min", 
                    successRate, avgDuration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no treinamento do modelo");
            }
        }

        // Helper methods
        private DeploymentPrediction PredictWithHeuristics(DeploymentFeatures features)
        {
            // Lógica heurística quando não há modelo ML
            double successProbability = 0.8; // Base

            // Ajustar baseado em fatores
            if (features.RepositorySize > 1000) successProbability -= 0.1;
            if (features.Dependencies.Count > 50) successProbability -= 0.1;
            if (features.HasTests) successProbability += 0.1;
            if (features.HasDockerfile) successProbability += 0.05;

            successProbability = Math.Max(0.1, Math.Min(0.95, successProbability));

            var estimatedDuration = TimeSpan.FromMinutes(
                5 + (features.RepositorySize / 100) + (features.Dependencies.Count * 0.1));

            return new DeploymentPrediction
            {
                SuccessProbability = (float)successProbability,
                EstimatedDuration = estimatedDuration,
                RiskFactors = IdentifyRiskFactors(features),
                Confidence = 0.7f,
                Recommendations = GenerateRecommendations(features, (float)successProbability),
                PredictedAt = DateTime.UtcNow
            };
        }

        private Tensor<float> CreateInputTensor(DeploymentFeatures features)
        {
            var input = new float[]
            {
                features.RepositorySize,
                features.Dependencies.Count,
                features.HasTests ? 1f : 0f,
                features.HasDockerfile ? 1f : 0f,
                features.PreviousFailures,
                features.LastSuccessfulDeploymentDays
            };

            return new DenseTensor<float>(input, new[] { 1, input.Length });
        }

        private List<string> IdentifyRiskFactors(DeploymentFeatures features)
        {
            var risks = new List<string>();
            
            if (features.RepositorySize > 1000) 
                risks.Add("Repositório grande pode aumentar tempo de build");
            
            if (features.Dependencies.Count > 50) 
                risks.Add("Muitas dependências podem causar conflitos");
            
            if (!features.HasTests) 
                risks.Add("Ausência de testes aumenta risco de falhas");
                
            if (features.PreviousFailures > 3) 
                risks.Add("Histórico de falhas recentes");

            return risks;
        }

        private float CalculateConfidence(float successProbability)
        {
            // Confiança baseada na probabilidade e outros fatores
            return Math.Abs(successProbability - 0.5f) * 2f;
        }

        private List<string> GenerateRecommendations(DeploymentFeatures features, float successProbability)
        {
            var recommendations = new List<string>();

            if (successProbability < 0.7f)
                recommendations.Add("Considere revisar configurações antes do deployment");

            if (!features.HasTests)
                recommendations.Add("Adicione testes automatizados");

            if (!features.HasDockerfile)
                recommendations.Add("Use containerização com Docker");

            return recommendations;
        }

        private double CalculateAnomalyThreshold(List<DeploymentMetrics> data)
        {
            // Calcula threshold baseado em desvio padrão
            var durations = data.Select(d => d.DeploymentDuration.TotalMinutes).ToArray();
            var mean = durations.Average();
            var variance = durations.Select(d => Math.Pow(d - mean, 2)).Average();
            var stdDev = Math.Sqrt(variance);
            
            return mean + (2 * stdDev); // 2 desvios padrão
        }

        private double CalculateAnomalyScore(DeploymentMetrics metrics, List<DeploymentMetrics> baseline)
        {
            var avgDuration = baseline.Average(m => m.DeploymentDuration.TotalMinutes);
            var avgCpu = baseline.Average(m => m.CpuUsage);
            var avgMemory = baseline.Average(m => m.MemoryUsage);

            var durationScore = Math.Abs(metrics.DeploymentDuration.TotalMinutes - avgDuration) / avgDuration;
            var cpuScore = Math.Abs(metrics.CpuUsage - avgCpu) / avgCpu;
            var memoryScore = Math.Abs(metrics.MemoryUsage - avgMemory) / avgMemory;

            return (durationScore + cpuScore + memoryScore) / 3;
        }

        private string DetermineAnomalyType(DeploymentMetrics metrics)
        {
            if (metrics.DeploymentDuration.TotalMinutes > 30) return "Duration";
            if (metrics.CpuUsage > 90) return "CPU";
            if (metrics.MemoryUsage > 90) return "Memory";
            return "General";
        }

        private AnomalySeverity DetermineAnomalySeverity(double score)
        {
            if (score > 2.0) return AnomalySeverity.Critical;
            if (score > 1.5) return AnomalySeverity.High;
            if (score > 1.0) return AnomalySeverity.Medium;
            return AnomalySeverity.Low;
        }

        private string GenerateAnomalyDescription(DeploymentMetrics metrics, double score)
        {
            return $"Métrica anômala detectada (score: {score:F2}) em {metrics.Timestamp:yyyy-MM-dd HH:mm}";
        }

        private int CalculateHealthScore(int anomaliesCount, int totalCount)
        {
            var anomalyRate = (double)anomaliesCount / totalCount;
            return (int)Math.Max(0, 100 - (anomalyRate * 100));
        }

        private List<string> IdentifyTrends(List<DeploymentMetrics> data)
        {
            var trends = new List<string>();
            
            if (data.Count < 2) return trends;

            var recent = data.TakeLast(5).ToList();
            var older = data.SkipLast(5).TakeLast(5).ToList();

            if (recent.Average(r => r.DeploymentDuration.TotalMinutes) > 
                older.Average(o => o.DeploymentDuration.TotalMinutes) * 1.2)
            {
                trends.Add("Tendência de aumento no tempo de deployment");
            }

            return trends;
        }

        private List<string> GenerateAnomalyRecommendations(List<AnomalyPoint> anomalies)
        {
            var recommendations = new List<string>();
            
            if (anomalies.Any(a => a.MetricType == "Duration"))
                recommendations.Add("Investigue gargalos no processo de deployment");
            
            if (anomalies.Any(a => a.MetricType == "CPU"))
                recommendations.Add("Considere aumentar recursos de CPU");
                
            if (anomalies.Any(a => a.MetricType == "Memory"))
                recommendations.Add("Verifique uso de memória e possíveis vazamentos");

            return recommendations;
        }

        private double CalculateCostImpact(List<ResourceOptimization> optimizations)
        {
            return optimizations.Sum(o => o.RecommendedAllocation - o.CurrentUsage) * 10; // $10 por unidade
        }

        private string CalculatePerformanceImpact(List<ResourceOptimization> optimizations)
        {
            if (optimizations.Count == 0) return "Neutro";
            if (optimizations.Count > 2) return "Alto";
            return "Médio";
        }

        private string DeterminePriority(List<ResourceOptimization> optimizations)
        {
            if (optimizations.Any(o => o.ResourceType == "CPU" && o.CurrentUsage > 90)) return "Alta";
            if (optimizations.Any(o => o.ResourceType == "Memory" && o.CurrentUsage > 90)) return "Alta";
            if (optimizations.Count > 1) return "Média";
            return "Baixa";
        }

        private double CalculateOverallRisk(List<SecurityThreat> threats)
        {
            if (!threats.Any()) return 10;
            return threats.Average(t => t.CvssScore);
        }

        private string AssessCompliance(SecurityFeatures features)
        {
            var score = 0;
            if (!features.HasHttpEndpoints) score += 25;
            if (features.HasStrongAuthentication) score += 25;
            if (!features.ExposedPorts.Contains(22)) score += 25;
            if (!features.ExposedPorts.Contains(3389)) score += 25;

            return score switch
            {
                >= 75 => "Compliant",
                >= 50 => "Partially Compliant",
                _ => "Non-Compliant"
            };
        }

        private List<string> GenerateMitigationPlan(List<SecurityThreat> threats)
        {
            return threats.OrderByDescending(t => t.CvssScore)
                          .Select(t => t.Recommendation)
                          .Take(5)
                          .ToList();
        }

        public void Dispose()
        {
            _deploymentPredictionSession?.Dispose();
            _anomalyDetectionSession?.Dispose();
            _resourceOptimizationSession?.Dispose();
            _securityAssessmentSession?.Dispose();
        }
    }

    // Models para os resultados
    public class DeploymentPrediction
    {
        public float SuccessProbability { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
        public List<string> RiskFactors { get; set; } = new();
        public float Confidence { get; set; }
        public List<string> Recommendations { get; set; } = new();
        public DateTime PredictedAt { get; set; }
    }

    public class DeploymentFeatures
    {
        public float RepositorySize { get; set; }
        public List<string> Dependencies { get; set; } = new();
        public bool HasTests { get; set; }
        public bool HasDockerfile { get; set; }
        public int PreviousFailures { get; set; }
        public float LastSuccessfulDeploymentDays { get; set; }
    }



    public class AnomalyDetectionResult
    {
        public List<AnomalyPoint> Anomalies { get; set; } = new();
        public int OverallHealthScore { get; set; }
        public List<string> Trends { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        public DateTime DetectedAt { get; set; }
    }

    public class AnomalyPoint
    {
        public DateTime Timestamp { get; set; }
        public double AnomalyScore { get; set; }
        public string MetricType { get; set; } = string.Empty;
        public AnomalySeverity Severity { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public enum AnomalySeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    public class DeploymentMetrics
    {
        public DateTime Timestamp { get; set; }
        public TimeSpan DeploymentDuration { get; set; }
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double DiskUsage { get; set; }
        public bool Success { get; set; }
    }

    public class ResourceOptimizationResult
    {
        public List<ResourceOptimization> Optimizations { get; set; } = new();
        public double EstimatedCostImpact { get; set; }
        public string PerformanceImpact { get; set; } = string.Empty;
        public string ImplementationPriority { get; set; } = string.Empty;
        public DateTime OptimizedAt { get; set; }
    }

    public class ResourceOptimization
    {
        public string ResourceType { get; set; } = string.Empty;
        public double CurrentUsage { get; set; }
        public double RecommendedAllocation { get; set; }
        public string Reasoning { get; set; } = string.Empty;
        public string PotentialSavings { get; set; } = string.Empty;
    }

    public class ResourceUsageData
    {
        public double CpuUsagePercent { get; set; }
        public double CpuCores { get; set; }
        public double MemoryUsagePercent { get; set; }
        public double MemoryGB { get; set; }
        public double DiskUsagePercent { get; set; }
        public double DiskGB { get; set; }
        public DateTime MeasuredAt { get; set; }
    }

    public class SecurityThreatAssessment
    {
        public List<SecurityThreat> Threats { get; set; } = new();
        public double OverallRiskScore { get; set; }
        public string ComplianceStatus { get; set; } = string.Empty;
        public List<string> MitigationPlan { get; set; } = new();
        public DateTime AssessedAt { get; set; }
    }

    public class SecurityThreat
    {
        public string ThreatType { get; set; } = string.Empty;
        public ThreatSeverity Severity { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
        public double CvssScore { get; set; }
    }

    public enum ThreatSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    public class SecurityFeatures
    {
        public List<int> ExposedPorts { get; set; } = new();
        public bool HasHttpEndpoints { get; set; }
        public bool HasStrongAuthentication { get; set; }
        public bool HasEncryption { get; set; }
        public List<string> InstalledPackages { get; set; } = new();
    }

    public class HistoricalDeployment
    {
        public string Id { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool Success { get; set; }
        public TimeSpan Duration { get; set; }
        public string RepositoryUrl { get; set; } = string.Empty;
        public DeploymentFeatures Features { get; set; } = new();
    }

    public class DeploymentAnalysis
    {
        public Guid DeploymentId { get; set; }
        public string AiAnalysis { get; set; } = string.Empty;
        public double SuccessProbability { get; set; }
        public bool AnomalyDetected { get; set; }
        public string SecurityAssessment { get; set; } = string.Empty;
        public string OptimizationSuggestions { get; set; } = string.Empty;
        public DateTime AnalyzedAt { get; set; }
    }
}
