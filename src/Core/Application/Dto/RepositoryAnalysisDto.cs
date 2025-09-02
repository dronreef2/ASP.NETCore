using System.ComponentModel.DataAnnotations;

namespace TutorCopiloto.Services.Dto
{
    /// <summary>
    /// DTO para solicitar análise de um repositório
    /// </summary>
    public class RepositoryAnalysisRequest
    {
        [Required]
        [MaxLength(200)]
        public string RepositoryUrl { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public bool ForceReanalysis { get; set; } = false;
    }

    /// <summary>
    /// DTO para resposta de análise de repositório
    /// </summary>
    public class RepositoryAnalysisResponse
    {
        public int RepositoryId { get; set; }
        public int AnalysisReportId { get; set; }
        public string RepositoryName { get; set; } = string.Empty;
        public string RepositoryUrl { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public decimal QualityScore { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para listagem de repositórios analisados
    /// </summary>
    public class RepositorySummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public int Stars { get; set; }
        public decimal LastQualityScore { get; set; }
        public DateTime LastAnalyzedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO detalhado de relatório de análise
    /// </summary>
    public class AnalysisReportDto
    {
        public int Id { get; set; }
        public int RepositoryId { get; set; }
        public string RepositoryName { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }

        // Métricas gerais
        public int TotalLinesOfCode { get; set; }
        public int FilesCount { get; set; }
        public decimal QualityScore { get; set; }

        // Conformidade Debian
        public bool HasDebianPackaging { get; set; }
        public int LintianErrors { get; set; }
        public int LintianWarnings { get; set; }
        public int LintianInfo { get; set; }

        // Segurança
        public int SecurityIssues { get; set; }
        public int CriticalSecurityIssues { get; set; }

        // Testes e CI/CD
        public bool HasTests { get; set; }
        public bool HasCI { get; set; }
        public decimal TestCoverage { get; set; }

        // Documentação
        public bool HasReadme { get; set; }
        public bool HasDocumentation { get; set; }

        // Listas detalhadas
        public List<LintianFindingDto> LintianFindings { get; set; } = new();
        public List<BugReportDto> BugReports { get; set; } = new();
        public List<CodeMetricDto> CodeMetrics { get; set; } = new();
    }

    /// <summary>
    /// DTO para findings do Lintian
    /// </summary>
    public class LintianFindingDto
    {
        public string Severity { get; set; } = string.Empty;
        public string Tag { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public int LineNumber { get; set; }
    }

    /// <summary>
    /// DTO para relatórios de bug
    /// </summary>
    public class BugReportDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public int LineNumber { get; set; }
        public bool IsFixed { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO para métricas de código
    /// </summary>
    public class CodeMetricDto
    {
        public string Language { get; set; } = string.Empty;
        public int Files { get; set; }
        public int Lines { get; set; }
        public int CodeLines { get; set; }
        public int CommentLines { get; set; }
        public int BlankLines { get; set; }
        public int Complexity { get; set; }
    }

    /// <summary>
    /// DTO para filtros de busca de repositórios
    /// </summary>
    public class RepositoryFilterDto
    {
        public string? Language { get; set; }
        public int? MinStars { get; set; }
        public int? MaxStars { get; set; }
        public decimal? MinQualityScore { get; set; }
        public string? SortBy { get; set; } // "stars", "quality", "lastAnalyzed"
        public string? SortOrder { get; set; } // "asc", "desc"
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// DTO para estatísticas gerais da plataforma
    /// </summary>
    public class PlatformStatsDto
    {
        public int TotalRepositories { get; set; }
        public int AnalyzedRepositories { get; set; }
        public int PendingAnalyses { get; set; }
        public decimal AverageQualityScore { get; set; }
        public Dictionary<string, int> LanguageDistribution { get; set; } = new();
        public Dictionary<string, int> QualityScoreDistribution { get; set; } = new();
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// DTO para resposta de busca no GitHub
    /// </summary>
    public class GitHubSearchResponse
    {
        public int TotalCount { get; set; }
        public bool IncompleteResults { get; set; }
        public List<GitHubRepositoryDto> Items { get; set; } = new();
    }

    /// <summary>
    /// DTO para repositório do GitHub
    /// </summary>
    public class GitHubRepositoryDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string HtmlUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public int Stars { get; set; }
        public int Forks { get; set; }
        public int OpenIssues { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public GitHubOwnerDto Owner { get; set; } = new();
    }

    /// <summary>
    /// DTO para proprietário de repositório do GitHub
    /// </summary>
    public class GitHubOwnerDto
    {
        public long Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public string HtmlUrl { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para solicitação de análise em lote
    /// </summary>
    public class BatchAnalysisRequest
    {
        [Required]
        public List<string> RepositoryUrls { get; set; } = new();

        public bool ForceReanalysis { get; set; } = false;
        public int MaxConcurrentAnalyses { get; set; } = 3;
    }

    /// <summary>
    /// DTO para resposta de análise em lote
    /// </summary>
    public class BatchAnalysisResponse
    {
        public int TotalRequested { get; set; }
        public int SuccessfullyQueued { get; set; }
        public int FailedToQueue { get; set; }
        public List<string> Errors { get; set; } = new();
        public string BatchId { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
