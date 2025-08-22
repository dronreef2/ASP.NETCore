using TutorCopiloto.Models;

namespace TutorCopiloto.Services;

/// <summary>
/// Interface para serviços avançados de análise de qualidade de código
/// </summary>
public interface ICodeQualityService
{
    // Code Analysis Operations
    Task<CodeQualityReport> AnalyzeCodeAsync(string code, string language, CodeAnalysisOptions? options = null);
    Task<CodeQualityReport> AnalyzeFileAsync(string filePath, CodeAnalysisOptions? options = null);
    Task<ProjectQualityReport> AnalyzeProjectAsync(string projectPath, CodeAnalysisOptions? options = null);
    
    // Metrics and Insights
    Task<ComplexityMetrics> CalculateComplexityAsync(string code, string language);
    Task<CodeSecurityAnalysis> AnalyzeSecurityAsync(string code, string language);
    Task<PerformanceAnalysisResult> AnalyzePerformanceAsync(string code, string language);
    Task<MaintainabilityScore> CalculateMaintainabilityAsync(string code, string language);
    
    // Code Patterns and Best Practices
    Task<List<CodePattern>> DetectPatternsAsync(string code, string language);
    Task<List<BestPracticeViolation>> CheckBestPracticesAsync(string code, string language);
    Task<List<CodeSmell>> DetectCodeSmellsAsync(string code, string language);
    
    // Improvement Suggestions
    Task<List<ImprovementSuggestion>> GetImprovementSuggestionsAsync(string code, string language);
    Task<RefactoringRecommendation> GetRefactoringRecommendationsAsync(string code, string language);
    
    // Historical Analysis
    Task<QualityTrend> GetQualityTrendAsync(string userId, string projectId, DateTime fromDate, DateTime toDate);
    Task<List<QualityInsight>> GetQualityInsightsAsync(string userId, int maxResults = 10);
    
    // Team Analytics
    Task<TeamQualityMetrics> GetTeamQualityMetricsAsync(string teamId);
    Task<List<QualityLeaderboard>> GetQualityLeaderboardAsync(string teamId);
}

/// <summary>
/// Opções para análise de código
/// </summary>
public class CodeAnalysisOptions
{
    public bool IncludeSecurityAnalysis { get; set; } = true;
    public bool IncludePerformanceAnalysis { get; set; } = true;
    public bool IncludeMaintainabilityAnalysis { get; set; } = true;
    public bool IncludeComplexityAnalysis { get; set; } = true;
    public bool IncludeBestPracticesCheck { get; set; } = true;
    public bool IncludeCodeSmellDetection { get; set; } = true;
    public List<string> ExcludeRules { get; set; } = new();
    public List<string> IncludeOnlyRules { get; set; } = new();
    public QualityLevel MinimumQualityLevel { get; set; } = QualityLevel.Info;
}

/// <summary>
/// Relatório de qualidade de código
/// </summary>
public class CodeQualityReport
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime AnalyzedAt { get; set; }
    public QualityScore OverallScore { get; set; } = new();
    public ComplexityMetrics Complexity { get; set; } = new();
    public CodeSecurityAnalysis Security { get; set; } = new();
    public PerformanceAnalysisResult Performance { get; set; } = new();
    public MaintainabilityScore Maintainability { get; set; } = new();
    public List<CodeIssue> Issues { get; set; } = new();
    public List<ImprovementSuggestion> Suggestions { get; set; } = new();
    public TimeSpan AnalysisDuration { get; set; }
}

/// <summary>
/// Relatório de qualidade de projeto
/// </summary>
public class ProjectQualityReport
{
    public string ProjectId { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public DateTime AnalyzedAt { get; set; }
    public QualityScore OverallScore { get; set; } = new();
    public List<CodeQualityReport> FileReports { get; set; } = new();
    public ProjectMetrics Metrics { get; set; } = new();
    public List<ProjectIssue> ProjectIssues { get; set; } = new();
    public QualityTrend Trend { get; set; } = new();
}

/// <summary>
/// Score de qualidade
/// </summary>
public class QualityScore
{
    public double Overall { get; set; }
    public double Security { get; set; }
    public double Performance { get; set; }
    public double Maintainability { get; set; }
    public double Complexity { get; set; }
    public QualityRating Rating { get; set; }
    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// Métricas de complexidade
/// </summary>
public class ComplexityMetrics
{
    public int CyclomaticComplexity { get; set; }
    public int CognitiveComplexity { get; set; }
    public int LinesOfCode { get; set; }
    public int NumberOfMethods { get; set; }
    public int NumberOfClasses { get; set; }
    public double AverageMethodComplexity { get; set; }
    public List<ComplexMethod> MostComplexMethods { get; set; } = new();
}

/// <summary>
/// Resultado de análise de segurança
/// </summary>
public class CodeSecurityAnalysis
{
    public int VulnerabilityCount { get; set; }
    public SecurityRisk RiskLevel { get; set; }
    public List<SecurityVulnerability> Vulnerabilities { get; set; } = new();
    public List<SecurityRecommendation> Recommendations { get; set; } = new();
}

/// <summary>
/// Resultado de análise de performance
/// </summary>
public class PerformanceAnalysisResult
{
    public PerformanceRating Rating { get; set; }
    public List<PerformanceIssue> Issues { get; set; } = new();
    public List<PerformanceOptimization> Optimizations { get; set; } = new();
    public EstimatedMetrics EstimatedMetrics { get; set; } = new();
}

/// <summary>
/// Score de manutenibilidade
/// </summary>
public class MaintainabilityScore
{
    public double Score { get; set; }
    public MaintainabilityRating Rating { get; set; }
    public List<MaintainabilityFactor> Factors { get; set; } = new();
    public List<string> ImprovementAreas { get; set; } = new();
}

/// <summary>
/// Padrão de código detectado
/// </summary>
public class CodePattern
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public int ColumnNumber { get; set; }
    public PatternConfidence Confidence { get; set; }
    public List<string> Examples { get; set; } = new();
}

/// <summary>
/// Violação de melhores práticas
/// </summary>
public class BestPracticeViolation
{
    public string Rule { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public QualityLevel Severity { get; set; }
    public int LineNumber { get; set; }
    public int ColumnNumber { get; set; }
    public string Suggestion { get; set; } = string.Empty;
    public List<string> References { get; set; } = new();
}

/// <summary>
/// Code smell detectado
/// </summary>
public class CodeSmell
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SmellSeverity Severity { get; set; }
    public int LineNumber { get; set; }
    public string RefactoringAdvice { get; set; } = string.Empty;
    public double ImpactScore { get; set; }
}

/// <summary>
/// Sugestão de melhoria
/// </summary>
public class ImprovementSuggestion
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ImprovementCategory Category { get; set; }
    public ImprovementPriority Priority { get; set; }
    public string BeforeCode { get; set; } = string.Empty;
    public string AfterCode { get; set; } = string.Empty;
    public List<string> Benefits { get; set; } = new();
    public int EstimatedEffortMinutes { get; set; }
}

