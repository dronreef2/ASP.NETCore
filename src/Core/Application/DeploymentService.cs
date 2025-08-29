using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TutorCopiloto.Services
{
    public interface IDeploymentService
    {
        Task<Deployment> CreateDeploymentAsync(DeploymentRequest request);
        Task<List<Deployment>> GetDeploymentsAsync(int page = 1, int size = 10);
        Task<Deployment?> GetDeploymentAsync(string id);
        Task<string?> GetDeploymentLogsAsync(string id);
        Task UpdateDeploymentStatusAsync(string id, DeploymentStatus status, string? message = null);
        
        // Novo método para acessar análises automáticas
        Task<DeploymentAnalysis?> GetDeploymentAnalysisAsync(string deploymentId);
        
        // Novo método para acessar análises de repositório
        Task<RepositoryAnalysis?> GetRepositoryAnalysisAsync(string deploymentId);
        
        // Métodos síncronos para compatibilidade
        List<Deployment> GetDeployments();
        Deployment? GetDeployment(string id);
        List<DeploymentLogEntry> GetLogs(string id);
    }

    public class DeploymentService : IDeploymentService
    {
        private readonly ILogger<DeploymentService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IIntelligentAnalysisService _intelligentAnalysis;
        private readonly IOnnxInferenceService _onnxInference;
        private readonly IRepositoryAnalysisService _repositoryAnalysis;
        private readonly Dictionary<string, Deployment> _deployments = new();
        private readonly Dictionary<string, List<string>> _deploymentLogs = new();
        private readonly Dictionary<string, DeploymentAnalysis> _deploymentAnalyses = new();
        private readonly Dictionary<string, RepositoryAnalysis> _repositoryAnalyses = new();

        public DeploymentService(
            ILogger<DeploymentService> logger, 
            IConfiguration configuration,
            IIntelligentAnalysisService intelligentAnalysis,
            IOnnxInferenceService onnxInference,
            IRepositoryAnalysisService repositoryAnalysis)
        {
            _logger = logger;
            _configuration = configuration;
            _intelligentAnalysis = intelligentAnalysis;
            _onnxInference = onnxInference;
            _repositoryAnalysis = repositoryAnalysis;
        }        public async Task<Deployment> CreateDeploymentAsync(DeploymentRequest request)
        {
            var deployment = new Deployment
            {
                Id = Guid.NewGuid().ToString(),
                RepositoryUrl = request.RepositoryUrl,
                Branch = request.Branch,
                CommitSha = request.CommitSha,
                Status = DeploymentStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                Trigger = request.Trigger,
                Author = request.Author
            };

            _deployments[deployment.Id] = deployment;
            _deploymentLogs[deployment.Id] = new List<string>();

            _logger.LogInformation("Deployment criado: {DeploymentId} para {Repository}@{Branch}", 
                deployment.Id, deployment.RepositoryUrl, deployment.Branch);

            // Inicia o processo de deploy em background
            _ = Task.Run(() => ProcessDeploymentAsync(deployment));

            return deployment;
        }

        public Task<List<Deployment>> GetDeploymentsAsync(int page = 1, int size = 10)
        {
            var result = _deployments.Values
                .OrderByDescending(d => d.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .ToList();
            return Task.FromResult(result);
        }

        public Task<Deployment?> GetDeploymentAsync(string id)
        {
            var result = _deployments.TryGetValue(id, out var deployment) ? deployment : null;
            return Task.FromResult(result);
        }

        public Task<string?> GetDeploymentLogsAsync(string id)
        {
            if (!_deploymentLogs.TryGetValue(id, out var logs))
                return Task.FromResult<string?>(null);

            return Task.FromResult<string?>(string.Join("\n", logs));
        }

        public Task UpdateDeploymentStatusAsync(string id, DeploymentStatus status, string? message = null)
        {
            if (_deployments.TryGetValue(id, out var deployment))
            {
                deployment.Status = status;
                deployment.UpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(message))
                {
                    AddLog(id, $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {message}");
                }

                if (status == DeploymentStatus.Success)
                {
                    deployment.DeployedAt = DateTime.UtcNow;
                    deployment.Duration = deployment.DeployedAt - deployment.CreatedAt;
                    AddLog(id, $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] ✅ Deploy concluído com sucesso!");
                }
                else if (status == DeploymentStatus.Failed)
                {
                    deployment.DeployedAt = DateTime.UtcNow;
                    deployment.Duration = deployment.DeployedAt - deployment.CreatedAt;
                    AddLog(id, $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] ❌ Deploy falhou!");
                }
            }
            return Task.CompletedTask;
        }

        public Task<DeploymentAnalysis?> GetDeploymentAnalysisAsync(string deploymentId)
        {
            var result = _deploymentAnalyses.TryGetValue(deploymentId, out var analysis) ? analysis : null;
            return Task.FromResult(result);
        }

        public Task<RepositoryAnalysis?> GetRepositoryAnalysisAsync(string deploymentId)
        {
            var result = _repositoryAnalyses.TryGetValue(deploymentId, out var analysis) ? analysis : null;
            return Task.FromResult(result);
        }

        private async Task ProcessDeploymentAsync(Deployment deployment)
        {
            try
            {
                await UpdateDeploymentStatusAsync(deployment.Id, DeploymentStatus.Running, "Iniciando deploy...");
                
                AddLog(deployment.Id, $"🚀 Iniciando deploy do repositório: {deployment.RepositoryUrl}");
                AddLog(deployment.Id, $"📝 Branch: {deployment.Branch}");
                AddLog(deployment.Id, $"👤 Autor: {deployment.Author}");
                
                if (!string.IsNullOrEmpty(deployment.CommitSha))
                {
                    AddLog(deployment.Id, $"📋 Commit: {deployment.CommitSha[..8]}...");
                }

                // ✅ Análise REAL do repositório
                await PerformRepositoryAnalysis(deployment);

                // Simula processo de deploy
                await SimulateDeploymentProcess(deployment);

                // ✅ Análise automática com IA e ONNX
                await PerformAutomaticAnalysis(deployment);
                
                await UpdateDeploymentStatusAsync(deployment.Id, DeploymentStatus.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante deployment {DeploymentId}", deployment.Id);
                await UpdateDeploymentStatusAsync(deployment.Id, DeploymentStatus.Failed, ex.Message);
            }
        }

        private async Task PerformRepositoryAnalysis(Deployment deployment)
        {
            try
            {
                AddLog(deployment.Id, $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] 📊 Iniciando análise REAL do repositório...");

                // Analisar o repositório usando o serviço dedicado
                var repositoryAnalysis = await _repositoryAnalysis.AnalyzeRepositoryAsync(
                    deployment.RepositoryUrl, 
                    deployment.Branch);

                // Armazenar análise do repositório
                _repositoryAnalyses[deployment.Id] = repositoryAnalysis;

                // Log dos resultados da análise
                AddLog(deployment.Id, $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] 📁 Repositório: {repositoryAnalysis.RepositoryName}");
                AddLog(deployment.Id, $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] 📊 Arquivos encontrados: {repositoryAnalysis.TotalFiles}");
                AddLog(deployment.Id, $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] 💾 Tamanho total: {FormatFileSize(repositoryAnalysis.TotalSize)}");

                if (repositoryAnalysis.ProgrammingLanguages.Any())
                {
                    var languages = string.Join(", ", repositoryAnalysis.ProgrammingLanguages.Select(l => $"{l.Name} ({l.FileCount})"));
                    AddLog(deployment.Id, $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] 💻 Linguagens: {languages}");
                }

                if (repositoryAnalysis.ConfigurationFiles.Any())
                {
                    var configs = string.Join(", ", repositoryAnalysis.ConfigurationFiles.Select(c => c.FileName));
                    AddLog(deployment.Id, $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] ⚙️ Configurações: {configs}");
                }

                if (repositoryAnalysis.ImportantFiles.Any())
                {
                    var important = string.Join(", ", repositoryAnalysis.ImportantFiles.Select(f => f.FileName));
                    AddLog(deployment.Id, $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] 📄 Arquivos importantes: {important}");
                }

                // Verificar se há README
                var readme = repositoryAnalysis.ImportantFiles.FirstOrDefault(f => 
                    f.FileName.ToLower().Contains("readme"));
                if (readme != null)
                {
                    AddLog(deployment.Id, $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] 📖 README encontrado - analisando conteúdo...");
                    
                    // Aqui poderia fazer uma análise mais profunda do README
                    if (readme.Content.Contains("## Installation") || readme.Content.Contains("## Instalação"))
                    {
                        AddLog(deployment.Id, $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] 📦 Instruções de instalação encontradas");
                    }
                    
                    if (readme.Content.Contains("## Usage") || readme.Content.Contains("## Uso"))
                    {
                        AddLog(deployment.Id, $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] 🚀 Instruções de uso encontradas");
                    }
                }

                AddLog(deployment.Id, $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] ✅ Análise do repositório concluída!");
                _logger.LogInformation("Análise do repositório concluída para {RepositoryUrl}", deployment.RepositoryUrl);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Análise do repositório falhou para {RepositoryUrl}, continuando...", deployment.RepositoryUrl);
                AddLog(deployment.Id, $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] ⚠️ Análise do repositório falhou: {ex.Message}");
            }
        }

        private async Task PerformAutomaticAnalysis(Deployment deployment)
        {
            try
            {
                AddLog(deployment.Id, $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] 🧠 Iniciando análise automática com IA...");

                // 1. Extrair logs atuais
                var logsText = string.Join("\n", _deploymentLogs[deployment.Id]);

                // 2. Análise de logs com IA conversacional
                var aiAnalysis = await _intelligentAnalysis.AnalyzeDeploymentLogsAsync(deployment.Id, logsText);
                AddLog(deployment.Id, $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] 📊 Análise IA concluída - Status: {aiAnalysis.Status}");

                // 3. Predição com modelos ONNX
                var features = ExtractDeploymentFeatures(deployment, logsText);
                var prediction = await _onnxInference.PredictDeploymentOutcomeAsync(features);
                AddLog(deployment.Id, $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] 🎯 Predição ONNX: {prediction.SuccessProbability:P0} probabilidade de sucesso");

                // 4. Detecção de anomalias (dados históricos)
                var historicalMetrics = GetHistoricalMetrics();
                AnomalyDetectionResult? anomalyResult = null;
                if (historicalMetrics.Any())
                {
                    anomalyResult = await _onnxInference.DetectAnomaliesAsync(historicalMetrics);
                    AddLog(deployment.Id, $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] 🔍 Anomalias detectadas: {anomalyResult.Anomalies.Count}");
                }

                // 5. Análise de segurança
                var securityFeatures = ExtractSecurityFeatures(logsText);
                var securityAssessment = await _onnxInference.AssessSecurityThreatsAsync(securityFeatures);
                AddLog(deployment.Id, $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] 🔒 Análise de segurança: {securityAssessment.Threats.Count} ameaças encontradas");

                // 6. Armazenar análise completa
                var analysis = new DeploymentAnalysis
                {
                    DeploymentId = Guid.Parse(deployment.Id),
                    AiAnalysis = aiAnalysis.AiInsights,
                    SuccessProbability = prediction.SuccessProbability,
                    AnomalyDetected = anomalyResult?.Anomalies.Any() ?? false,
                    SecurityAssessment = string.Join(", ", securityAssessment.Threats.Select(t => t.Description)),
                    OptimizationSuggestions = string.Join(", ", aiAnalysis.Recommendations.Concat(prediction.Recommendations)),
                    AnalyzedAt = DateTime.UtcNow
                };

                _deploymentAnalyses[deployment.Id] = analysis;

                // 7. Log das recomendações principais
                var recommendations = aiAnalysis.Recommendations.Concat(prediction.Recommendations).ToList();
                if (recommendations.Any())
                {
                    AddLog(deployment.Id, $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] 💡 Principais recomendações:");
                    foreach (var rec in recommendations.Take(3))
                    {
                        AddLog(deployment.Id, $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}]   • {rec}");
                    }
                }

                AddLog(deployment.Id, $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] ✅ Análise automática concluída!");
                _logger.LogInformation("Análise automática concluída para deployment {DeploymentId}", deployment.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Análise automática falhou para deployment {DeploymentId}, continuando...", deployment.Id);
                AddLog(deployment.Id, $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] ⚠️ Análise automática falhou: {ex.Message}");
            }
        }

        private async Task SimulateDeploymentProcess(Deployment deployment)
        {
            var steps = new[]
            {
                ("📦 Clonando repositório...", 2000),
                ("🔍 Analisando código...", 1500),
                ("📋 Instalando dependências...", 3000),
                ("🏗️ Compilando aplicação...", 2500),
                ("🔧 Configurando ambiente...", 1000),
                ("🚀 Fazendo deploy...", 2000),
                ("✅ Verificando saúde da aplicação...", 1500)
            };

            foreach (var (message, delay) in steps)
            {
                AddLog(deployment.Id, $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {message}");
                await Task.Delay(delay);
            }

            // Simula URL de deploy
            var deployUrl = GenerateDeployUrl(deployment);
            deployment.DeployUrl = deployUrl;
            AddLog(deployment.Id, $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] 🌐 Deploy disponível em: {deployUrl}");
        }

        private string GenerateDeployUrl(Deployment deployment)
        {
            var baseUrl = _configuration["Deployment:BaseUrl"] ?? "https://deploy.tutorcopiloto.com";
            var slug = deployment.Id[..8].ToLowerInvariant();
            return $"{baseUrl}/{slug}";
        }

        private void AddLog(string deploymentId, string message)
        {
            if (_deploymentLogs.TryGetValue(deploymentId, out var logs))
            {
                logs.Add(message);
                _logger.LogInformation("Deploy {DeploymentId}: {Message}", deploymentId, message);
            }
        }

        // Métodos síncronos para compatibilidade
        public List<Deployment> GetDeployments()
        {
            return _deployments.Values.OrderByDescending(d => d.CreatedAt).ToList();
        }

        public Deployment? GetDeployment(string id)
        {
            return _deployments.TryGetValue(id, out var deployment) ? deployment : null;
        }

        public List<DeploymentLogEntry> GetLogs(string id)
        {
            if (!_deploymentLogs.TryGetValue(id, out var logs))
                return new List<DeploymentLogEntry>();

            return logs.Select((log, index) => new DeploymentLogEntry
            {
                Id = index.ToString(),
                DeploymentId = id,
                Message = log,
                Timestamp = DateTime.UtcNow.AddSeconds(-logs.Count + index),
                Level = log.Contains("ERRO") ? LogLevel.Error : 
                       log.Contains("WARN") ? LogLevel.Warning : 
                       LogLevel.Information
            }).ToList();
        }

        // Métodos auxiliares para análise automática
        private DeploymentFeatures ExtractDeploymentFeatures(Deployment deployment, string logs)
        {
            return new DeploymentFeatures
            {
                RepositorySize = Random.Shared.Next(100, 2000), // Simulado - em produção seria calculado
                Dependencies = ExtractDependencies(logs),
                HasTests = logs.Contains("test") || logs.Contains("spec"),
                HasDockerfile = logs.Contains("docker") || logs.Contains("Dockerfile"),
                PreviousFailures = GetPreviousFailures(deployment.RepositoryUrl),
                LastSuccessfulDeploymentDays = GetDaysSinceLastSuccess(deployment.RepositoryUrl)
            };
        }

        private List<string> ExtractDependencies(string logs)
        {
            var dependencies = new List<string>();
            // Simulação - em produção seria extraído dos logs reais
            if (logs.Contains("npm")) dependencies.AddRange(new[] { "react", "typescript", "vite" });
            if (logs.Contains("dotnet")) dependencies.AddRange(new[] { "asp.net", "entity-framework" });
            return dependencies;
        }

        private int GetPreviousFailures(string repositoryUrl)
        {
            // Simulação - em produção seria consultado do banco
            return Random.Shared.Next(0, 3);
        }

        private int GetDaysSinceLastSuccess(string repositoryUrl)
        {
            // Simulação - em produção seria calculado baseado em dados históricos
            return Random.Shared.Next(1, 30);
        }

        private List<DeploymentMetrics> GetHistoricalMetrics()
        {
            // Simulação - em produção seria carregado do banco de dados
            return _deployments.Values
                .Where(d => d.Status == DeploymentStatus.Success)
                .Take(10)
                .Select(d => new DeploymentMetrics
                {
                    Timestamp = d.CreatedAt,
                    DeploymentDuration = d.Duration ?? TimeSpan.FromMinutes(5),
                    CpuUsage = Random.Shared.NextDouble() * 100,
                    MemoryUsage = Random.Shared.NextDouble() * 100,
                    DiskUsage = Random.Shared.NextDouble() * 100,
                    Success = true
                })
                .ToList();
        }

        private SecurityFeatures ExtractSecurityFeatures(string logs)
        {
            return new SecurityFeatures
            {
                ExposedPorts = ExtractExposedPorts(logs),
                HasHttpEndpoints = logs.Contains("http://") || logs.Contains("GET") || logs.Contains("POST"),
                HasStrongAuthentication = logs.Contains("authentication") || logs.Contains("jwt") || logs.Contains("oauth"),
                HasEncryption = logs.Contains("https://") || logs.Contains("ssl") || logs.Contains("tls"),
                InstalledPackages = ExtractPackages(logs)
            };
        }

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

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double size = bytes;
            
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            
            return $"{size:0.##} {sizes[order]}";
        }
    }

    // Models
    public class Deployment
    {
        public string Id { get; set; } = string.Empty;
        public string RepositoryUrl { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public string? CommitSha { get; set; }
        public DeploymentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeployedAt { get; set; }
        public TimeSpan? Duration { get; set; }
        public string? DeployUrl { get; set; }
        public string Trigger { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
    }

    public class DeploymentRequest
    {
        public string RepositoryUrl { get; set; } = string.Empty;
        public string Branch { get; set; } = "main";
        public string? CommitSha { get; set; }
        public string Trigger { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
    }

    public enum DeploymentStatus
    {
        Pending,
        Running,
        Success,
        Failed,
        Cancelled
    }

    public class DeploymentLogEntry
    {
        public string Id { get; set; } = string.Empty;
        public string DeploymentId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public LogLevel Level { get; set; } = LogLevel.Information;
    }
}
