using TutorCopiloto.Services.Dto;

namespace TutorCopiloto.Services.AgentTasks
{
    /// <summary>
    /// Interface para tarefas de agente autônomo
    /// </summary>
    public interface IAgentTask
    {
        /// <summary>
        /// Nome único do agente
        /// </summary>
        string AgentName { get; }

        /// <summary>
        /// Descrição do que o agente faz
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Prioridade de execução (menor número = maior prioridade)
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Verifica se o agente pode ser executado para o repositório
        /// </summary>
        Task<bool> CanExecuteAsync(string repositoryPath, RepositoryAnalysisContext context);

        /// <summary>
        /// Executa a tarefa do agente
        /// </summary>
        Task<AgentTaskResult> ExecuteAsync(string repositoryPath, RepositoryAnalysisContext context);

        /// <summary>
        /// Estima o tempo de execução
        /// </summary>
        TimeSpan EstimateExecutionTime();
    }

    /// <summary>
    /// Contexto para análise de repositório
    /// </summary>
    public class RepositoryAnalysisContext
    {
        public string RepositoryUrl { get; set; } = string.Empty;
        public string Branch { get; set; } = "main";
        public string RepositoryName { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public List<string> ProgrammingLanguages { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
        public bool IncludeAutoFix { get; set; } = false;
        public AgentTaskExecutionOptions Options { get; set; } = new();
    }

    /// <summary>
    /// Resultado da execução de uma tarefa de agente
    /// </summary>
    public class AgentTaskResult
    {
        public string AgentName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Success, Failed, Warning, Skipped
        public string Summary { get; set; } = string.Empty;
        public List<AgentFinding> Findings { get; set; } = new();
        public List<AgentFix> AppliedFixes { get; set; } = new();
        public List<AgentRecommendation> Recommendations { get; set; } = new();
        public TimeSpan ExecutionTime { get; set; }
        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
        public string ErrorMessage { get; set; } = string.Empty;
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }

    /// <summary>
    /// Descoberta/problema identificado pelo agente
    /// </summary>
    public class AgentFinding
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Type { get; set; } = string.Empty; // Bug, Security, Performance, Style, etc.
        public string Severity { get; set; } = string.Empty; // Critical, High, Medium, Low
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public int LineNumber { get; set; } = 0;
        public int ColumnNumber { get; set; } = 0;
        public string Code { get; set; } = string.Empty;
        public string Rule { get; set; } = string.Empty;
        public bool CanAutoFix { get; set; } = false;
        public string SuggestedFix { get; set; } = string.Empty;
    }

    /// <summary>
    /// Correção aplicada pelo agente
    /// </summary>
    public class AgentFix
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string FindingId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // CodeFix, ConfigurationFix, DocumentationFix
        public string Description { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string OriginalContent { get; set; } = string.Empty;
        public string FixedContent { get; set; } = string.Empty;
        public bool Applied { get; set; } = false;
        public string AppliedAt { get; set; } = string.Empty;
    }

    /// <summary>
    /// Recomendação do agente
    /// </summary>
    public class AgentRecommendation
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Type { get; set; } = string.Empty; // Architecture, Security, Performance, Best Practice
        public string Priority { get; set; } = string.Empty; // High, Medium, Low
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> ActionItems { get; set; } = new();
        public List<string> Resources { get; set; } = new();
        public string EstimatedEffort { get; set; } = string.Empty;
    }

    /// <summary>
    /// Opções de execução para tarefas de agente
    /// </summary>
    public class AgentTaskExecutionOptions
    {
        public bool IncludeAutoFix { get; set; } = false;
        public bool DryRun { get; set; } = false;
        public int MaxFindingsPerAgent { get; set; } = 100;
        public int MaxExecutionTimeMinutes { get; set; } = 30;
        public List<string> ExcludedAgents { get; set; } = new();
        public List<string> IncludedAgents { get; set; } = new();
        public Dictionary<string, object> AgentSpecificOptions { get; set; } = new();
    }
}