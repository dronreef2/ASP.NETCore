using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TutorCopiloto.Services
{
    public class GitHubMcpService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GitHubMcpService> _logger;
        private readonly IConfiguration _configuration;

        public GitHubMcpService(
            HttpClient httpClient,
            ILogger<GitHubMcpService> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;

            // Configure HTTP client
            var timeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("GitHub:MCP:TimeoutSeconds", 30));
            _httpClient.Timeout = timeout;
        }

        public async Task<McpResponse> ExecuteToolAsync(string toolName, Dictionary<string, object> parameters)
        {
            try
            {
                var serverUrl = GetServerUrl();
                var authHeader = GetAuthenticationHeader();

                var request = new
                {
                    jsonrpc = "2.0",
                    id = Guid.NewGuid().ToString(),
                    method = "tools/call",
                    @params = new
                    {
                        name = toolName,
                        arguments = parameters
                    }
                };

                var jsonContent = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                if (!string.IsNullOrEmpty(authHeader))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authHeader);
                }

                _logger.LogInformation("Executing MCP tool: {ToolName} on {ServerUrl}", toolName, serverUrl);

                var response = await _httpClient.PostAsync(serverUrl, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<McpResponse>(responseContent);

                _logger.LogInformation("MCP tool execution completed: {ToolName}", toolName);
                return result ?? new McpResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing MCP tool: {ToolName}", toolName);
                throw new GitHubMcpException($"Failed to execute MCP tool {toolName}", ex);
            }
        }

        public async Task<List<McpTool>> ListToolsAsync()
        {
            try
            {
                var serverUrl = GetServerUrl();
                var authHeader = GetAuthenticationHeader();

                var request = new
                {
                    jsonrpc = "2.0",
                    id = Guid.NewGuid().ToString(),
                    method = "tools/list",
                    @params = new { }
                };

                var jsonContent = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                if (!string.IsNullOrEmpty(authHeader))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authHeader);
                }

                _logger.LogInformation("Listing MCP tools from {ServerUrl}", serverUrl);

                var response = await _httpClient.PostAsync(serverUrl, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<McpToolListResponse>(responseContent);

                _logger.LogInformation("Retrieved {Count} MCP tools", result?.Result?.Tools?.Count ?? 0);
                return result?.Result?.Tools ?? new List<McpTool>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing MCP tools");
                throw new GitHubMcpException("Failed to list MCP tools", ex);
            }
        }

        public async Task<McpRepositoryAnalysis> AnalyzeRepositoryAsync(string owner, string repo, string branch = "main")
        {
            try
            {
                _logger.LogInformation("Starting repository analysis for {Owner}/{Repo}:{Branch}", owner, repo, branch);

                // Get repository information
                var repoInfo = await ExecuteToolAsync("get_repository", new Dictionary<string, object>
                {
                    ["owner"] = owner,
                    ["repo"] = repo
                });

                // Get recent commits
                var commits = await ExecuteToolAsync("list_commits", new Dictionary<string, object>
                {
                    ["owner"] = owner,
                    ["repo"] = repo,
                    ["per_page"] = 10
                });

                // Get issues
                var issues = await ExecuteToolAsync("list_issues", new Dictionary<string, object>
                {
                    ["owner"] = owner,
                    ["repo"] = repo,
                    ["state"] = "open",
                    ["per_page"] = 20
                });

                // Get pull requests
                var pullRequests = await ExecuteToolAsync("list_pull_requests", new Dictionary<string, object>
                {
                    ["owner"] = owner,
                    ["repo"] = repo,
                    ["state"] = "open",
                    ["per_page"] = 10
                });

                var analysis = new McpRepositoryAnalysis
                {
                    Repository = $"{owner}/{repo}",
                    Branch = branch,
                    RepositoryInfo = repoInfo,
                    RecentCommits = commits,
                    OpenIssues = issues,
                    OpenPullRequests = pullRequests,
                    AnalysisTimestamp = DateTime.UtcNow
                };

                _logger.LogInformation("Repository analysis completed for {Owner}/{Repo}", owner, repo);
                return analysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing repository {Owner}/{Repo}", owner, repo);
                throw new GitHubMcpException($"Failed to analyze repository {owner}/{repo}", ex);
            }
        }

        public async Task<McpQueryResult> QueryRepositoryAsync(string owner, string repo, string query, string context = "")
        {
            try
            {
                _logger.LogInformation("Querying repository {Owner}/{Repo} with query: {Query}", owner, repo, query);

                // Search for files matching the query
                var fileSearch = await ExecuteToolAsync("search_code", new Dictionary<string, object>
                {
                    ["q"] = $"{query} repo:{owner}/{repo}",
                    ["per_page"] = 20
                });

                // Get repository structure
                var repoStructure = await ExecuteToolAsync("list_repository_contents", new Dictionary<string, object>
                {
                    ["owner"] = owner,
                    ["repo"] = repo,
                    ["path"] = ""
                });

                var result = new McpQueryResult
                {
                    Repository = $"{owner}/{repo}",
                    Query = query,
                    Context = context,
                    FileSearchResults = fileSearch,
                    RepositoryStructure = repoStructure,
                    QueryTimestamp = DateTime.UtcNow
                };

                _logger.LogInformation("Repository query completed for {Owner}/{Repo}", owner, repo);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying repository {Owner}/{Repo}", owner, repo);
                throw new GitHubMcpException($"Failed to query repository {owner}/{repo}", ex);
            }
        }

        private string GetServerUrl()
        {
            var serverType = _configuration.GetValue<string>("GitHub:MCP:ServerType", "Remote");

            return serverType switch
            {
                "Remote" => _configuration.GetValue<string>("GitHub:MCP:RemoteUrl", "https://api.githubcopilot.com/mcp/") ?? "https://api.githubcopilot.com/mcp/",
                "Local" => _configuration.GetValue<string>("GitHub:MCP:LocalUrl", "http://localhost:8002") ?? "http://localhost:8002",
                _ => throw new InvalidOperationException($"Unsupported server type: {serverType}")
            };
        }

        private string? GetAuthenticationHeader()
        {
            var authType = _configuration.GetValue<string>("GitHub:MCP:Authentication:Type", "PAT");

            return authType switch
            {
                "PAT" => _configuration.GetValue<string>("GitHub:MCP:Authentication:Token"),
                "OAuth" => _configuration.GetValue<string>("GitHub:MCP:Authentication:Token"),
                _ => null
            };
        }
    }

    // Data models
    public class McpResponse
    {
        public string? Jsonrpc { get; set; }
        public string? Id { get; set; }
        public McpResult? Result { get; set; }
        public McpError? Error { get; set; }
    }

    public class McpResult
    {
        public List<McpTool>? Tools { get; set; }
        public object? Content { get; set; }
    }

    public class McpTool
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public McpToolInputSchema? InputSchema { get; set; }
    }

    public class McpToolInputSchema
    {
        public string? Type { get; set; }
        public Dictionary<string, McpToolProperty>? Properties { get; set; }
        public List<string>? Required { get; set; }
    }

    public class McpToolProperty
    {
        public string? Type { get; set; }
        public string? Description { get; set; }
    }

    public class McpError
    {
        public int Code { get; set; }
        public string? Message { get; set; }
        public object? Data { get; set; }
    }

    public class McpToolListResponse
    {
        public string? Jsonrpc { get; set; }
        public string? Id { get; set; }
        public McpToolListResult? Result { get; set; }
        public McpError? Error { get; set; }
    }

    public class McpToolListResult
    {
        public List<McpTool>? Tools { get; set; }
    }

    public class McpRepositoryAnalysis
    {
        public string Repository { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public McpResponse RepositoryInfo { get; set; } = new();
        public McpResponse RecentCommits { get; set; } = new();
        public McpResponse OpenIssues { get; set; } = new();
        public McpResponse OpenPullRequests { get; set; } = new();
        public DateTime AnalysisTimestamp { get; set; }
    }

    public class McpQueryResult
    {
        public string Repository { get; set; } = string.Empty;
        public string Query { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
        public McpResponse FileSearchResults { get; set; } = new();
        public McpResponse RepositoryStructure { get; set; } = new();
        public DateTime QueryTimestamp { get; set; }
    }

    public class GitHubMcpException : Exception
    {
        public GitHubMcpException(string message) : base(message) { }
        public GitHubMcpException(string message, Exception innerException) : base(message, innerException) { }
    }
}
