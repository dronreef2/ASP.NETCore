using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace TutorCopiloto.Services
{
    public interface IGitHubChatIntegrationService
    {
        Task<GitHubAnalysisResult> IndexRepositoryAsync(string repoUrl, string branch = "main");
        Task<GitHubAnalysisResult> QueryRepositoryAsync(string repoUrl, string question, List<ConversationMessage>? conversationHistory = null);
        Task<IntegrationStatus> GetIntegrationStatusAsync();
    }

    public class GitHubChatIntegrationService : IGitHubChatIntegrationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GitHubChatIntegrationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _integrationBaseUrl;

        public GitHubChatIntegrationService(
            HttpClient httpClient,
            ILogger<GitHubChatIntegrationService> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
            _integrationBaseUrl = _configuration["GitHubIntegration:BaseUrl"] ?? "http://localhost:8001";
        }

        public async Task<GitHubAnalysisResult> IndexRepositoryAsync(string repoUrl, string branch = "main")
        {
            try
            {
                _logger.LogInformation("Indexando repositório via GitHub Chat MCP: {RepoUrl}", repoUrl);

                var request = new
                {
                    repo_url = repoUrl,
                    branch = branch
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync($"{_integrationBaseUrl}/api/github/index", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<GitHubAnalysisResult>(responseContent);

                    _logger.LogInformation("Repositório indexado com sucesso: {RepoUrl}", repoUrl);
                    return result ?? new GitHubAnalysisResult { Success = true, Message = "Indexação iniciada" };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Erro ao indexar repositório: {StatusCode} - {Error}", response.StatusCode, errorContent);

                    return new GitHubAnalysisResult
                    {
                        Success = false,
                        Message = $"Erro na indexação: {response.StatusCode}",
                        Error = errorContent
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exceção ao indexar repositório: {RepoUrl}", repoUrl);
                return new GitHubAnalysisResult
                {
                    Success = false,
                    Message = "Erro interno do servidor",
                    Error = ex.Message
                };
            }
        }

        public async Task<GitHubAnalysisResult> QueryRepositoryAsync(string repoUrl, string question, List<ConversationMessage>? conversationHistory = null)
        {
            try
            {
                _logger.LogInformation("Consultando repositório via GitHub Chat MCP: {RepoUrl}", repoUrl);

                var request = new
                {
                    repo_url = repoUrl,
                    question = question,
                    conversation_history = conversationHistory?.Select(m => new
                    {
                        role = m.Role,
                        content = m.Content
                    }).ToList()
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync($"{_integrationBaseUrl}/api/github/query", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<GitHubAnalysisResult>(responseContent);

                    _logger.LogInformation("Consulta realizada com sucesso: {RepoUrl}", repoUrl);
                    return result ?? new GitHubAnalysisResult { Success = true, Message = "Consulta realizada" };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Erro ao consultar repositório: {StatusCode} - {Error}", response.StatusCode, errorContent);

                    return new GitHubAnalysisResult
                    {
                        Success = false,
                        Message = $"Erro na consulta: {response.StatusCode}",
                        Error = errorContent
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exceção ao consultar repositório: {RepoUrl}", repoUrl);
                return new GitHubAnalysisResult
                {
                    Success = false,
                    Message = "Erro interno do servidor",
                    Error = ex.Message
                };
            }
        }

        public async Task<IntegrationStatus> GetIntegrationStatusAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_integrationBaseUrl}/api/github/status");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var status = JsonSerializer.Deserialize<IntegrationStatus>(responseContent);

                    return status ?? new IntegrationStatus
                    {
                        Service = "github-chat-mcp-integration",
                        Status = "unknown",
                        IsActive = false
                    };
                }
                else
                {
                    return new IntegrationStatus
                    {
                        Service = "github-chat-mcp-integration",
                        Status = "error",
                        IsActive = false,
                        Error = $"HTTP {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar status da integração");
                return new IntegrationStatus
                {
                    Service = "github-chat-mcp-integration",
                    Status = "error",
                    IsActive = false,
                    Error = ex.Message
                };
            }
        }
    }

    // Modelos de dados
    public class GitHubAnalysisResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
        public string? Error { get; set; }
        public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    }

    public class ConversationMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public class IntegrationStatus
    {
        public string Service { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? Error { get; set; }
        public string? McpServer { get; set; }
        public bool? GithubApiConfigured { get; set; }
    }
}
