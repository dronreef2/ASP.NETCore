using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Threading.Tasks;
using TutorCopiloto.Services;
using Microsoft.Extensions.Logging;

namespace TutorCopiloto.Controllers
{
    [ApiController]
    [Route("api/github-integration")]
    public class GitHubIntegrationController : ControllerBase
    {
        private readonly IGitHubChatIntegrationService _githubIntegration;
        private readonly ILogger<GitHubIntegrationController> _logger;

        public GitHubIntegrationController(
            IGitHubChatIntegrationService githubIntegration,
            ILogger<GitHubIntegrationController> logger)
        {
            _githubIntegration = githubIntegration;
            _logger = logger;
        }

        [HttpPost("index")]
        public async Task<IActionResult> IndexRepository([FromBody] IndexRepositoryRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.RepoUrl))
                {
                    return BadRequest(new { error = "URL do repositório é obrigatória" });
                }

                _logger.LogInformation("Indexando repositório: {RepoUrl}", request.RepoUrl);

                var result = await _githubIntegration.IndexRepositoryAsync(
                    request.RepoUrl,
                    request.Branch ?? "main"
                );

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = result.Message,
                        data = result.Data,
                        analyzedAt = result.AnalyzedAt
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = result.Message,
                        error = result.Error
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao indexar repositório: {RepoUrl}", request.RepoUrl);
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }

        [HttpPost("query")]
        public async Task<IActionResult> QueryRepository([FromBody] QueryRepositoryRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.RepoUrl) || string.IsNullOrEmpty(request.Question))
                {
                    return BadRequest(new { error = "URL do repositório e pergunta são obrigatórios" });
                }

                _logger.LogInformation("Consultando repositório: {RepoUrl}", request.RepoUrl);

                var result = await _githubIntegration.QueryRepositoryAsync(
                    request.RepoUrl,
                    request.Question,
                    request.ConversationHistory
                );

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = result.Message,
                        data = result.Data,
                        analyzedAt = result.AnalyzedAt
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = result.Message,
                        error = result.Error
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar repositório: {RepoUrl}", request.RepoUrl);
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            try
            {
                var status = await _githubIntegration.GetIntegrationStatusAsync();

                return Ok(new
                {
                    service = status.Service,
                    status = status.Status,
                    isActive = status.IsActive,
                    mcpServer = status.McpServer,
                    githubApiConfigured = status.GithubApiConfigured,
                    error = status.Error
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar status da integração");
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }

        [HttpPost("analyze-and-query")]
        public async Task<IActionResult> AnalyzeAndQuery([FromBody] AnalyzeAndQueryRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.RepoUrl))
                {
                    return BadRequest(new { error = "URL do repositório é obrigatória" });
                }

                _logger.LogInformation("Análise completa do repositório: {RepoUrl}", request.RepoUrl);

                // Primeiro, indexa o repositório
                var indexResult = await _githubIntegration.IndexRepositoryAsync(
                    request.RepoUrl,
                    request.Branch ?? "main"
                );

                if (!indexResult.Success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Falha na indexação do repositório",
                        error = indexResult.Error
                    });
                }

                // Aguarda um pouco para a indexação completar
                await Task.Delay(2000);

                // Faz perguntas padrão sobre o repositório
                var questions = request.Questions ?? new List<string>
                {
                    "Qual é a arquitetura principal deste projeto?",
                    "Quais são as principais tecnologias utilizadas?",
                    "Como está estruturado o projeto?",
                    "Quais são os pontos de entrada da aplicação?"
                };

                var queryResults = new List<object>();

                foreach (var question in questions)
                {
                    try
                    {
                        var queryResult = await _githubIntegration.QueryRepositoryAsync(
                            request.RepoUrl,
                            question,
                            request.ConversationHistory
                        );

                        queryResults.Add(new
                        {
                            question = question,
                            success = queryResult.Success,
                            answer = queryResult.Data,
                            error = queryResult.Error
                        });
                    }
                    catch (Exception ex)
                    {
                        queryResults.Add(new
                        {
                            question = question,
                            success = false,
                            answer = (object?)null,
                            error = ex.Message
                        });
                    }
                }

                return Ok(new
                {
                    success = true,
                    message = "Análise completa realizada com sucesso",
                    repository = request.RepoUrl,
                    indexing = new
                    {
                        success = indexResult.Success,
                        message = indexResult.Message
                    },
                    queries = queryResults,
                    analyzedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na análise completa: {RepoUrl}", request.RepoUrl);
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }
    }

    // Modelos de request
    public class IndexRepositoryRequest
    {
        public string RepoUrl { get; set; } = string.Empty;
        public string? Branch { get; set; }
    }

    public class QueryRepositoryRequest
    {
        public string RepoUrl { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public List<ConversationMessage>? ConversationHistory { get; set; }
    }

    public class AnalyzeAndQueryRequest
    {
        public string RepoUrl { get; set; } = string.Empty;
        public string? Branch { get; set; }
        public List<string>? Questions { get; set; }
        public List<ConversationMessage>? ConversationHistory { get; set; }
    }
}
