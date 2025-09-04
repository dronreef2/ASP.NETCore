# Agent Task System Documentation

## Overview

The Agent Task System provides a comprehensive framework for automated repository review and fixing using autonomous AI-powered agents. The system allows for parallel execution of multiple specialized agents that analyze different aspects of code repositories.

## Architecture

### Core Components

1. **IAgentTask Interface**: Defines the contract for all agent implementations
2. **BaseAgentTask**: Abstract base class providing common functionality
3. **AgentTaskOrchestrator**: Coordinates execution of multiple agents
4. **Specific Agents**: Specialized implementations for different types of analysis

### Available Agents

#### CodeReviewAgent
- **Priority**: 1 (Highest)
- **Purpose**: AI-powered code analysis for quality, bugs, and improvement opportunities
- **Features**:
  - Static code analysis for common issues
  - AI-enhanced deep analysis
  - Support for multiple programming languages
  - Automated fix suggestions for simple issues

#### SecurityAnalysisAgent  
- **Priority**: 2 (High)
- **Purpose**: Comprehensive security analysis
- **Features**:
  - Credential detection in code and config files
  - Dependency vulnerability scanning
  - Insecure configuration detection
  - Code pattern security analysis

#### DocumentationAgent
- **Priority**: 5 (Lower)
- **Purpose**: Documentation quality analysis and improvement suggestions
- **Features**:
  - README quality assessment
  - API documentation completeness
  - Code comment analysis
  - Missing documentation detection

## API Endpoints

### GET /api/agenttask/agents
Lists all available agents with their information.

**Response**:
```json
{
  "agents": [
    {
      "name": "Code Review Agent",
      "description": "Analisa código fonte e identifica problemas de qualidade...",
      "priority": 1,
      "estimatedExecutionTime": "00:10:00"
    }
  ],
  "estimatedTotalTime": "00:23:00",
  "totalAgents": 3
}
```

### POST /api/agenttask/execute-full-analysis
Executes all applicable agents for a repository.

**Request**:
```json
{
  "repositoryUrl": "https://github.com/user/repo",
  "branch": "main",
  "includeAutoFix": false,
  "dryRun": false,
  "maxFindingsPerAgent": 100,
  "maxExecutionTimeMinutes": 30,
  "excludedAgents": [],
  "includedAgents": []
}
```

**Response**:
```json
{
  "executionId": "uuid",
  "status": "Completed",
  "startedAt": "2025-01-01T00:00:00Z",
  "completedAt": "2025-01-01T00:10:00Z",
  "totalExecutionTime": "00:10:00",
  "repositoryUrl": "https://github.com/user/repo",
  "agentResults": [...],
  "consolidatedReport": {...}
}
```

### POST /api/agenttask/execute-specific-agents
Executes only specified agents.

**Request**:
```json
{
  "repositoryUrl": "https://github.com/user/repo",
  "branch": "main",
  "agentNames": ["Code Review Agent", "Security Analysis Agent"],
  "includeAutoFix": false,
  "dryRun": false
}
```

### POST /api/agenttask/estimate-execution-time
Estimates execution time for a set of agents.

## Agent Results Structure

Each agent execution produces:

- **Findings**: Issues, bugs, or problems identified
- **Applied Fixes**: Automatic fixes that were applied (if enabled)
- **Recommendations**: Improvement suggestions and best practices
- **Execution Metadata**: Timing, status, and error information

## Configuration

Agents are automatically registered in the DI container:

```csharp
// In Program.cs
builder.Services.AddScoped<CodeReviewAgent>();
builder.Services.AddScoped<SecurityAnalysisAgent>();
builder.Services.AddScoped<DocumentationAgent>();
builder.Services.AddScoped<AgentTaskOrchestrator>();
```

## Usage Examples

### Basic Repository Analysis
```bash
curl -X POST http://localhost:5000/api/agenttask/execute-full-analysis \
  -H "Content-Type: application/json" \
  -d '{"repositoryUrl": "https://github.com/user/repo"}'
```

### Security-Only Analysis
```bash
curl -X POST http://localhost:5000/api/agenttask/execute-specific-agents \
  -H "Content-Type: application/json" \
  -d '{
    "repositoryUrl": "https://github.com/user/repo",
    "agentNames": ["Security Analysis Agent"]
  }'
```

## Extensibility

To add a new agent:

1. Implement `IAgentTask` or inherit from `BaseAgentTask`
2. Register in DI container
3. Add to orchestrator initialization

Example:
```csharp
public class PerformanceAgent : BaseAgentTask
{
    public override string AgentName => "Performance Agent";
    public override string Description => "Analyzes performance issues";
    public override int Priority => 3;
    
    protected override async Task<bool> CanExecuteInternalAsync(string repositoryPath, RepositoryAnalysisContext context)
    {
        // Implementation
    }
    
    protected override async Task<AgentTaskResult> ExecuteInternalAsync(string repositoryPath, RepositoryAnalysisContext context)
    {
        // Implementation
    }
}
```

## Error Handling

The system includes comprehensive error handling:

- Individual agent failures don't stop other agents
- Timeout protection for long-running analyses
- Graceful degradation when AI services are unavailable
- Detailed error reporting in results

## Performance Considerations

- Agents run concurrently (limited by processor count)
- File size limits prevent excessive AI token usage
- Configurable timeouts and finding limits
- Efficient file pattern matching and filtering