/// <summary>
/// Recomendação de refatoração
/// </summary>
public class RefactoringRecommendation
{
    public List<RefactoringOpportunity> Opportunities { get; set; } = new();
    public RefactoringPriority Priority { get; set; }
    public int EstimatedEffortHours { get; set; }
    public List<string> ExpectedBenefits { get; set; } = new();
}

/// <summary>
/// Tendência de qualidade
/// </summary>
public class QualityTrend
{
    public List<QualityDataPoint> DataPoints { get; set; } = new();
    public TrendDirection Direction { get; set; }
    public double ChangePercentage { get; set; }
    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// Insight de qualidade
/// </summary>
public class QualityInsight
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public InsightType Type { get; set; }
    public InsightPriority Priority { get; set; }
    public DateTime GeneratedAt { get; set; }
    public List<string> ActionItems { get; set; } = new();
}

/// <summary>
/// Métricas de qualidade da equipe
/// </summary>
public class TeamQualityMetrics
{
    public string TeamId { get; set; } = string.Empty;
    public double AverageQualityScore { get; set; }
    public int TotalLinesAnalyzed { get; set; }
    public int TotalIssuesFound { get; set; }
    public int TotalIssuesFixed { get; set; }
    public List<TeamMemberMetrics> MemberMetrics { get; set; } = new();
    public QualityTrend TeamTrend { get; set; } = new();
}

/// <summary>
/// Leaderboard de qualidade
/// </summary>
public class QualityLeaderboard
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public double QualityScore { get; set; }
    public int Rank { get; set; }
    public int ImprovementPoints { get; set; }
    public List<string> Achievements { get; set; } = new();
}

// Enums
public enum QualityLevel { Info, Low, Medium, High, Critical }
public enum QualityRating { Excellent, Good, Fair, Poor, Critical }
public enum SecurityRisk { Low, Medium, High, Critical }
public enum PerformanceRating { Excellent, Good, Average, Poor, Critical }
public enum MaintainabilityRating { VeryHigh, High, Medium, Low, VeryLow }
public enum PatternConfidence { Low, Medium, High, Certain }
public enum SmellSeverity { Minor, Major, Critical }
public enum ImprovementCategory { Security, Performance, Maintainability, Style, Architecture }
public enum ImprovementPriority { Low, Medium, High, Critical }
public enum RefactoringPriority { Optional, Recommended, Important, Critical }
public enum TrendDirection { Improving, Stable, Declining }
public enum InsightType { Achievement, Warning, Recommendation, Trend }
public enum InsightPriority { Low, Medium, High }

// Supporting Classes
public class ComplexMethod
{
    public string Name { get; set; } = string.Empty;
    public int Complexity { get; set; }
    public int LineNumber { get; set; }
}

public class SecurityVulnerability
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SecurityRisk Risk { get; set; }
    public int LineNumber { get; set; }
}

public class SecurityRecommendation
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Fix { get; set; } = string.Empty;
}

public class PerformanceIssue
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PerformanceImpact Impact { get; set; }
    public int LineNumber { get; set; }
}

public class PerformanceOptimization
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Implementation { get; set; } = string.Empty;
    public double ExpectedImprovement { get; set; }
}

public class EstimatedMetrics
{
    public double EstimatedExecutionTimeMs { get; set; }
    public double EstimatedMemoryUsageMB { get; set; }
    public double EstimatedCpuUsage { get; set; }
}

public class MaintainabilityFactor
{
    public string Name { get; set; } = string.Empty;
    public double Score { get; set; }
    public string Impact { get; set; } = string.Empty;
}

public class RefactoringOpportunity
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public double ImpactScore { get; set; }
}

public class QualityDataPoint
{
    public DateTime Date { get; set; }
    public double Score { get; set; }
    public int IssueCount { get; set; }
}

public class TeamMemberMetrics
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public double AverageQualityScore { get; set; }
    public int ContributionCount { get; set; }
}

public class ProjectMetrics
{
    public int TotalFiles { get; set; }
    public int TotalLinesOfCode { get; set; }
    public double TestCoverage { get; set; }
    public int TechnicalDebtHours { get; set; }
}

public class ProjectIssue
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public QualityLevel Severity { get; set; }
}

public class CodeIssue
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public QualityLevel Severity { get; set; }
    public int LineNumber { get; set; }
    public int ColumnNumber { get; set; }
    public string Suggestion { get; set; } = string.Empty;
}

public enum PerformanceImpact { Low, Medium, High, Critical }