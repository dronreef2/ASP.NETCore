using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TutorCopiloto.Services;
using TutorCopiloto.Services.Dto;

namespace TutorCopiloto.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RepositoryAnalysisController : ControllerBase
    {
        private readonly RepositoryAnalysisOrchestrator _orchestrator;
        private readonly ILogger<RepositoryAnalysisController> _logger;

        public RepositoryAnalysisController(
            RepositoryAnalysisOrchestrator orchestrator,
            ILogger<RepositoryAnalysisController> logger)
        {
            _orchestrator = orchestrator;
            _logger = logger;
        }

        /// <summary>
        /// Inicia análise de um repositório
        /// </summary>
        [HttpPost("analyze")]
        [ProducesResponseType(typeof(RepositoryAnalysisResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<RepositoryAnalysisResponse>> AnalyzeRepository(
            [FromBody] RepositoryAnalysisRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Análise solicitada para: {Url}", request.RepositoryUrl);

                var result = await _orchestrator.AnalyzeRepositoryAsync(request);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao analisar repositório: {Url}", request.RepositoryUrl);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Lista repositórios analisados com filtros
        /// </summary>
        [HttpGet("repositories")]
        [ProducesResponseType(typeof(List<RepositorySummaryDto>), 200)]
        public async Task<ActionResult<List<RepositorySummaryDto>>> GetRepositories(
            [FromQuery] RepositoryFilterDto filter)
        {
            try
            {
                var repositories = await _orchestrator.GetRepositoriesAsync(filter);
                return Ok(repositories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar repositórios");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Obtém estatísticas da plataforma
        /// </summary>
        [HttpGet("stats")]
        [ProducesResponseType(typeof(PlatformStatsDto), 200)]
        public async Task<ActionResult<PlatformStatsDto>> GetPlatformStats()
        {
            try
            {
                var stats = await _orchestrator.GetPlatformStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estatísticas da plataforma");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Obtém relatório detalhado de análise de um repositório
        /// </summary>
        [HttpGet("repositories/{repositoryId}/analysis")]
        [ProducesResponseType(typeof(AnalysisReportDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<AnalysisReportDto>> GetAnalysisReport(int repositoryId)
        {
            try
            {
                using var scope = HttpContext.RequestServices.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

                var report = await dbContext.AnalysisReports
                    .Include(ar => ar.LintianFindings)
                    .Include(ar => ar.BugReports)
                    .Include(ar => ar.CodeMetrics)
                    .Where(ar => ar.RepositoryId == repositoryId)
                    .OrderByDescending(ar => ar.AnalysisDate)
                    .FirstOrDefaultAsync();

                if (report == null)
                {
                    return NotFound(new { message = "Relatório de análise não encontrado" });
                }

                var repository = await dbContext.Repositories
                    .FirstOrDefaultAsync(r => r.Id == repositoryId);

                if (repository == null)
                {
                    return NotFound(new { message = "Repositório não encontrado" });
                }

                var reportDto = new AnalysisReportDto
                {
                    Id = report.Id,
                    RepositoryId = report.RepositoryId,
                    RepositoryName = repository.Name,
                    AnalysisDate = report.AnalysisDate,
                    TotalLinesOfCode = report.TotalLinesOfCode,
                    FilesCount = report.FilesCount,
                    QualityScore = report.QualityScore,
                    HasDebianPackaging = report.HasDebianPackaging,
                    LintianErrors = report.LintianErrors,
                    LintianWarnings = report.LintianWarnings,
                    LintianInfo = report.LintianInfo,
                    SecurityIssues = report.SecurityIssues,
                    CriticalSecurityIssues = report.CriticalSecurityIssues,
                    HasTests = report.HasTests,
                    HasCI = report.HasCI,
                    TestCoverage = report.TestCoverage,
                    HasReadme = report.HasReadme,
                    HasDocumentation = report.HasDocumentation,
                    LintianFindings = report.LintianFindings.Select(f => new LintianFindingDto
                    {
                        Severity = f.Severity,
                        Tag = f.Tag,
                        Description = f.Description,
                        FilePath = f.FilePath,
                        LineNumber = f.LineNumber
                    }).ToList(),
                    BugReports = report.BugReports.Select(b => new BugReportDto
                    {
                        Title = b.Title,
                        Description = b.Description,
                        Severity = b.Severity,
                        Category = b.Category,
                        FilePath = b.FilePath,
                        LineNumber = b.LineNumber,
                        IsFixed = b.IsFixed,
                        CreatedAt = b.CreatedAt
                    }).ToList(),
                    CodeMetrics = report.CodeMetrics.Select(m => new CodeMetricDto
                    {
                        Language = m.Language,
                        Files = m.Files,
                        Lines = m.Lines,
                        CodeLines = m.CodeLines,
                        CommentLines = m.CommentLines,
                        BlankLines = m.BlankLines,
                        Complexity = m.Complexity
                    }).ToList()
                };

                return Ok(reportDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter relatório de análise para repositório {Id}", repositoryId);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Busca repositórios no GitHub
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(GitHubSearchResponse), 200)]
        public async Task<ActionResult<GitHubSearchResponse>> SearchRepositories(
            [FromQuery] string q,
            [FromQuery] string language = null,
            [FromQuery] string sort = "stars",
            [FromQuery] int page = 1,
            [FromQuery] int per_page = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                {
                    return BadRequest(new { message = "Termo de busca é obrigatório" });
                }

                var result = await _orchestrator.SearchGitHubRepositoriesAsync(q, language, sort, page, per_page);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar repositórios no GitHub: {Query}", q);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Análise em lote de múltiplos repositórios
        /// </summary>
        [HttpPost("analyze-batch")]
        [ProducesResponseType(typeof(BatchAnalysisResponse), 200)]
        public async Task<ActionResult<BatchAnalysisResponse>> AnalyzeBatch(
            [FromBody] BatchAnalysisRequest request)
        {
            try
            {
                if (request?.RepositoryUrls == null || !request.RepositoryUrls.Any())
                {
                    return BadRequest(new { message = "Lista de URLs de repositórios é obrigatória" });
                }

                _logger.LogInformation("Análise em lote solicitada para {Count} repositórios", request.RepositoryUrls.Count);

                var result = await _orchestrator.AnalyzeBatchAsync(request);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na análise em lote");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Obtém informações de um repositório específico
        /// </summary>
        [HttpGet("repositories/{repositoryId}")]
        [ProducesResponseType(typeof(RepositorySummaryDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<RepositorySummaryDto>> GetRepository(int repositoryId)
        {
            try
            {
                using var scope = HttpContext.RequestServices.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

                var repository = await dbContext.Repositories
                    .FirstOrDefaultAsync(r => r.Id == repositoryId);

                if (repository == null)
                {
                    return NotFound(new { message = "Repositório não encontrado" });
                }

                var lastAnalysis = await dbContext.AnalysisReports
                    .Where(ar => ar.RepositoryId == repositoryId)
                    .OrderByDescending(ar => ar.AnalysisDate)
                    .Select(ar => ar.QualityScore)
                    .FirstOrDefaultAsync();

                var summary = new RepositorySummaryDto
                {
                    Id = repository.Id,
                    Name = repository.Name,
                    Owner = repository.Owner,
                    Language = repository.Language,
                    Stars = repository.Stars,
                    LastQualityScore = lastAnalysis,
                    LastAnalyzedAt = repository.LastAnalyzedAt,
                    Status = repository.LastAnalyzedAt > DateTime.MinValue ? "Analyzed" : "Not Analyzed"
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter repositório {Id}", repositoryId);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }
    }
}
