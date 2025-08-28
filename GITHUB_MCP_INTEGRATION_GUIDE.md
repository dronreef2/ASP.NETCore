# GitHub MCP Server Integration Guide

## Overview

This guide covers the integration of the **official GitHub MCP Server** into the ASP.NET Core Tutor Copiloto system. The official GitHub MCP Server provides comprehensive access to GitHub's platform through standardized Model Context Protocol (MCP) tools.

## Architecture

### Components

1. **GitHubMcpService** - Core service for communicating with GitHub MCP Server
2. **GitHubMcpController** - REST API endpoints for MCP operations
3. **Configuration** - appsettings.json configuration for MCP settings

### Supported Server Types

- **Remote Server** (Recommended): `https://api.githubcopilot.com/mcp/`
- **Local Server**: Custom Docker deployment at `http://localhost:8002`

## Configuration

### appsettings.json Setup

```json
{
  "GitHub": {
    "MCP": {
      "ServerType": "Remote",
      "RemoteUrl": "https://api.githubcopilot.com/mcp/",
      "LocalUrl": "http://localhost:8002",
      "TimeoutSeconds": 30,
      "RetryAttempts": 3,
      "Authentication": {
        "Type": "PAT",
        "Token": "your_github_token_here",
        "ClientId": "",
        "ClientSecret": ""
      }
    }
  }
}
```

### Authentication Options

#### Personal Access Token (PAT)
1. Go to GitHub Settings → Developer settings → Personal access tokens
2. Generate a new token with appropriate permissions:
   - `repo` - Full control of private repositories
   - `read:org` - Read org and team membership
   - `read:user` - Read user profile data
   - `read:project` - Read project boards
3. Set the token in configuration: `"Token": "ghp_your_token_here"`

#### OAuth (For Remote Server)
1. Create a GitHub OAuth App in your organization
2. Configure Client ID and Secret in appsettings.json
3. Implement OAuth flow in your application

## API Endpoints

### List Available Tools
```http
GET /api/github-mcp/tools
```

**Response:**
```json
{
  "tools": [
    {
      "name": "get_repository",
      "description": "Get repository information",
      "inputSchema": {
        "type": "object",
        "properties": {
          "owner": { "type": "string" },
          "repo": { "type": "string" }
        },
        "required": ["owner", "repo"]
      }
    }
  ],
  "count": 25
}
```

### Analyze Repository
```http
POST /api/github-mcp/analyze-repository
Content-Type: application/json

{
  "owner": "microsoft",
  "repo": "vscode",
  "branch": "main"
}
```

**Response:**
```json
{
  "repository": "microsoft/vscode",
  "branch": "main",
  "repositoryInfo": { ... },
  "recentCommits": { ... },
  "openIssues": { ... },
  "openPullRequests": { ... },
  "analysisTimestamp": "2025-08-28T10:30:00Z"
}
```

### Query Repository
```http
POST /api/github-mcp/query-repository
Content-Type: application/json

{
  "owner": "microsoft",
  "repo": "vscode",
  "query": "authentication",
  "context": "Looking for auth-related code"
}
```

### Execute Specific Tool
```http
POST /api/github-mcp/execute-tool
Content-Type: application/json

{
  "toolName": "search_code",
  "parameters": {
    "q": "function login",
    "repo": "microsoft/vscode"
  }
}
```

### Comprehensive Analysis & Query
```http
POST /api/github-mcp/analyze-and-query
Content-Type: application/json

{
  "owner": "microsoft",
  "repo": "vscode",
  "branch": "main",
  "query": "authentication",
  "context": "Security audit"
}
```

## Available MCP Tools

The official GitHub MCP Server provides the following toolsets:

### Repository Tools
- `get_repository` - Get repository information
- `list_branches` - List repository branches
- `list_commits` - List commits with filtering
- `get_file_contents` - Get file or directory contents
- `search_code` - Search for code patterns

### Issue & PR Tools
- `list_issues` - List repository issues
- `get_issue` - Get specific issue details
- `list_pull_requests` - List pull requests
- `get_pull_request` - Get pull request details
- `get_pull_request_diff` - Get PR diff
- `get_pull_request_files` - Get PR files

### Organization Tools
- `list_organization_repositories` - List org repos
- `get_organization` - Get organization info
- `list_team_repositories` - List team repositories

### Actions & CI/CD
- `list_workflows` - List GitHub Actions workflows
- `get_workflow` - Get workflow details
- `list_workflow_runs` - List workflow runs

### Security & Code Quality
- `list_code_scanning_alerts` - List security alerts
- `list_dependabot_alerts` - List dependency alerts
- `get_repository_vulnerability_alerts` - Get vulnerability info

## Local Server Setup (Alternative)

