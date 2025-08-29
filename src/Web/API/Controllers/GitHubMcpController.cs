using Microsoft.AspNetCore.Mvc;
using TutorCopiloto.Services;

namespace TutorCopiloto.Controllers
{
    [ApiController]
    [Route("api/github-mcp")]
    public class GitHubMcpController : ControllerBase
    {
        private readonly GitHubMcpService _mcpService;
        private readonly ILogger<GitHubMcpController> _logger;

        public GitHubMcpController(
            GitHubMcpService mcpService,
            ILogger<GitHubMcpController> logger)
        {
            _mcpService = mcpService;
            _logger = logger;
        }

        /// <summary>
        /// Lists all available MCP tools
        /// </summary>
        [HttpGet("tools")]
        public async Task<IActionResult> ListTools()
        {
            try
            {
                _logger.LogInformation("Listing MCP tools");
                var tools = await _mcpService.ListToolsAsync();
                return Ok(new { tools, count = tools.Count });
            }
            catch (GitHubMcpException ex)
            {
                _logger.LogError(ex, "Error listing MCP tools");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error listing MCP tools");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Analyzes a GitHub repository using MCP tools
        /// </summary>
        [HttpPost("analyze-repository")]
        public async Task<IActionResult> AnalyzeRepository([FromBody] McpRepositoryRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _logger.LogInformation("Analyzing repository {Owner}/{Repo}:{Branch}",
                    request.Owner, request.Repo, request.Branch ?? "main");

                var analysis = await _mcpService.AnalyzeRepositoryAsync(
                    request.Owner,
                    request.Repo,
                    request.Branch ?? "main");

                return Ok(analysis);
            }
            catch (GitHubMcpException ex)
            {
                _logger.LogError(ex, "Error analyzing repository {Owner}/{Repo}", request.Owner, request.Repo);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error analyzing repository {Owner}/{Repo}", request.Owner, request.Repo);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Queries a GitHub repository using MCP tools
        /// </summary>
        [HttpPost("query-repository")]
        public async Task<IActionResult> QueryRepository([FromBody] McpQueryRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _logger.LogInformation("Querying repository {Owner}/{Repo} with query: {Query}",
                    request.Owner, request.Repo, request.Query);

                var result = await _mcpService.QueryRepositoryAsync(
                    request.Owner,
                    request.Repo,
                    request.Query,
                    request.Context ?? "");

                return Ok(result);
            }
            catch (GitHubMcpException ex)
            {
                _logger.LogError(ex, "Error querying repository {Owner}/{Repo}", request.Owner, request.Repo);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error querying repository {Owner}/{Repo}", request.Owner, request.Repo);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Executes a specific MCP tool
        /// </summary>
        [HttpPost("execute-tool")]
        public async Task<IActionResult> ExecuteTool([FromBody] McpToolExecutionRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _logger.LogInformation("Executing MCP tool: {ToolName}", request.ToolName);

                var result = await _mcpService.ExecuteToolAsync(
                    request.ToolName,
                    request.Parameters ?? new Dictionary<string, object>());

                return Ok(result);
            }
            catch (GitHubMcpException ex)
            {
                _logger.LogError(ex, "Error executing MCP tool: {ToolName}", request.ToolName);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error executing MCP tool: {ToolName}", request.ToolName);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Performs comprehensive analysis and querying of a repository
        /// </summary>
        [HttpPost("analyze-and-query")]
        public async Task<IActionResult> AnalyzeAndQuery([FromBody] McpAnalyzeAndQueryRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _logger.LogInformation("Starting comprehensive analysis for {Owner}/{Repo}",
                    request.Owner, request.Repo);

                // First analyze the repository
                var analysis = await _mcpService.AnalyzeRepositoryAsync(
                    request.Owner,
                    request.Repo,
                    request.Branch ?? "main");

                // Then perform the query if provided
                McpQueryResult? queryResult = null;
                if (!string.IsNullOrEmpty(request.Query))
                {
                    _logger.LogInformation("Performing query: {Query}", request.Query);
                    queryResult = await _mcpService.QueryRepositoryAsync(
                        request.Owner,
                        request.Repo,
                        request.Query,
                        request.Context ?? "");
                }

                var response = new
                {
                    repository = $"{request.Owner}/{request.Repo}",
                    branch = request.Branch ?? "main",
                    analysis = analysis,
                    queryResult = queryResult,
                    timestamp = DateTime.UtcNow
                };

                _logger.LogInformation("Comprehensive analysis completed for {Owner}/{Repo}", request.Owner, request.Repo);
                return Ok(response);
            }
            catch (GitHubMcpException ex)
            {
                _logger.LogError(ex, "Error in comprehensive analysis for {Owner}/{Repo}", request.Owner, request.Repo);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in comprehensive analysis for {Owner}/{Repo}", request.Owner, request.Repo);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }

    // Request models
    public class McpRepositoryRequest
    {
        public required string Owner { get; set; }
        public required string Repo { get; set; }
        public string? Branch { get; set; }
    }

    public class McpQueryRequest
    {
        public required string Owner { get; set; }
        public required string Repo { get; set; }
        public required string Query { get; set; }
        public string? Context { get; set; }
    }

    public class McpToolExecutionRequest
    {
        public required string ToolName { get; set; }
        public Dictionary<string, object>? Parameters { get; set; }
    }

    public class McpAnalyzeAndQueryRequest
    {
        public required string Owner { get; set; }
        public required string Repo { get; set; }
        public string? Branch { get; set; }
        public string? Query { get; set; }
        public string? Context { get; set; }
    }
}
