using System.Text.Json;

namespace TutorCopiloto.Services
{
    public interface IDeploymentService
    {
        Task<Deployment> CreateDeploymentAsync(DeploymentRequest request);
        Task<List<Deployment>> GetDeploymentsAsync(int page = 1, int size = 10);
        Task<Deployment?> GetDeploymentAsync(string id);
        Task<string?> GetDeploymentLogsAsync(string id);
        Task UpdateDeploymentStatusAsync(string id, DeploymentStatus status, string? message = null);
        
        // Métodos síncronos para compatibilidade
        List<Deployment> GetDeployments();
        Deployment? GetDeployment(string id);
        List<DeploymentLogEntry> GetLogs(string id);
    }

    public class DeploymentService : IDeploymentService
    {
        private readonly ILogger<DeploymentService> _logger;
        private readonly IConfiguration _configuration;
        private readonly Dictionary<string, Deployment> _deployments = new();
        private readonly Dictionary<string, List<string>> _deploymentLogs = new();

        public DeploymentService(ILogger<DeploymentService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<Deployment> CreateDeploymentAsync(DeploymentRequest request)
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

        public async Task<List<Deployment>> GetDeploymentsAsync(int page = 1, int size = 10)
        {
            await Task.CompletedTask;
            
            return _deployments.Values
                .OrderByDescending(d => d.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .ToList();
        }

        public async Task<Deployment?> GetDeploymentAsync(string id)
        {
            await Task.CompletedTask;
            return _deployments.TryGetValue(id, out var deployment) ? deployment : null;
        }

        public async Task<string?> GetDeploymentLogsAsync(string id)
        {
            await Task.CompletedTask;
            
            if (!_deploymentLogs.TryGetValue(id, out var logs))
                return null;

            return string.Join("\n", logs);
        }

        public async Task UpdateDeploymentStatusAsync(string id, DeploymentStatus status, string? message = null)
        {
            await Task.CompletedTask;
            
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

                // Simula processo de deploy
                await SimulateDeploymentProcess(deployment);
                
                await UpdateDeploymentStatusAsync(deployment.Id, DeploymentStatus.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante deployment {DeploymentId}", deployment.Id);
                await UpdateDeploymentStatusAsync(deployment.Id, DeploymentStatus.Failed, ex.Message);
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