If you prefer to run a local MCP server instead of using the remote one:

### Using Docker
```bash
docker run -p 8002:8002 \
  -e GITHUB_PERSONAL_ACCESS_TOKEN=your_token \
  ghcr.io/github/github-mcp-server:latest
```

### Using Docker Compose
```yaml
version: '3.8'
services:
  github-mcp:
    image: ghcr.io/github/github-mcp-server:latest
    ports:
      - "8002:8002"
    environment:
      - GITHUB_PERSONAL_ACCESS_TOKEN=your_token
```

### Building from Source
```bash
git clone https://github.com/github/github-mcp-server.git
cd github-mcp-server
go build -o github-mcp-server cmd/github-mcp-server/main.go
GITHUB_PERSONAL_ACCESS_TOKEN=your_token ./github-mcp-server
```

## Usage Examples

### Example 1: Repository Analysis
```csharp
// Using the service directly
var analysis = await _mcpService.AnalyzeRepositoryAsync("octocat", "Hello-World");
Console.WriteLine($"Repository: {analysis.Repository}");
Console.WriteLine($"Open Issues: {analysis.OpenIssues?.Result?.Content}");
```

### Example 2: Code Search
```csharp
var searchResults = await _mcpService.ExecuteToolAsync("search_code", new Dictionary<string, object>
{
    ["q"] = "TODO",
    ["repo"] = "octocat/Hello-World"
});
```

### Example 3: Get Repository Structure
```csharp
var structure = await _mcpService.ExecuteToolAsync("list_repository_contents", new Dictionary<string, object>
{
    ["owner"] = "octocat",
    ["repo"] = "Hello-World",
    ["path"] = ""
});
```

## Error Handling

The service includes comprehensive error handling:

```csharp
try
{
    var result = await _mcpService.AnalyzeRepositoryAsync(owner, repo);
    return Ok(result);
}
catch (GitHubMcpException ex)
{
    // MCP-specific errors (authentication, API limits, etc.)
    return BadRequest(new { error = ex.Message });
}
catch (Exception ex)
{
    // General errors
    _logger.LogError(ex, "Unexpected error");
    return StatusCode(500, new { error = "Internal server error" });
}
```

## Security Considerations

1. **Token Storage**: Store GitHub tokens securely using Azure Key Vault or similar
2. **Token Scope**: Use minimal required permissions for PATs
3. **Rate Limiting**: The MCP server handles GitHub API rate limits automatically
4. **Audit Logging**: All API calls are logged in GitHub's audit logs

## Performance Optimization

1. **Connection Reuse**: HttpClient is injected as singleton for connection reuse
2. **Timeout Configuration**: Configurable timeouts prevent hanging requests
3. **Retry Logic**: Built-in retry mechanism for transient failures
4. **Caching**: Consider implementing response caching for frequently accessed data

## Troubleshooting

### Common Issues

1. **Authentication Failed**
   - Verify GitHub token is valid and has required permissions
   - Check token hasn't expired
   - Ensure token has correct scopes

2. **Tool Not Found**
   - Verify tool name is correct
   - Check if toolset is enabled in server configuration
   - Ensure you have appropriate permissions for the tool

3. **Rate Limit Exceeded**
   - Wait for rate limit reset (usually 1 hour)
   - Consider upgrading to higher rate limit tier
   - Implement request throttling in your application

4. **Network Timeout**
   - Increase timeout in configuration
   - Check network connectivity
   - Verify MCP server is running and accessible

### Debug Mode

Enable detailed logging by setting log level to Debug:

```json
{
  "Logging": {
    "LogLevel": {
      "TutorCopiloto.Services.GitHubMcpService": "Debug"
    }
  }
}
```

## Migration from Previous Integration

If migrating from the AsyncFuncAI/github-chat-mcp integration:

1. **Update Configuration**: Replace old GitHub integration config with new MCP config
2. **Update API Calls**: Change from custom endpoints to standardized MCP tools
3. **Update Authentication**: Switch from custom auth to PAT/OAuth
4. **Test Thoroughly**: Verify all functionality works with new integration

## Support and Resources

- [Official GitHub MCP Server Repository](https://github.com/github/github-mcp-server)
- [MCP Specification](https://modelcontextprotocol.io/specification/)
- [GitHub API Documentation](https://docs.github.com/en/rest)
- [Personal Access Token Guide](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/managing-your-personal-access-tokens)

---

## Next Steps

1. Configure authentication with your GitHub token
2. Test the API endpoints with a sample repository
3. Integrate the MCP tools into your application workflows
4. Set up monitoring and alerting for MCP operations
5. Consider implementing caching for improved performance</content>
<parameter name="filePath">/workspaces/ASP.NETCore/GITHUB_MCP_INTEGRATION_GUIDE.md
