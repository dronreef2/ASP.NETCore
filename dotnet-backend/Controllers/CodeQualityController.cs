using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TutorCopiloto.Models;
using TutorCopiloto.Services;

namespace TutorCopiloto.Controllers;

/// <summary>
/// API Controller para análise avançada de qualidade de código
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class CodeQualityController : ControllerBase
{
    private readonly ICodeQualityService _codeQualityService;
    private readonly ILogger<CodeQualityController> _logger;

    public CodeQualityController(
        ICodeQualityService codeQualityService,
        ILogger<CodeQualityController> logger)
    {
        _codeQualityService = codeQualityService;
        _logger = logger;
    }

    #region Code Analysis Operations

    /// <summary>
    /// Analisa a qualidade de um bloco de código
    /// </summary>
    /// <param name="request">Dados do código para análise</param>
    /// <returns>Relatório de qualidade do código</returns>
    [HttpPost("analyze")]
    [ProducesResponseType(typeof(CodeQualityReport), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<CodeQualityReport>> AnalyzeCode([FromBody] CodeAnalysisRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(request.Code))
            {
                return BadRequest("Code is required for analysis");
            }

            var report = await _codeQualityService.AnalyzeCodeAsync(
                request.Code, 
                request.Language, 
                request.Options);

            _logger.LogInformation("Code quality analysis completed for {Language} code", request.Language);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing code quality");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Analisa a qualidade de um arquivo
    /// </summary>
    /// <param name="request">Dados do arquivo para análise</param>
    /// <returns>Relatório de qualidade do arquivo</returns>
    [HttpPost("analyze-file")]
    [ProducesResponseType(typeof(CodeQualityReport), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<CodeQualityReport>> AnalyzeFile([FromBody] FileAnalysisRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var report = await _codeQualityService.AnalyzeFileAsync(request.FilePath, request.Options);
            
            _logger.LogInformation("File quality analysis completed for {FilePath}", request.FilePath);
            return Ok(report);
        }
        catch (FileNotFoundException)
        {
            return NotFound($"File not found: {request.FilePath}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing file quality for {FilePath}", request.FilePath);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Analisa a qualidade de um projeto inteiro
    /// </summary>
    /// <param name="request">Dados do projeto para análise</param>
    /// <returns>Relatório de qualidade do projeto</returns>
    [HttpPost("analyze-project")]
    [ProducesResponseType(typeof(ProjectQualityReport), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ProjectQualityReport>> AnalyzeProject([FromBody] ProjectAnalysisRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var report = await _codeQualityService.AnalyzeProjectAsync(request.ProjectPath, request.Options);
            
            _logger.LogInformation("Project quality analysis completed for {ProjectPath}", request.ProjectPath);
            return Ok(report);
        }
        catch (DirectoryNotFoundException)
        {
            return NotFound($"Project directory not found: {request.ProjectPath}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing project quality for {ProjectPath}", request.ProjectPath);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    #endregion

    #region Metrics and Insights

    /// <summary>
    /// Calcula métricas de complexidade do código
    /// </summary>
    /// <param name="request">Código para análise de complexidade</param>
    /// <returns>Métricas de complexidade</returns>
    [HttpPost("complexity")]
    [ProducesResponseType(typeof(ComplexityMetrics), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<ComplexityMetrics>> CalculateComplexity([FromBody] CodeAnalysisRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                return BadRequest("Code is required for complexity analysis");
            }

            var metrics = await _codeQualityService.CalculateComplexityAsync(request.Code, request.Language);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating complexity for {Language} code", request.Language);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Analisa aspectos de segurança do código
    /// </summary>
    /// <param name="request">Código para análise de segurança</param>
    /// <returns>Resultado da análise de segurança</returns>
    [HttpPost("security")]
    [ProducesResponseType(typeof(CodeSecurityAnalysis), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<CodeSecurityAnalysis>> AnalyzeSecurity([FromBody] CodeAnalysisRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                return BadRequest("Code is required for security analysis");
            }

            var result = await _codeQualityService.AnalyzeSecurityAsync(request.Code, request.Language);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing security for {Language} code", request.Language);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Analisa aspectos de performance do código
    /// </summary>
    /// <param name="request">Código para análise de performance</param>
    /// <returns>Resultado da análise de performance</returns>
    [HttpPost("performance")]
    [ProducesResponseType(typeof(PerformanceAnalysisResult), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<PerformanceAnalysisResult>> AnalyzePerformance([FromBody] CodeAnalysisRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                return BadRequest("Code is required for performance analysis");
            }

            var result = await _codeQualityService.AnalyzePerformanceAsync(request.Code, request.Language);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing performance for {Language} code", request.Language);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Calcula score de manutenibilidade do código
    /// </summary>
    /// <param name="request">Código para análise de manutenibilidade</param>
    /// <returns>Score de manutenibilidade</returns>
    [HttpPost("maintainability")]
    [ProducesResponseType(typeof(MaintainabilityScore), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<MaintainabilityScore>> CalculateMaintainability([FromBody] CodeAnalysisRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                return BadRequest("Code is required for maintainability analysis");
            }

            var score = await _codeQualityService.CalculateMaintainabilityAsync(request.Code, request.Language);
            return Ok(score);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating maintainability for {Language} code", request.Language);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    #endregion

    #region Code Patterns and Best Practices

    /// <summary>
    /// Detecta padrões no código
    /// </summary>
    /// <param name="request">Código para detecção de padrões</param>
    /// <returns>Lista de padrões detectados</returns>
    [HttpPost("patterns")]
    [ProducesResponseType(typeof(List<CodePattern>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<List<CodePattern>>> DetectPatterns([FromBody] CodeAnalysisRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                return BadRequest("Code is required for pattern detection");
            }

            var patterns = await _codeQualityService.DetectPatternsAsync(request.Code, request.Language);
            return Ok(patterns);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting patterns for {Language} code", request.Language);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Verifica conformidade com melhores práticas
    /// </summary>
    /// <param name="request">Código para verificação de boas práticas</param>
    /// <returns>Lista de violações de melhores práticas</returns>
    [HttpPost("best-practices")]
    [ProducesResponseType(typeof(List<BestPracticeViolation>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<List<BestPracticeViolation>>> CheckBestPractices([FromBody] CodeAnalysisRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                return BadRequest("Code is required for best practices check");
            }

            var violations = await _codeQualityService.CheckBestPracticesAsync(request.Code, request.Language);
            return Ok(violations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking best practices for {Language} code", request.Language);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Detecta code smells no código
    /// </summary>
    /// <param name="request">Código para detecção de code smells</param>
    /// <returns>Lista de code smells detectados</returns>
    [HttpPost("code-smells")]
    [ProducesResponseType(typeof(List<CodeSmell>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<List<CodeSmell>>> DetectCodeSmells([FromBody] CodeAnalysisRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                return BadRequest("Code is required for code smell detection");
            }

            var smells = await _codeQualityService.DetectCodeSmellsAsync(request.Code, request.Language);
            return Ok(smells);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting code smells for {Language} code", request.Language);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    #endregion

    #region Improvement Suggestions

    /// <summary>
    /// Obtém sugestões de melhoria para o código
    /// </summary>
    /// <param name="request">Código para geração de sugestões</param>
    /// <returns>Lista de sugestões de melhoria</returns>
    [HttpPost("suggestions")]
    [ProducesResponseType(typeof(List<ImprovementSuggestion>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<List<ImprovementSuggestion>>> GetImprovementSuggestions([FromBody] CodeAnalysisRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                return BadRequest("Code is required for improvement suggestions");
            }

            var suggestions = await _codeQualityService.GetImprovementSuggestionsAsync(request.Code, request.Language);
            return Ok(suggestions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating improvement suggestions for {Language} code", request.Language);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém recomendações de refatoração
    /// </summary>
    /// <param name="request">Código para recomendações de refatoração</param>
    /// <returns>Recomendações de refatoração</returns>
    [HttpPost("refactoring")]
    [ProducesResponseType(typeof(RefactoringRecommendation), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<RefactoringRecommendation>> GetRefactoringRecommendations([FromBody] CodeAnalysisRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                return BadRequest("Code is required for refactoring recommendations");
            }

            var recommendations = await _codeQualityService.GetRefactoringRecommendationsAsync(request.Code, request.Language);
            return Ok(recommendations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating refactoring recommendations for {Language} code", request.Language);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    #endregion

    #region Historical Analysis

    /// <summary>
    /// Obtém tendência de qualidade ao longo do tempo
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <param name="projectId">ID do projeto</param>
    /// <param name="fromDate">Data inicial</param>
    /// <param name="toDate">Data final</param>
    /// <returns>Tendência de qualidade</returns>
    [HttpGet("trend/{userId}/{projectId}")]
    [ProducesResponseType(typeof(QualityTrend), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<QualityTrend>> GetQualityTrend(
        string userId, 
        string projectId, 
        [FromQuery] DateTime? fromDate = null, 
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
            var to = toDate ?? DateTime.UtcNow;

            if (from > to)
            {
                return BadRequest("From date cannot be greater than to date");
            }

            var trend = await _codeQualityService.GetQualityTrendAsync(userId, projectId, from, to);
            return Ok(trend);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving quality trend for user {UserId} project {ProjectId}", userId, projectId);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém insights de qualidade para um usuário
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <param name="maxResults">Número máximo de insights</param>
    /// <returns>Lista de insights de qualidade</returns>
    [HttpGet("insights/{userId}")]
    [ProducesResponseType(typeof(List<QualityInsight>), 200)]
    public async Task<ActionResult<List<QualityInsight>>> GetQualityInsights(string userId, [FromQuery] int maxResults = 10)
    {
        try
        {
            var insights = await _codeQualityService.GetQualityInsightsAsync(userId, maxResults);
            return Ok(insights);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving quality insights for user {UserId}", userId);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    #endregion

    #region Team Analytics

    /// <summary>
    /// Obtém métricas de qualidade da equipe
    /// </summary>
    /// <param name="teamId">ID da equipe</param>
    /// <returns>Métricas de qualidade da equipe</returns>
    [HttpGet("team/{teamId}/metrics")]
    [ProducesResponseType(typeof(TeamQualityMetrics), 200)]
    public async Task<ActionResult<TeamQualityMetrics>> GetTeamQualityMetrics(string teamId)
    {
        try
        {
            var metrics = await _codeQualityService.GetTeamQualityMetricsAsync(teamId);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving team quality metrics for team {TeamId}", teamId);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém leaderboard de qualidade da equipe
    /// </summary>
    /// <param name="teamId">ID da equipe</param>
    /// <returns>Leaderboard de qualidade</returns>
    [HttpGet("team/{teamId}/leaderboard")]
    [ProducesResponseType(typeof(List<QualityLeaderboard>), 200)]
    public async Task<ActionResult<List<QualityLeaderboard>>> GetQualityLeaderboard(string teamId)
    {
        try
        {
            var leaderboard = await _codeQualityService.GetQualityLeaderboardAsync(teamId);
            return Ok(leaderboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving quality leaderboard for team {TeamId}", teamId);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    #endregion
}

/// <summary>
/// Request model para análise de código
/// </summary>
public class CodeAnalysisRequest
{
    public string Code { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public CodeAnalysisOptions? Options { get; set; }
}

/// <summary>
/// Request model para análise de arquivo
/// </summary>
public class FileAnalysisRequest
{
    public string FilePath { get; set; } = string.Empty;
    public CodeAnalysisOptions? Options { get; set; }
}

/// <summary>
/// Request model para análise de projeto
/// </summary>
public class ProjectAnalysisRequest
{
    public string ProjectPath { get; set; } = string.Empty;
    public CodeAnalysisOptions? Options { get; set; }
}