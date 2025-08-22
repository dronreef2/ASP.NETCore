using System.Text.RegularExpressions;
using TutorCopiloto.Models;

namespace TutorCopiloto.Services;

/// <summary>
/// Implementação do serviço de análise avançada de qualidade de código
/// </summary>
public class CodeQualityService : ICodeQualityService
{
    private readonly ILogger<CodeQualityService> _logger;
    private readonly IClaudeService? _claudeService;

    public CodeQualityService(ILogger<CodeQualityService> logger, IClaudeService? claudeService = null)
    {
        _logger = logger;
        _claudeService = claudeService;
    }

    #region Code Analysis Operations

    public async Task<CodeQualityReport> AnalyzeCodeAsync(string code, string language, CodeAnalysisOptions? options = null)
    {
        options ??= new CodeAnalysisOptions();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var report = new CodeQualityReport
            {
                Id = Guid.NewGuid().ToString(),
                Language = language,
                FileName = "inline-code",
                AnalyzedAt = DateTime.UtcNow
            };

            // Análise de complexidade
            if (options.IncludeComplexityAnalysis)
            {
                report.Complexity = await CalculateComplexityAsync(code, language);
            }

            // Análise de segurança
            if (options.IncludeSecurityAnalysis)
            {
                report.Security = await AnalyzeSecurityAsync(code, language);
            }

            // Análise de performance
            if (options.IncludePerformanceAnalysis)
            {
                report.Performance = await AnalyzePerformanceAsync(code, language);
            }

            // Análise de manutenibilidade
            if (options.IncludeMaintainabilityAnalysis)
            {
                report.Maintainability = await CalculateMaintainabilityAsync(code, language);
            }

            // Detecção de problemas gerais
            report.Issues = await DetectCodeIssuesAsync(code, language, options);

            // Sugestões de melhoria
            report.Suggestions = await GetImprovementSuggestionsAsync(code, language);

            // Cálculo do score geral
            report.OverallScore = CalculateOverallScore(report);

            stopwatch.Stop();
            report.AnalysisDuration = stopwatch.Elapsed;

            _logger.LogInformation("Code quality analysis completed in {Duration}ms for {Language} code", 
                stopwatch.ElapsedMilliseconds, language);

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing code quality for {Language}", language);
            throw;
        }
    }

    public async Task<CodeQualityReport> AnalyzeFileAsync(string filePath, CodeAnalysisOptions? options = null)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        var code = await File.ReadAllTextAsync(filePath);
        var language = DetectLanguageFromFile(filePath);
        var report = await AnalyzeCodeAsync(code, language, options);
        
        report.FileName = Path.GetFileName(filePath);
        return report;
    }

    public async Task<ProjectQualityReport> AnalyzeProjectAsync(string projectPath, CodeAnalysisOptions? options = null)
    {
        if (!Directory.Exists(projectPath))
        {
            throw new DirectoryNotFoundException($"Project directory not found: {projectPath}");
        }

        var report = new ProjectQualityReport
        {
            ProjectId = Guid.NewGuid().ToString(),
            ProjectName = Path.GetFileName(projectPath),
            AnalyzedAt = DateTime.UtcNow
        };

        var codeFiles = GetCodeFiles(projectPath);
        var fileReports = new List<CodeQualityReport>();

        foreach (var file in codeFiles)
        {
            try
            {
                var fileReport = await AnalyzeFileAsync(file, options);
                fileReports.Add(fileReport);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to analyze file {File}", file);
            }
        }

        report.FileReports = fileReports;
        report.Metrics = CalculateProjectMetrics(fileReports);
        report.OverallScore = CalculateProjectOverallScore(fileReports);
        report.ProjectIssues = AggregateProjectIssues(fileReports);

        return report;
    }

    #endregion

    #region Metrics and Insights

    public async Task<ComplexityMetrics> CalculateComplexityAsync(string code, string language)
    {
        return await Task.FromResult(language.ToLower() switch
        {
            "javascript" or "typescript" => CalculateJavaScriptComplexity(code),
            "csharp" or "c#" => CalculateCSharpComplexity(code),
            "python" => CalculatePythonComplexity(code),
            "java" => CalculateJavaComplexity(code),
            _ => CalculateGenericComplexity(code)
        });
    }

    public async Task<CodeSecurityAnalysis> AnalyzeSecurityAsync(string code, string language)
    {
        var vulnerabilities = new List<SecurityVulnerability>();
        var recommendations = new List<SecurityRecommendation>();

        // Análise de padrões de segurança comuns
        vulnerabilities.AddRange(DetectCommonSecurityVulnerabilities(code, language));
        
        // Análise específica por linguagem
        vulnerabilities.AddRange(language.ToLower() switch
        {
            "javascript" or "typescript" => DetectJavaScriptSecurityIssues(code),
            "csharp" or "c#" => DetectCSharpSecurityIssues(code),
            "python" => DetectPythonSecurityIssues(code),
            "java" => DetectJavaSecurityIssues(code),
            _ => new List<SecurityVulnerability>()
        });

        var riskLevel = CalculateSecurityRisk(vulnerabilities);
        recommendations.AddRange(GenerateSecurityRecommendations(vulnerabilities, language));

        return await Task.FromResult(new CodeSecurityAnalysis
        {
            VulnerabilityCount = vulnerabilities.Count,
            RiskLevel = riskLevel,
            Vulnerabilities = vulnerabilities,
            Recommendations = recommendations
        });
    }

    public async Task<PerformanceAnalysisResult> AnalyzePerformanceAsync(string code, string language)
    {
        var issues = new List<PerformanceIssue>();
        var optimizations = new List<PerformanceOptimization>();

        // Detecção de problemas de performance
        issues.AddRange(DetectPerformanceIssues(code, language));
        
        // Sugestões de otimização
        optimizations.AddRange(GeneratePerformanceOptimizations(issues, language));

        var rating = CalculatePerformanceRating(issues);
        var estimatedMetrics = EstimatePerformanceMetrics(code, language);

        return await Task.FromResult(new PerformanceAnalysisResult
        {
            Rating = rating,
            Issues = issues,
            Optimizations = optimizations,
            EstimatedMetrics = estimatedMetrics
        });
    }

    public async Task<MaintainabilityScore> CalculateMaintainabilityAsync(string code, string language)
    {
        var factors = new List<MaintainabilityFactor>();
        
        // Fatores de manutenibilidade
        factors.Add(new MaintainabilityFactor { Name = "Code Length", Score = CalculateCodeLengthScore(code), Impact = "Shorter methods are easier to maintain" });
        factors.Add(new MaintainabilityFactor { Name = "Naming Quality", Score = CalculateNamingQualityScore(code), Impact = "Clear names improve readability" });
        factors.Add(new MaintainabilityFactor { Name = "Comment Quality", Score = CalculateCommentQualityScore(code), Impact = "Good comments explain why, not what" });
        factors.Add(new MaintainabilityFactor { Name = "Structure", Score = CalculateStructureScore(code, language), Impact = "Well-structured code is easier to follow" });

        var overallScore = factors.Average(f => f.Score);
        var rating = GetMaintainabilityRating(overallScore);
        var improvementAreas = IdentifyImprovementAreas(factors);

        return await Task.FromResult(new MaintainabilityScore
        {
            Score = overallScore,
            Rating = rating,
            Factors = factors,
            ImprovementAreas = improvementAreas
        });
    }

    #endregion

    #region Code Patterns and Best Practices

    public async Task<List<CodePattern>> DetectPatternsAsync(string code, string language)
    {
        var patterns = new List<CodePattern>();

        // Padrões comuns
        patterns.AddRange(DetectCommonPatterns(code, language));
        
        // Padrões específicos por linguagem
        patterns.AddRange(language.ToLower() switch
        {
            "javascript" or "typescript" => DetectJavaScriptPatterns(code),
            "csharp" or "c#" => DetectCSharpPatterns(code),
            "python" => DetectPythonPatterns(code),
            "java" => DetectJavaPatterns(code),
            _ => new List<CodePattern>()
        });

        return await Task.FromResult(patterns);
    }

    public async Task<List<BestPracticeViolation>> CheckBestPracticesAsync(string code, string language)
    {
        var violations = new List<BestPracticeViolation>();

        // Práticas gerais
        violations.AddRange(CheckGeneralBestPractices(code));
        
        // Práticas específicas por linguagem
        violations.AddRange(language.ToLower() switch
        {
            "javascript" or "typescript" => CheckJavaScriptBestPractices(code),
            "csharp" or "c#" => CheckCSharpBestPractices(code),
            "python" => CheckPythonBestPractices(code),
            "java" => CheckJavaBestPractices(code),
            _ => new List<BestPracticeViolation>()
        });

        return await Task.FromResult(violations);
    }

    public async Task<List<CodeSmell>> DetectCodeSmellsAsync(string code, string language)
    {
        var smells = new List<CodeSmell>();

        // Code smells comuns
        smells.AddRange(DetectCommonCodeSmells(code));
        
        // Code smells específicos por linguagem
        smells.AddRange(language.ToLower() switch
        {
            "javascript" or "typescript" => DetectJavaScriptCodeSmells(code),
            "csharp" or "c#" => DetectCSharpCodeSmells(code),
            "python" => DetectPythonCodeSmells(code),
            "java" => DetectJavaCodeSmells(code),
            _ => new List<CodeSmell>()
        });

        return await Task.FromResult(smells);
    }

    #endregion

    #region Improvement Suggestions

    public async Task<List<ImprovementSuggestion>> GetImprovementSuggestionsAsync(string code, string language)
    {
        var suggestions = new List<ImprovementSuggestion>();

        // Análise de possíveis melhorias
        var complexity = await CalculateComplexityAsync(code, language);
        var security = await AnalyzeSecurityAsync(code, language);
        var performance = await AnalyzePerformanceAsync(code, language);
        var bestPractices = await CheckBestPracticesAsync(code, language);
        var codeSmells = await DetectCodeSmellsAsync(code, language);

        // Sugestões baseadas em complexidade
        if (complexity.CyclomaticComplexity > 10)
        {
            suggestions.Add(new ImprovementSuggestion
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Reduce Cyclomatic Complexity",
                Description = "This code has high cyclomatic complexity. Consider breaking it into smaller methods.",
                Category = ImprovementCategory.Maintainability,
                Priority = ImprovementPriority.High,
                EstimatedEffortMinutes = 30,
                Benefits = new List<string> { "Improved readability", "Easier testing", "Better maintainability" }
            });
        }

        // Sugestões baseadas em segurança
        foreach (var vulnerability in security.Vulnerabilities.Where(v => v.Risk >= SecurityRisk.Medium))
        {
            suggestions.Add(new ImprovementSuggestion
            {
                Id = Guid.NewGuid().ToString(),
                Title = $"Fix Security Issue: {vulnerability.Type}",
                Description = vulnerability.Description,
                Category = ImprovementCategory.Security,
                Priority = vulnerability.Risk >= SecurityRisk.High ? ImprovementPriority.Critical : ImprovementPriority.High,
                EstimatedEffortMinutes = 15
            });
        }

        // Sugestões baseadas em performance
        foreach (var issue in performance.Issues.Where(i => i.Impact >= PerformanceImpact.Medium))
        {
            suggestions.Add(new ImprovementSuggestion
            {
                Id = Guid.NewGuid().ToString(),
                Title = $"Performance Optimization: {issue.Type}",
                Description = issue.Description,
                Category = ImprovementCategory.Performance,
                Priority = issue.Impact >= PerformanceImpact.High ? ImprovementPriority.High : ImprovementPriority.Medium,
                EstimatedEffortMinutes = 20
            });
        }

        return await Task.FromResult(suggestions);
    }

    public async Task<RefactoringRecommendation> GetRefactoringRecommendationsAsync(string code, string language)
    {
        var opportunities = new List<RefactoringOpportunity>();

        // Análise de oportunidades de refatoração
        var codeSmells = await DetectCodeSmellsAsync(code, language);
        var complexity = await CalculateComplexityAsync(code, language);

        foreach (var smell in codeSmells.Where(s => s.Severity >= SmellSeverity.Major))
        {
            opportunities.Add(new RefactoringOpportunity
            {
                Type = smell.Name,
                Description = smell.RefactoringAdvice,
                LineNumber = smell.LineNumber,
                ImpactScore = smell.ImpactScore
            });
        }

        var priority = DetermineRefactoringPriority(opportunities, complexity);
        var estimatedEffort = EstimateRefactoringEffort(opportunities);
        var benefits = GenerateRefactoringBenefits(opportunities);

        return await Task.FromResult(new RefactoringRecommendation
        {
            Opportunities = opportunities,
            Priority = priority,
            EstimatedEffortHours = estimatedEffort,
            ExpectedBenefits = benefits
        });
    }

    #endregion

    #region Historical Analysis

    public async Task<QualityTrend> GetQualityTrendAsync(string userId, string projectId, DateTime fromDate, DateTime toDate)
    {
        // Em uma implementação real, isso consultaria o banco de dados histórico
        // Por agora, retornamos dados simulados
        var dataPoints = new List<QualityDataPoint>();
        var random = new Random();
        var currentDate = fromDate;

        while (currentDate <= toDate)
        {
            dataPoints.Add(new QualityDataPoint
            {
                Date = currentDate,
                Score = 70 + random.NextDouble() * 30, // Score entre 70-100
                IssueCount = random.Next(0, 10)
            });
            currentDate = currentDate.AddDays(1);
        }

        var direction = dataPoints.Count > 1 && dataPoints.Last().Score > dataPoints.First().Score 
            ? TrendDirection.Improving 
            : TrendDirection.Declining;

        var changePercentage = dataPoints.Count > 1 
            ? ((dataPoints.Last().Score - dataPoints.First().Score) / dataPoints.First().Score) * 100 
            : 0;

        return await Task.FromResult(new QualityTrend
        {
            DataPoints = dataPoints,
            Direction = direction,
            ChangePercentage = changePercentage,
            Summary = $"Quality has been {direction.ToString().ToLower()} by {Math.Abs(changePercentage):F1}% over the period"
        });
    }

    public async Task<List<QualityInsight>> GetQualityInsightsAsync(string userId, int maxResults = 10)
    {
        var insights = new List<QualityInsight>
        {
            new QualityInsight
            {
                Title = "Consistent Code Quality Improvement",
                Description = "Your code quality has improved by 15% over the last month",
                Type = InsightType.Achievement,
                Priority = InsightPriority.Medium,
                GeneratedAt = DateTime.UtcNow,
                ActionItems = new List<string> { "Keep up the good work!", "Consider mentoring other developers" }
            },
            new QualityInsight
            {
                Title = "Security Vulnerabilities Detected",
                Description = "Recent code submissions contain potential security issues",
                Type = InsightType.Warning,
                Priority = InsightPriority.High,
                GeneratedAt = DateTime.UtcNow,
                ActionItems = new List<string> { "Review security best practices", "Use input validation", "Enable static analysis tools" }
            }
        };

        return await Task.FromResult(insights.Take(maxResults).ToList());
    }

    #endregion

    #region Team Analytics

    public async Task<TeamQualityMetrics> GetTeamQualityMetricsAsync(string teamId)
    {
        // Implementação simulada - em produção, consultaria o banco de dados
        return await Task.FromResult(new TeamQualityMetrics
        {
            TeamId = teamId,
            AverageQualityScore = 82.5,
            TotalLinesAnalyzed = 15000,
            TotalIssuesFound = 89,
            TotalIssuesFixed = 67,
            MemberMetrics = new List<TeamMemberMetrics>
            {
                new TeamMemberMetrics { UserId = "user1", UserName = "Alice", AverageQualityScore = 85.0, ContributionCount = 45 },
                new TeamMemberMetrics { UserId = "user2", UserName = "Bob", AverageQualityScore = 80.0, ContributionCount = 38 }
            },
            TeamTrend = new QualityTrend
            {
                Direction = TrendDirection.Improving,
                ChangePercentage = 8.5,
                Summary = "Team quality has improved by 8.5% this month"
            }
        });
    }

    public async Task<List<QualityLeaderboard>> GetQualityLeaderboardAsync(string teamId)
    {
        // Implementação simulada - em produção, consultaria o banco de dados
        return await Task.FromResult(new List<QualityLeaderboard>
        {
            new QualityLeaderboard { UserId = "user1", UserName = "Alice", QualityScore = 92.5, Rank = 1, ImprovementPoints = 150, Achievements = new List<string> { "Security Champion", "Clean Code Expert" } },
            new QualityLeaderboard { UserId = "user2", UserName = "Bob", QualityScore = 88.0, Rank = 2, ImprovementPoints = 120, Achievements = new List<string> { "Performance Optimizer" } },
            new QualityLeaderboard { UserId = "user3", UserName = "Charlie", QualityScore = 85.5, Rank = 3, ImprovementPoints = 95, Achievements = new List<string> { "Consistent Contributor" } }
        });
    }

    #endregion

    #region Private Helper Methods

    private string DetectLanguageFromFile(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLower();
        return extension switch
        {
            ".js" => "javascript",
            ".ts" => "typescript",
            ".cs" => "csharp",
            ".py" => "python",
            ".java" => "java",
            ".cpp" or ".cc" or ".cxx" => "cpp",
            ".c" => "c",
            ".php" => "php",
            ".rb" => "ruby",
            ".go" => "go",
            ".rs" => "rust",
            _ => "unknown"
        };
    }

    private List<string> GetCodeFiles(string projectPath)
    {
        var extensions = new[] { ".js", ".ts", ".cs", ".py", ".java", ".cpp", ".c", ".php", ".rb", ".go", ".rs" };
        var files = new List<string>();

        foreach (var extension in extensions)
        {
            files.AddRange(Directory.GetFiles(projectPath, $"*{extension}", SearchOption.AllDirectories));
        }

        return files;
    }

    private ComplexityMetrics CalculateJavaScriptComplexity(string code)
    {
        var lines = code.Split('\n');
        var linesOfCode = lines.Count(line => !string.IsNullOrWhiteSpace(line) && !line.Trim().StartsWith("//"));
        
        // Complexidade ciclomática simplificada
        var cyclomaticComplexity = CountKeywords(code, new[] { "if", "else", "while", "for", "switch", "case", "catch" }) + 1;
        
        // Número de funções
        var functionCount = Regex.Matches(code, @"function\s+\w+|=>\s*{|=\s*function").Count;
        
        return new ComplexityMetrics
        {
            CyclomaticComplexity = cyclomaticComplexity,
            CognitiveComplexity = (int)Math.Min(cyclomaticComplexity * 1.2, 50), // Aproximação
            LinesOfCode = linesOfCode,
            NumberOfMethods = functionCount,
            NumberOfClasses = Regex.Matches(code, @"class\s+\w+").Count,
            AverageMethodComplexity = functionCount > 0 ? (double)cyclomaticComplexity / functionCount : 0
        };
    }

    private ComplexityMetrics CalculateCSharpComplexity(string code)
    {
        var lines = code.Split('\n');
        var linesOfCode = lines.Count(line => !string.IsNullOrWhiteSpace(line) && !line.Trim().StartsWith("//"));
        
        var cyclomaticComplexity = CountKeywords(code, new[] { "if", "else", "while", "for", "foreach", "switch", "case", "catch", "&&", "||" }) + 1;
        var methodCount = Regex.Matches(code, @"\b(public|private|protected|internal)\s+.*\s+\w+\s*\(").Count;
        
        return new ComplexityMetrics
        {
            CyclomaticComplexity = cyclomaticComplexity,
            CognitiveComplexity = (int)Math.Min(cyclomaticComplexity * 1.1, 50),
            LinesOfCode = linesOfCode,
            NumberOfMethods = methodCount,
            NumberOfClasses = Regex.Matches(code, @"\bclass\s+\w+").Count,
            AverageMethodComplexity = methodCount > 0 ? (double)cyclomaticComplexity / methodCount : 0
        };
    }

    private ComplexityMetrics CalculatePythonComplexity(string code)
    {
        var lines = code.Split('\n');
        var linesOfCode = lines.Count(line => !string.IsNullOrWhiteSpace(line) && !line.Trim().StartsWith("#"));
        
        var cyclomaticComplexity = CountKeywords(code, new[] { "if", "elif", "else", "while", "for", "try", "except", "and", "or" }) + 1;
        var functionCount = Regex.Matches(code, @"def\s+\w+").Count;
        
        return new ComplexityMetrics
        {
            CyclomaticComplexity = cyclomaticComplexity,
            CognitiveComplexity = (int)Math.Min(cyclomaticComplexity * 1.15, 50),
            LinesOfCode = linesOfCode,
            NumberOfMethods = functionCount,
            NumberOfClasses = Regex.Matches(code, @"class\s+\w+").Count,
            AverageMethodComplexity = functionCount > 0 ? (double)cyclomaticComplexity / functionCount : 0
        };
    }

    private ComplexityMetrics CalculateJavaComplexity(string code)
    {
        var lines = code.Split('\n');
        var linesOfCode = lines.Count(line => !string.IsNullOrWhiteSpace(line) && !line.Trim().StartsWith("//"));
        
        var cyclomaticComplexity = CountKeywords(code, new[] { "if", "else", "while", "for", "switch", "case", "catch", "&&", "||" }) + 1;
        var methodCount = Regex.Matches(code, @"\b(public|private|protected)\s+.*\s+\w+\s*\(").Count;
        
        return new ComplexityMetrics
        {
            CyclomaticComplexity = cyclomaticComplexity,
            CognitiveComplexity = (int)Math.Min(cyclomaticComplexity * 1.1, 50),
            LinesOfCode = linesOfCode,
            NumberOfMethods = methodCount,
            NumberOfClasses = Regex.Matches(code, @"\bclass\s+\w+").Count,
            AverageMethodComplexity = methodCount > 0 ? (double)cyclomaticComplexity / methodCount : 0
        };
    }

    private ComplexityMetrics CalculateGenericComplexity(string code)
    {
        var lines = code.Split('\n');
        var linesOfCode = lines.Count(line => !string.IsNullOrWhiteSpace(line));
        
        return new ComplexityMetrics
        {
            CyclomaticComplexity = Math.Max(1, linesOfCode / 10), // Aproximação simples
            CognitiveComplexity = Math.Max(1, linesOfCode / 8),
            LinesOfCode = linesOfCode,
            NumberOfMethods = 1, // Assume como uma função
            NumberOfClasses = 0,
            AverageMethodComplexity = linesOfCode / 10.0
        };
    }

    private int CountKeywords(string code, string[] keywords)
    {
        return keywords.Sum(keyword => Regex.Matches(code, $@"\b{keyword}\b").Count);
    }

    // Implementações simplificadas para os outros métodos
    private List<SecurityVulnerability> DetectCommonSecurityVulnerabilities(string code, string language) => new();
    private List<SecurityVulnerability> DetectJavaScriptSecurityIssues(string code) => new();
    private List<SecurityVulnerability> DetectCSharpSecurityIssues(string code) => new();
    private List<SecurityVulnerability> DetectPythonSecurityIssues(string code) => new();
    private List<SecurityVulnerability> DetectJavaSecurityIssues(string code) => new();
    private SecurityRisk CalculateSecurityRisk(List<SecurityVulnerability> vulnerabilities) => SecurityRisk.Low;
    private List<SecurityRecommendation> GenerateSecurityRecommendations(List<SecurityVulnerability> vulnerabilities, string language) => new();
    
    private List<PerformanceIssue> DetectPerformanceIssues(string code, string language) => new();
    private List<PerformanceOptimization> GeneratePerformanceOptimizations(List<PerformanceIssue> issues, string language) => new();
    private PerformanceRating CalculatePerformanceRating(List<PerformanceIssue> issues) => PerformanceRating.Good;
    private EstimatedMetrics EstimatePerformanceMetrics(string code, string language) => new();
    
    private double CalculateCodeLengthScore(string code) => Math.Max(0, 100 - code.Split('\n').Length / 10.0);
    private double CalculateNamingQualityScore(string code) => 85.0; // Simplificado
    private double CalculateCommentQualityScore(string code) => 80.0; // Simplificado  
    private double CalculateStructureScore(string code, string language) => 75.0; // Simplificado
    private MaintainabilityRating GetMaintainabilityRating(double score) => score switch
    {
        >= 90 => MaintainabilityRating.VeryHigh,
        >= 75 => MaintainabilityRating.High,
        >= 60 => MaintainabilityRating.Medium,
        >= 40 => MaintainabilityRating.Low,
        _ => MaintainabilityRating.VeryLow
    };
    
    private List<string> IdentifyImprovementAreas(List<MaintainabilityFactor> factors) => 
        factors.Where(f => f.Score < 70).Select(f => f.Name).ToList();

    private List<CodePattern> DetectCommonPatterns(string code, string language) => new();
    private List<CodePattern> DetectJavaScriptPatterns(string code) => new();
    private List<CodePattern> DetectCSharpPatterns(string code) => new();
    private List<CodePattern> DetectPythonPatterns(string code) => new();
    private List<CodePattern> DetectJavaPatterns(string code) => new();
    
    private List<BestPracticeViolation> CheckGeneralBestPractices(string code) => new();
    private List<BestPracticeViolation> CheckJavaScriptBestPractices(string code) => new();
    private List<BestPracticeViolation> CheckCSharpBestPractices(string code) => new();
    private List<BestPracticeViolation> CheckPythonBestPractices(string code) => new();
    private List<BestPracticeViolation> CheckJavaBestPractices(string code) => new();
    
    private List<CodeSmell> DetectCommonCodeSmells(string code) => new();
    private List<CodeSmell> DetectJavaScriptCodeSmells(string code) => new();
    private List<CodeSmell> DetectCSharpCodeSmells(string code) => new();
    private List<CodeSmell> DetectPythonCodeSmells(string code) => new();
    private List<CodeSmell> DetectJavaCodeSmells(string code) => new();
    
    private RefactoringPriority DetermineRefactoringPriority(List<RefactoringOpportunity> opportunities, ComplexityMetrics complexity) => RefactoringPriority.Recommended;
    private int EstimateRefactoringEffort(List<RefactoringOpportunity> opportunities) => opportunities.Count * 2;
    private List<string> GenerateRefactoringBenefits(List<RefactoringOpportunity> opportunities) => new List<string> { "Improved maintainability", "Better performance", "Reduced complexity" };
    
    private async Task<List<CodeIssue>> DetectCodeIssuesAsync(string code, string language, CodeAnalysisOptions options)
    {
        var issues = new List<CodeIssue>();

        // Problemas básicos
        if (code.Length > 1000)
        {
            issues.Add(new CodeIssue
            {
                Type = "Long Code Block",
                Description = "This code block is very long and might be hard to maintain",
                Severity = QualityLevel.Medium,
                LineNumber = 1,
                Suggestion = "Consider breaking this into smaller functions"
            });
        }

        return await Task.FromResult(issues);
    }

    private QualityScore CalculateOverallScore(CodeQualityReport report)
    {
        var scores = new List<double>();
        
        if (report.Complexity.CyclomaticComplexity > 0)
            scores.Add(Math.Max(0, 100 - report.Complexity.CyclomaticComplexity * 5));
        
        if (report.Security.VulnerabilityCount == 0)
            scores.Add(100);
        else
            scores.Add(Math.Max(0, 100 - report.Security.VulnerabilityCount * 10));
        
        scores.Add(report.Performance.Rating switch
        {
            PerformanceRating.Excellent => 100,
            PerformanceRating.Good => 80,
            PerformanceRating.Average => 60,
            PerformanceRating.Poor => 40,
            _ => 20
        });
        
        scores.Add(report.Maintainability.Score);

        var overall = scores.Average();
        
        return new QualityScore
        {
            Overall = overall,
            Security = scores.Count > 1 ? scores[1] : 100,
            Performance = scores.Count > 2 ? scores[2] : 80,
            Maintainability = report.Maintainability.Score,
            Complexity = scores.Count > 0 ? scores[0] : 80,
            Rating = overall switch
            {
                >= 90 => QualityRating.Excellent,
                >= 75 => QualityRating.Good,
                >= 60 => QualityRating.Fair,
                >= 40 => QualityRating.Poor,
                _ => QualityRating.Critical
            },
            Summary = $"Overall code quality is {(overall >= 75 ? "good" : "needs improvement")}"
        };
    }

    private ProjectMetrics CalculateProjectMetrics(List<CodeQualityReport> fileReports)
    {
        return new ProjectMetrics
        {
            TotalFiles = fileReports.Count,
            TotalLinesOfCode = fileReports.Sum(r => r.Complexity.LinesOfCode),
            TestCoverage = 0, // Não implementado
            TechnicalDebtHours = fileReports.Sum(r => r.Issues.Count) * 30 / 60 // 30 min por issue
        };
    }

    private QualityScore CalculateProjectOverallScore(List<CodeQualityReport> fileReports)
    {
        if (!fileReports.Any()) return new QualityScore { Overall = 0, Rating = QualityRating.Critical };
        
        var averageScore = fileReports.Average(r => r.OverallScore.Overall);
        return new QualityScore
        {
            Overall = averageScore,
            Rating = averageScore switch
            {
                >= 90 => QualityRating.Excellent,
                >= 75 => QualityRating.Good,
                >= 60 => QualityRating.Fair,
                >= 40 => QualityRating.Poor,
                _ => QualityRating.Critical
            }
        };
    }

    private List<ProjectIssue> AggregateProjectIssues(List<CodeQualityReport> fileReports)
    {
        return fileReports
            .SelectMany(r => r.Issues.Select(i => new ProjectIssue
            {
                Type = i.Type,
                Description = i.Description,
                FileName = r.FileName,
                Severity = i.Severity
            }))
            .ToList();
    }

    #endregion
}