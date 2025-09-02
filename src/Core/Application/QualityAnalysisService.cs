using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using TutorCopiloto.Domain.Entities;
using TutorCopiloto.Services.Dto;

namespace TutorCopiloto.Services
{
    /// <summary>
    /// Configurações para o serviço de análise de qualidade
    /// </summary>
    public class QualityAnalysisOptions
    {
        public string AnalysisServiceUrl { get; set; } = "http://localhost:8001";
        public int AnalysisTimeoutSeconds { get; set; } = 300;
        public bool UseLocalAnalysis { get; set; } = true;
        public string TempDirectory { get; set; } = "/tmp/repo-analysis";
        public List<string> EnabledTools { get; set; } = new() { "lintian", "bandit", "eslint", "cloc" };
        public bool EnableSecurityAnalysis { get; set; } = true;
        public bool EnableCodeMetrics { get; set; } = true;
    }

    /// <summary>
    /// Serviço para análise de qualidade de repositórios
    /// </summary>
    public class QualityAnalysisService
    {
        private readonly HttpClient _httpClient;
        private readonly QualityAnalysisOptions _options;
        private readonly ILogger<QualityAnalysisService> _logger;

        public QualityAnalysisService(
            HttpClient httpClient,
            IOptions<QualityAnalysisOptions> options,
            ILogger<QualityAnalysisService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;

            _httpClient.BaseAddress = new Uri(_options.AnalysisServiceUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(_options.AnalysisTimeoutSeconds);
        }

        /// <summary>
        /// Executa análise completa de qualidade de um repositório
        /// </summary>
        public async Task<AnalysisReport> AnalyzeRepositoryAsync(
            Repository repository,
            string repositoryPath)
        {
            var analysisReport = new AnalysisReport
            {
                RepositoryId = repository.Id,
                Status = AnalysisStatus.InProgress,
                AnalysisDate = DateTime.UtcNow
            };

            try
            {
                _logger.LogInformation("Iniciando análise do repositório: {Name}", repository.Name);

                // Análise usando microserviço Python (recomendado)
                if (!_options.UseLocalAnalysis)
                {
                    analysisReport = await AnalyzeViaMicroserviceAsync(repository, repositoryPath);
                }
                else
                {
                    // Análise local usando ferramentas do sistema
                    analysisReport = await AnalyzeLocallyAsync(repository, repositoryPath);
                }

                analysisReport.Status = AnalysisStatus.Completed;
                analysisReport.LastAnalyzedAt = DateTime.UtcNow;

                _logger.LogInformation("Análise concluída para {Name}. Score: {Score}",
                    repository.Name, analysisReport.QualityScore);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na análise do repositório {Name}", repository.Name);
                analysisReport.Status = AnalysisStatus.Failed;
                analysisReport.ErrorMessage = ex.Message;
            }

            return analysisReport;
        }

        /// <summary>
        /// Análise via microserviço Python
        /// </summary>
        private async Task<AnalysisReport> AnalyzeViaMicroserviceAsync(
            Repository repository,
            string repositoryPath)
        {
            var analysisReport = new AnalysisReport
            {
                RepositoryId = repository.Id,
                AnalysisDate = DateTime.UtcNow
            };

            try
            {
                var analysisRequest = new
                {
                    repository_url = repository.Url,
                    repository_path = repositoryPath,
                    language = repository.Language,
                    enabled_tools = _options.EnabledTools
                };

                var jsonContent = JsonSerializer.Serialize(analysisRequest);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation("Enviando análise para microserviço: {Url}", repository.Url);

                var response = await _httpClient.PostAsync("/api/analyze", content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var analysisResult = JsonSerializer.Deserialize<AnalysisResultDto>(jsonResponse);

                    if (analysisResult != null)
                    {
                        analysisReport = MapAnalysisResultToReport(analysisResult, repository.Id);
                    }
                }
                else
                {
                    _logger.LogWarning("Microserviço retornou erro: {StatusCode}", response.StatusCode);
                    // Fallback para análise local
                    return await AnalyzeLocallyAsync(repository, repositoryPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro no microserviço, fazendo fallback para análise local");
                return await AnalyzeLocallyAsync(repository, repositoryPath);
            }

            return analysisReport;
        }

        /// <summary>
        /// Análise local usando ferramentas do sistema
        /// </summary>
        private async Task<AnalysisReport> AnalyzeLocallyAsync(
            Repository repository,
            string repositoryPath)
        {
            var analysisReport = new AnalysisReport
            {
                RepositoryId = repository.Id,
                AnalysisDate = DateTime.UtcNow
            };

            try
            {
                // Análise de métricas de código
                if (_options.EnableCodeMetrics)
                {
                    var codeMetrics = await AnalyzeCodeMetricsAsync(repositoryPath);
                    analysisReport.CodeMetrics = codeMetrics;
                    analysisReport.TotalLinesOfCode = codeMetrics.Sum(m => m.Lines);
                    analysisReport.FilesCount = codeMetrics.Sum(m => m.Files);
                }

                // Verificar presença de arquivos importantes
                analysisReport.HasReadme = File.Exists(Path.Combine(repositoryPath, "README.md")) ||
                                          File.Exists(Path.Combine(repositoryPath, "readme.md"));
                analysisReport.HasTests = Directory.Exists(Path.Combine(repositoryPath, "tests")) ||
                                         Directory.Exists(Path.Combine(repositoryPath, "__tests__"));
                analysisReport.HasCI = File.Exists(Path.Combine(repositoryPath, ".github", "workflows", "ci.yml")) ||
                                      File.Exists(Path.Combine(repositoryPath, ".travis.yml"));

                // Análise específica por linguagem
                switch (repository.Language?.ToLower())
                {
                    case "python":
                        await AnalyzePythonProjectAsync(repositoryPath, analysisReport);
                        break;
                    case "javascript":
                    case "typescript":
                        await AnalyzeJavaScriptProjectAsync(repositoryPath, analysisReport);
                        break;
                    case "csharp":
                        await AnalyzeCSharpProjectAsync(repositoryPath, analysisReport);
                        break;
                    default:
                        _logger.LogInformation("Análise específica não disponível para {Language}",
                            repository.Language);
                        break;
                }

                // Calcular score de qualidade
                analysisReport.QualityScore = CalculateQualityScore(analysisReport);

                _logger.LogInformation("Análise local concluída. Score: {Score}", analysisReport.QualityScore);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na análise local");
                throw;
            }

            return analysisReport;
        }

        /// <summary>
        /// Análise de métricas de código usando cloc ou scc
        /// </summary>
        private async Task<List<CodeMetric>> AnalyzeCodeMetricsAsync(string repositoryPath)
        {
            var metrics = new List<CodeMetric>();

            try
            {
                // Tentar usar cloc primeiro, depois scc como fallback
                var clocOutput = await RunToolAsync("cloc", $"--json {repositoryPath}");

                if (!string.IsNullOrEmpty(clocOutput))
                {
                    var clocResult = JsonSerializer.Deserialize<Dictionary<string, ClocLanguageResult>>(clocOutput);

                    if (clocResult != null)
                    {
                        foreach (var (language, result) in clocResult)
                        {
                            if (language != "header" && language != "SUM")
                            {
                                metrics.Add(new CodeMetric
                                {
                                    Language = language,
                                    Files = result.NFiles,
                                    Lines = result.Code + result.Comment + result.Blank,
                                    CodeLines = result.Code,
                                    CommentLines = result.Comment,
                                    BlankLines = result.Blank
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao analisar métricas de código");
            }

            return metrics;
        }

        /// <summary>
        /// Análise específica para projetos Python
        /// </summary>
        private async Task AnalyzePythonProjectAsync(string repositoryPath, AnalysisReport report)
        {
            try
            {
                // Verificar se tem setup.py ou pyproject.toml
                report.HasDebianPackaging = File.Exists(Path.Combine(repositoryPath, "setup.py")) ||
                                           File.Exists(Path.Combine(repositoryPath, "pyproject.toml"));

                // Análise de segurança com bandit (se disponível)
                if (_options.EnableSecurityAnalysis && _options.EnabledTools.Contains("bandit"))
                {
                    var banditOutput = await RunToolAsync("bandit", $"-r {repositoryPath} -f json");

                    if (!string.IsNullOrEmpty(banditOutput))
                    {
                        var banditResult = JsonSerializer.Deserialize<BanditResult>(banditOutput);

                        if (banditResult != null)
                        {
                            report.SecurityIssues = banditResult.Results?.Count ?? 0;
                            report.CriticalSecurityIssues = banditResult.Results?
                                .Count(r => r.IssueSeverity == "HIGH" || r.IssueSeverity == "CRITICAL") ?? 0;

                            // Adicionar bugs de segurança
                            foreach (var result in banditResult.Results ?? new List<BanditIssueResult>())
                            {
                                report.BugReports.Add(new BugReport
                                {
                                    Title = result.IssueText,
                                    Description = $"Security issue: {result.IssueText}",
                                    Severity = result.IssueSeverity,
                                    Category = "security",
                                    FilePath = result.Filename,
                                    LineNumber = result.LineNumber
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro na análise Python");
            }
        }

        /// <summary>
        /// Análise específica para projetos JavaScript/TypeScript
        /// </summary>
        private async Task AnalyzeJavaScriptProjectAsync(string repositoryPath, AnalysisReport report)
        {
            try
            {
                // Verificar se tem package.json
                var packageJsonPath = Path.Combine(repositoryPath, "package.json");
                report.HasDebianPackaging = File.Exists(packageJsonPath);

                // Análise com ESLint (se disponível)
                if (_options.EnabledTools.Contains("eslint"))
                {
                    var eslintOutput = await RunToolAsync("eslint", $"{repositoryPath} --format json");

                    if (!string.IsNullOrEmpty(eslintOutput))
                    {
                        var eslintResults = JsonSerializer.Deserialize<List<EslintResult>>(eslintOutput);

                        if (eslintResults != null)
                        {
                            foreach (var result in eslintResults)
                            {
                                foreach (var message in result.Messages)
                                {
                                    report.BugReports.Add(new BugReport
                                    {
                                        Title = message.RuleId ?? "ESLint Issue",
                                        Description = message.Message,
                                        Severity = message.Severity == 2 ? "high" : "medium",
                                        Category = "code-quality",
                                        FilePath = result.FilePath,
                                        LineNumber = message.Line
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro na análise JavaScript");
            }
        }

        /// <summary>
        /// Análise específica para projetos C#
        /// </summary>
        private async Task AnalyzeCSharpProjectAsync(string repositoryPath, AnalysisReport report)
        {
            try
            {
                // Verificar se tem arquivos .csproj
                var csprojFiles = Directory.GetFiles(repositoryPath, "*.csproj", SearchOption.AllDirectories);
                report.HasDebianPackaging = csprojFiles.Length > 0;

                // Verificar se tem testes
                var testFiles = Directory.GetFiles(repositoryPath, "*Test*.cs", SearchOption.AllDirectories);
                report.HasTests = testFiles.Length > 0;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro na análise C#");
            }
        }

        /// <summary>
        /// Calcula o score de qualidade baseado nas métricas
        /// </summary>
        private decimal CalculateQualityScore(AnalysisReport report)
        {
            decimal score = 50; // Score base

            // Bônus por boas práticas
            if (report.HasReadme) score += 10;
            if (report.HasTests) score += 15;
            if (report.HasCI) score += 10;
            if (report.HasDocumentation) score += 5;
            if (report.HasDebianPackaging) score += 10;

            // Penalizações por problemas
            score -= Math.Min(report.LintianErrors * 5, 20);
            score -= Math.Min(report.SecurityIssues * 3, 15);
            score -= Math.Min(report.BugReports.Count * 2, 10);

            // Bônus por cobertura de testes
            if (report.TestCoverage > 0)
            {
                score += Math.Min((decimal)report.TestCoverage / 10, 10);
            }

            return Math.Max(0, Math.Min(100, score));
        }

        /// <summary>
        /// Executa uma ferramenta externa e retorna a saída
        /// </summary>
        private async Task<string> RunToolAsync(string toolName, string arguments)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = toolName,
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                if (process.ExitCode == 0)
                {
                    return output;
                }
                else
                {
                    _logger.LogWarning("Ferramenta {Tool} falhou: {Error}", toolName, error);
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao executar ferramenta {Tool}", toolName);
                return string.Empty;
            }
        }

        /// <summary>
        /// Mapeia resultado da análise para relatório
        /// </summary>
        private AnalysisReport MapAnalysisResultToReport(AnalysisResultDto result, int repositoryId)
        {
            return new AnalysisReport
            {
                RepositoryId = repositoryId,
                AnalysisDate = DateTime.UtcNow,
                TotalLinesOfCode = result.TotalLinesOfCode,
                FilesCount = result.FilesCount,
                QualityScore = result.QualityScore,
                HasDebianPackaging = result.HasDebianPackaging,
                LintianErrors = result.LintianErrors,
                LintianWarnings = result.LintianWarnings,
                LintianInfo = result.LintianInfo,
                SecurityIssues = result.SecurityIssues,
                CriticalSecurityIssues = result.CriticalSecurityIssues,
                HasTests = result.HasTests,
                HasCI = result.HasCI,
                TestCoverage = result.TestCoverage,
                HasReadme = result.HasReadme,
                HasDocumentation = result.HasDocumentation,
                LintianFindings = result.LintianFindings.Select(f => new LintianFinding
                {
                    Severity = f.Severity,
                    Tag = f.Tag,
                    Description = f.Description,
                    FilePath = f.FilePath,
                    LineNumber = f.LineNumber
                }).ToList(),
                BugReports = result.BugReports.Select(b => new BugReport
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
                CodeMetrics = result.CodeMetrics.Select(m => new CodeMetric
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
        }
    }

    // DTOs para resultados de análise
    public class AnalysisResultDto
    {
        public int TotalLinesOfCode { get; set; }
        public int FilesCount { get; set; }
        public decimal QualityScore { get; set; }
        public bool HasDebianPackaging { get; set; }
        public int LintianErrors { get; set; }
        public int LintianWarnings { get; set; }
        public int LintianInfo { get; set; }
        public int SecurityIssues { get; set; }
        public int CriticalSecurityIssues { get; set; }
        public bool HasTests { get; set; }
        public bool HasCI { get; set; }
        public decimal TestCoverage { get; set; }
        public bool HasReadme { get; set; }
        public bool HasDocumentation { get; set; }
        public List<LintianFindingDto> LintianFindings { get; set; } = new();
        public List<BugReportDto> BugReports { get; set; } = new();
        public List<CodeMetricDto> CodeMetrics { get; set; } = new();
    }

    // DTOs para ferramentas específicas
    public class ClocLanguageResult
    {
        public int Files { get; set; }
        public int Code { get; set; }
        public int Comment { get; set; }
        public int Blank { get; set; }
    }

    public class BanditResult
    {
        public List<BanditIssueResult> Results { get; set; } = new();
    }

    public class BanditIssueResult
    {
        public string IssueText { get; set; } = string.Empty;
        public string IssueSeverity { get; set; } = string.Empty;
        public string Filename { get; set; } = string.Empty;
        public int LineNumber { get; set; }
    }

    public class EslintResult
    {
        public string FilePath { get; set; } = string.Empty;
        public List<EslintMessage> Messages { get; set; } = new();
    }

    public class EslintMessage
    {
        public string RuleId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int Line { get; set; }
        public int Severity { get; set; }
    }
}
