using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Threading.Tasks;
using TutorCopiloto.Services;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace TutorCopiloto.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly IDeploymentService _deploymentService;
        private readonly ILogger<WebhookController> _logger;
        private readonly IConfiguration _configuration;

        public WebhookController(
            IDeploymentService deploymentService,
            ILogger<WebhookController> logger,
            IConfiguration configuration)
        {
            _deploymentService = deploymentService;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("github")]
        public async Task<IActionResult> HandleGitHubWebhook()
        {
            try
            {
                // Lê o corpo da requisição
                using var reader = new StreamReader(Request.Body);
                var payload = await reader.ReadToEndAsync();

                // Verifica a assinatura do webhook (se configurada)
                if (!await ValidateGitHubSignature(payload))
                {
                    _logger.LogWarning("Webhook GitHub com assinatura inválida");
                    return Unauthorized("Invalid signature");
                }

                // Parse do payload
                var webhookData = JsonSerializer.Deserialize<GitHubWebhookPayload>(payload);
                
                if (webhookData?.Repository == null)
                {
                    _logger.LogWarning("Webhook GitHub sem dados de repository");
                    return BadRequest("Invalid webhook payload");
                }

                _logger.LogInformation("Webhook GitHub recebido para: {Repository} - Event: {Event}", 
                    webhookData.Repository.FullName, 
                    Request.Headers["X-GitHub-Event"].FirstOrDefault());

                // Processa apenas eventos de push para branch main/master
                var githubEvent = Request.Headers["X-GitHub-Event"].FirstOrDefault();
                if (githubEvent == "push" && (webhookData.Ref == "refs/heads/main" || webhookData.Ref == "refs/heads/master"))
                {
                    var deployment = _deploymentService.CreateDeployment(new DeploymentRequest
                    {
                        RepositoryUrl = webhookData.Repository.CloneUrl ?? string.Empty,
                        Branch = webhookData.Ref.Replace("refs/heads/", ""),
                        CommitSha = webhookData.After,
                        Trigger = "webhook",
                        Author = webhookData.HeadCommit?.Author?.Name ?? "Unknown"
                    });

                    return Ok(new { deploymentId = deployment.Id, status = "started" });
                }

                return Ok(new { status = "ignored", reason = "Not a main/master branch push" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar webhook GitHub");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("manual-deploy")]
        public async Task<IActionResult> ManualDeploy([FromBody] ManualDeployRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.RepositoryUrl))
                {
                    return BadRequest("Repository URL is required");
                }

                _logger.LogInformation("Deploy manual iniciado para: {Repository}", request.RepositoryUrl);

                var deployment = await _deploymentService.CreateDeploymentAsync(new DeploymentRequest
                {
                    RepositoryUrl = request.RepositoryUrl,
                    Branch = request.Branch ?? "main",
                    Trigger = "manual",
                    Author = request.Author ?? "Manual Deploy"
                });

                return Ok(new { deploymentId = deployment.Id, status = "started" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar deploy manual");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("deployments")]
        public async Task<IActionResult> GetDeployments([FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            try
            {
                var deployments = await _deploymentService.GetDeploymentsAsync(page, size);
                return Ok(deployments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar deployments");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("deployments/{id}")]
        public async Task<IActionResult> GetDeployment(string id)
        {
            try
            {
                var deployment = await _deploymentService.GetDeploymentAsync(id);
                if (deployment == null)
                {
                    return NotFound();
                }

                return Ok(deployment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar deployment {DeploymentId}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("deployments/{id}/repository-analysis")]
        public async Task<IActionResult> GetRepositoryAnalysis(string id)
        {
            try
            {
                var analysis = await _deploymentService.GetRepositoryAnalysisAsync(id);
                if (analysis == null)
                {
                    return NotFound(new { error = "Análise do repositório não encontrada" });
                }

                return Ok(new { 
                    repositoryAnalysis = analysis,
                    summary = new
                    {
                        repositoryName = analysis.RepositoryName,
                        totalFiles = analysis.TotalFiles,
                        totalSize = analysis.TotalSize,
                        programmingLanguages = analysis.ProgrammingLanguages.Count,
                        status = analysis.Status,
                        analyzedAt = analysis.AnalyzedAt
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar análise do repositório {DeploymentId}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("deployments/{id}/analysis")]
        public async Task<IActionResult> GetDeploymentAnalysis(string id)
        {
            try
            {
                var analysis = await _deploymentService.GetDeploymentAnalysisAsync(id);
                if (analysis == null)
                {
                    return NotFound(new { message = "Análise não encontrada para este deployment" });
                }

                return Ok(analysis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar análise do deployment {DeploymentId}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        private async Task<bool> ValidateGitHubSignature(string payload)
        {
            var secret = _configuration["GitHub:WebhookSecret"];
            if (string.IsNullOrEmpty(secret))
            {
                _logger.LogWarning("GitHub webhook secret não configurado - pulando validação");
                return true; // Se não há secret configurado, aceita qualquer requisição
            }

            var signature = Request.Headers["X-Hub-Signature-256"].FirstOrDefault();
            if (string.IsNullOrEmpty(signature))
            {
                return false;
            }

            signature = signature.Replace("sha256=", "");
            
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var computedSignature = Convert.ToHexString(computedHash).ToLower();

            return signature.Equals(computedSignature, StringComparison.OrdinalIgnoreCase);
        }
    }

    // DTOs
    public class GitHubWebhookPayload
    {
        public string? Ref { get; set; }
        public string? After { get; set; }
        public Repository? Repository { get; set; }
        public Commit? HeadCommit { get; set; }
    }

    public class Repository
    {
        public string? Name { get; set; }
        public string? FullName { get; set; }
        public string? CloneUrl { get; set; }
    }

    public class Commit
    {
        public Author? Author { get; set; }
        public string? Message { get; set; }
    }

    public class Author
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
    }

    public class ManualDeployRequest
    {
        public string RepositoryUrl { get; set; } = string.Empty;
        public string? Branch { get; set; }
        public string? Author { get; set; }
    }
}
