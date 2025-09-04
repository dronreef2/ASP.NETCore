using System.ComponentModel.DataAnnotations;
using TutorCopiloto.Services.AgentTasks;

namespace TutorCopiloto.Services.Dto.AgentTasks
{
    /// <summary>
    /// DTO para solicitação de execução de agentes
    /// </summary>
    public class AgentTaskExecutionRequestDto
    {
        [Required]
        [MaxLength(500)]
        public string RepositoryUrl { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Branch { get; set; } = "main";

        public List<string> IncludedAgents { get; set; } = new();
        public List<string> ExcludedAgents { get; set; } = new();
        
        public bool IncludeAutoFix { get; set; } = false;
        public bool DryRun { get; set; } = false;
        
        public int MaxFindingsPerAgent { get; set; } = 100;
        public int MaxExecutionTimeMinutes { get; set; } = 30;
        
        public Dictionary<string, object> AgentSpecificOptions { get; set; } = new();
    }

    /// <summary>
    /// DTO para resposta de execução de agentes
    /// </summary>
    public class AgentTaskExecutionResponseDto
    {
        public string ExecutionId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public TimeSpan? TotalExecutionTime { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        
        public string RepositoryUrl { get; set; } = string.Empty;
        public string RepositoryName { get; set; } = string.Empty;
        
        public List<AgentTaskResultDto> AgentResults { get; set; } = new();
        public AgentTaskConsolidatedReportDto? ConsolidatedReport { get; set; }
    }

    /// <summary>
    /// DTO para resultado de execução de um agente
    /// </summary>
    public class AgentTaskResultDto
    {
        public string AgentName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        
        public TimeSpan ExecutionTime { get; set; }
        public DateTime ExecutedAt { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        
        public List<AgentFindingDto> Findings { get; set; } = new();
        public List<AgentFixDto> AppliedFixes { get; set; } = new();
        public List<AgentRecommendationDto> Recommendations { get; set; } = new();
        
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }

    /// <summary>
    /// DTO para descoberta/problema identificado pelo agente
    /// </summary>
    public class AgentFindingDto
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        public string FilePath { get; set; } = string.Empty;
        public int LineNumber { get; set; }
        public int ColumnNumber { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Rule { get; set; } = string.Empty;
        
        public bool CanAutoFix { get; set; }
        public string SuggestedFix { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para correção aplicada pelo agente
    /// </summary>
    public class AgentFixDto
    {
        public string Id { get; set; } = string.Empty;
        public string FindingId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        public string FilePath { get; set; } = string.Empty;
        public string OriginalContent { get; set; } = string.Empty;
        public string FixedContent { get; set; } = string.Empty;
        
        public bool Applied { get; set; }
        public string AppliedAt { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para recomendação do agente
    /// </summary>
    public class AgentRecommendationDto
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        public List<string> ActionItems { get; set; } = new();
        public List<string> Resources { get; set; } = new();
        public string EstimatedEffort { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para relatório consolidado
    /// </summary>
    public class AgentTaskConsolidatedReportDto
    {
        public int TotalAgentsExecuted { get; set; }
        public int SuccessfulAgents { get; set; }
        public int FailedAgents { get; set; }
        public int SkippedAgents { get; set; }
        
        public int TotalFindings { get; set; }
        public int CriticalFindings { get; set; }
        public int HighFindings { get; set; }
        public int MediumFindings { get; set; }
        public int LowFindings { get; set; }
        
        public int TotalRecommendations { get; set; }
        public int HighPriorityRecommendations { get; set; }
        
        public TimeSpan TotalExecutionTime { get; set; }
        public TimeSpan AverageExecutionTimePerAgent { get; set; }
        
        public Dictionary<string, int> FindingsByType { get; set; } = new();
        public Dictionary<string, int> FindingsByAgent { get; set; } = new();
        public Dictionary<string, int> RecommendationsByType { get; set; } = new();
        
        public string ExecutiveSummary { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para informações de agente disponível
    /// </summary>
    public class AgentInfoDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Priority { get; set; }
        public TimeSpan EstimatedExecutionTime { get; set; }
    }

    /// <summary>
    /// DTO para listagem de agentes disponíveis
    /// </summary>
    public class AvailableAgentsResponseDto
    {
        public List<AgentInfoDto> Agents { get; set; } = new();
        public TimeSpan EstimatedTotalTime { get; set; }
        public int TotalAgents { get; set; }
    }

    /// <summary>
    /// DTO para solicitação de análise de agente específico
    /// </summary>
    public class SpecificAgentAnalysisRequestDto
    {
        [Required]
        [MaxLength(500)]
        public string RepositoryUrl { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Branch { get; set; } = "main";

        [Required]
        public List<string> AgentNames { get; set; } = new();

        public bool IncludeAutoFix { get; set; } = false;
        public bool DryRun { get; set; } = false;

        public Dictionary<string, object> AgentSpecificOptions { get; set; } = new();
    }
}