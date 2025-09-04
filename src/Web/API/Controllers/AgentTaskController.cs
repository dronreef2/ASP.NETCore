using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TutorCopiloto.Services.AgentTasks;
using TutorCopiloto.Services.Dto.AgentTasks;
using TutorCopiloto.Services;

namespace TutorCopiloto.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgentTaskController : ControllerBase
    {
        private readonly AgentTaskOrchestrator _orchestrator;
        private readonly IRepositoryAnalysisService _repositoryAnalysisService;
        private readonly ILogger<AgentTaskController> _logger;

        public AgentTaskController(
            AgentTaskOrchestrator orchestrator,
            IRepositoryAnalysisService repositoryAnalysisService,
            ILogger<AgentTaskController> logger)
        {
            _orchestrator = orchestrator;
            _repositoryAnalysisService = repositoryAnalysisService;
            _logger = logger;
        }

        /// <summary>
        /// Lista agentes disponíveis para análise
        /// </summary>
        [HttpGet("agents")]
        [ProducesResponseType(typeof(AvailableAgentsResponseDto), 200)]
        public ActionResult<AvailableAgentsResponseDto> GetAvailableAgents()
        {
            try
            {
                var agents = _orchestrator.GetAvailableAgents();
                var totalTime = _orchestrator.EstimateTotalExecutionTime();

                var response = new AvailableAgentsResponseDto
                {
                    Agents = agents.Select(a => new AgentInfoDto
                    {
                        Name = a.Name,
                        Description = a.Description,
                        Priority = a.Priority,
                        EstimatedExecutionTime = a.EstimatedExecutionTime
                    }).ToList(),
                    EstimatedTotalTime = totalTime,
                    TotalAgents = agents.Count
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter agentes disponíveis");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Executa análise completa com todos os agentes aplicáveis
        /// </summary>
        [HttpPost("execute-full-analysis")]
        [ProducesResponseType(typeof(AgentTaskExecutionResponseDto), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<AgentTaskExecutionResponseDto>> ExecuteFullAnalysis(
            [FromBody] AgentTaskExecutionRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Solicitada análise completa para repositório: {Url}", request.RepositoryUrl);

                // Clona o repositório
                var repositoryAnalysis = await _repositoryAnalysisService.AnalyzeRepositoryAsync(
                    request.RepositoryUrl, request.Branch);

                // Prepara contexto de análise
                var context = CreateAnalysisContext(request, repositoryAnalysis);

                // Encontra diretório clonado
                var repositoryPath = await FindRepositoryPath(request.RepositoryUrl, request.Branch);
                if (string.IsNullOrEmpty(repositoryPath))
                {
                    return BadRequest(new { message = "Não foi possível localizar o repositório clonado" });
                }

                // Executa análise
                var executionResult = await _orchestrator.ExecuteFullAnalysisAsync(repositoryPath, context);

                // Converte para DTO
                var response = ConvertToResponseDto(executionResult, request.RepositoryUrl);

                _logger.LogInformation("Análise completa concluída para {Repository} com status {Status}", 
                    request.RepositoryUrl, executionResult.Status);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante execução de análise completa para {Repository}", request.RepositoryUrl);
                return StatusCode(500, new { message = "Erro interno do servidor", detail = ex.Message });
            }
        }

        /// <summary>
        /// Executa agentes específicos
        /// </summary>
        [HttpPost("execute-specific-agents")]
        [ProducesResponseType(typeof(AgentTaskExecutionResponseDto), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<AgentTaskExecutionResponseDto>> ExecuteSpecificAgents(
            [FromBody] SpecificAgentAnalysisRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (!request.AgentNames.Any())
                {
                    return BadRequest(new { message = "Pelo menos um agente deve ser especificado" });
                }

                _logger.LogInformation("Solicitada análise com agentes específicos para repositório: {Url}", 
                    request.RepositoryUrl);

                // Clona o repositório
                var repositoryAnalysis = await _repositoryAnalysisService.AnalyzeRepositoryAsync(
                    request.RepositoryUrl, request.Branch);

                // Prepara contexto de análise
                var context = new RepositoryAnalysisContext
                {
                    RepositoryUrl = request.RepositoryUrl,
                    Branch = request.Branch,
                    RepositoryName = ExtractRepositoryName(request.RepositoryUrl),
                    IncludeAutoFix = request.IncludeAutoFix,
                    Options = new AgentTaskExecutionOptions
                    {
                        IncludeAutoFix = request.IncludeAutoFix,
                        DryRun = request.DryRun,
                        AgentSpecificOptions = request.AgentSpecificOptions
                    }
                };

                // Encontra diretório clonado
                var repositoryPath = await FindRepositoryPath(request.RepositoryUrl, request.Branch);
                if (string.IsNullOrEmpty(repositoryPath))
                {
                    return BadRequest(new { message = "Não foi possível localizar o repositório clonado" });
                }

                // Executa agentes específicos
                var executionResult = await _orchestrator.ExecuteSpecificAgentsAsync(
                    repositoryPath, context, request.AgentNames);

                // Converte para DTO
                var response = ConvertToResponseDto(executionResult, request.RepositoryUrl);

                _logger.LogInformation("Análise com agentes específicos concluída para {Repository}", 
                    request.RepositoryUrl);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante execução de agentes específicos para {Repository}", 
                    request.RepositoryUrl);
                return StatusCode(500, new { message = "Erro interno do servidor", detail = ex.Message });
            }
        }

        /// <summary>
        /// Estima tempo de execução para conjunto de agentes
        /// </summary>
        [HttpPost("estimate-execution-time")]
        [ProducesResponseType(typeof(object), 200)]
        public ActionResult EstimateExecutionTime([FromBody] List<string> agentNames)
        {
            try
            {
                var estimatedTime = agentNames.Any() 
                    ? _orchestrator.EstimateTotalExecutionTime(agentNames)
                    : _orchestrator.EstimateTotalExecutionTime();

                return Ok(new 
                { 
                    estimatedExecutionTime = estimatedTime,
                    agentsCount = agentNames.Any() ? agentNames.Count : _orchestrator.GetAvailableAgents().Count,
                    specifiedAgents = agentNames
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao estimar tempo de execução");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        private RepositoryAnalysisContext CreateAnalysisContext(
            AgentTaskExecutionRequestDto request, 
            RepositoryAnalysis repositoryAnalysis)
        {
            return new RepositoryAnalysisContext
            {
                RepositoryUrl = request.RepositoryUrl,
                Branch = request.Branch,
                RepositoryName = repositoryAnalysis.RepositoryName ?? ExtractRepositoryName(request.RepositoryUrl),
                ProgrammingLanguages = repositoryAnalysis.DetectedLanguages ?? new List<string>(),
                IncludeAutoFix = request.IncludeAutoFix,
                Options = new AgentTaskExecutionOptions
                {
                    IncludeAutoFix = request.IncludeAutoFix,
                    DryRun = request.DryRun,
                    MaxFindingsPerAgent = request.MaxFindingsPerAgent,
                    MaxExecutionTimeMinutes = request.MaxExecutionTimeMinutes,
                    ExcludedAgents = request.ExcludedAgents,
                    IncludedAgents = request.IncludedAgents,
                    AgentSpecificOptions = request.AgentSpecificOptions
                }
            };
        }

        private async Task<string?> FindRepositoryPath(string repositoryUrl, string branch)
        {
            // Esta é uma implementação simplificada
            // Em um cenário real, você manteria um mapeamento de URLs para caminhos clonados
            var tempDir = Path.GetTempPath();
            var repoName = ExtractRepositoryName(repositoryUrl);
            var possiblePaths = new[]
            {
                Path.Combine(tempDir, "repo_analysis", repoName),
                Path.Combine(tempDir, repoName),
                Path.Combine("/tmp", "repo_analysis", repoName)
            };

            foreach (var path in possiblePaths)
            {
                if (Directory.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

        private string ExtractRepositoryName(string repositoryUrl)
        {
            try
            {
                var uri = new Uri(repositoryUrl);
                var segments = uri.Segments;
                if (segments.Length >= 2)
                {
                    var repoName = segments[^1].TrimEnd('/');
                    if (repoName.EndsWith(".git"))
                    {
                        repoName = repoName[..^4];
                    }
                    return repoName;
                }
            }
            catch
            {
                // Fallback para URLs malformadas
            }

            return "unknown-repository";
        }

        private AgentTaskExecutionResponseDto ConvertToResponseDto(
            AgentTaskExecutionResult executionResult, 
            string repositoryUrl)
        {
            return new AgentTaskExecutionResponseDto
            {
                ExecutionId = Guid.NewGuid().ToString(),
                Status = executionResult.Status,
                StartedAt = executionResult.StartedAt,
                CompletedAt = executionResult.CompletedAt != DateTime.MinValue ? executionResult.CompletedAt : null,
                TotalExecutionTime = executionResult.TotalExecutionTime,
                ErrorMessage = executionResult.ErrorMessage,
                RepositoryUrl = repositoryUrl,
                RepositoryName = executionResult.Context.RepositoryName,
                AgentResults = executionResult.AgentResults.Select(ConvertAgentResultToDto).ToList(),
                ConsolidatedReport = executionResult.ConsolidatedReport != null 
                    ? ConvertReportToDto(executionResult.ConsolidatedReport) 
                    : null
            };
        }

        private AgentTaskResultDto ConvertAgentResultToDto(AgentTaskResult result)
        {
            return new AgentTaskResultDto
            {
                AgentName = result.AgentName,
                Status = result.Status,
                Summary = result.Summary,
                ExecutionTime = result.ExecutionTime,
                ExecutedAt = result.ExecutedAt,
                ErrorMessage = result.ErrorMessage,
                Findings = result.Findings.Select(ConvertFindingToDto).ToList(),
                AppliedFixes = result.AppliedFixes.Select(ConvertFixToDto).ToList(),
                Recommendations = result.Recommendations.Select(ConvertRecommendationToDto).ToList(),
                AdditionalData = result.AdditionalData
            };
        }

        private AgentFindingDto ConvertFindingToDto(AgentFinding finding)
        {
            return new AgentFindingDto
            {
                Id = finding.Id,
                Type = finding.Type,
                Severity = finding.Severity,
                Title = finding.Title,
                Description = finding.Description,
                FilePath = finding.FilePath,
                LineNumber = finding.LineNumber,
                ColumnNumber = finding.ColumnNumber,
                Code = finding.Code,
                Rule = finding.Rule,
                CanAutoFix = finding.CanAutoFix,
                SuggestedFix = finding.SuggestedFix
            };
        }

        private AgentFixDto ConvertFixToDto(AgentFix fix)
        {
            return new AgentFixDto
            {
                Id = fix.Id,
                FindingId = fix.FindingId,
                Type = fix.Type,
                Description = fix.Description,
                FilePath = fix.FilePath,
                OriginalContent = fix.OriginalContent,
                FixedContent = fix.FixedContent,
                Applied = fix.Applied,
                AppliedAt = fix.AppliedAt
            };
        }

        private AgentRecommendationDto ConvertRecommendationToDto(AgentRecommendation recommendation)
        {
            return new AgentRecommendationDto
            {
                Id = recommendation.Id,
                Type = recommendation.Type,
                Priority = recommendation.Priority,
                Title = recommendation.Title,
                Description = recommendation.Description,
                ActionItems = recommendation.ActionItems,
                Resources = recommendation.Resources,
                EstimatedEffort = recommendation.EstimatedEffort
            };
        }

        private AgentTaskConsolidatedReportDto ConvertReportToDto(AgentTaskConsolidatedReport report)
        {
            return new AgentTaskConsolidatedReportDto
            {
                TotalAgentsExecuted = report.TotalAgentsExecuted,
                SuccessfulAgents = report.SuccessfulAgents,
                FailedAgents = report.FailedAgents,
                SkippedAgents = report.SkippedAgents,
                TotalFindings = report.TotalFindings,
                CriticalFindings = report.CriticalFindings,
                HighFindings = report.HighFindings,
                MediumFindings = report.MediumFindings,
                LowFindings = report.LowFindings,
                TotalRecommendations = report.TotalRecommendations,
                HighPriorityRecommendations = report.HighPriorityRecommendations,
                TotalExecutionTime = report.TotalExecutionTime,
                AverageExecutionTimePerAgent = report.AverageExecutionTimePerAgent,
                FindingsByType = report.FindingsByType,
                FindingsByAgent = report.FindingsByAgent,
                RecommendationsByType = report.RecommendationsByType,
                ExecutiveSummary = report.ExecutiveSummary
            };
        }
    }
}