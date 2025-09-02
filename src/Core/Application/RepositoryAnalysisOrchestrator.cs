using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TutorCopiloto.Services;
using TutorCopiloto.Services.Dto;
using TutorCopiloto.Domain.Entities;

namespace TutorCopiloto.Services
{
    /// <summary>
    /// Configurações para o serviço de orquestração de análise
    /// </summary>
    public class RepositoryAnalysisOrchestratorOptions
    {
        public int MaxConcurrentAnalyses { get; set; } = 3;
        public int AnalysisTimeoutMinutes { get; set; } = 30;
        public bool EnableAutoCollection { get; set; } = true;
        public int CollectionIntervalHours { get; set; } = 24;
        public string TempDirectory { get; set; } = "/tmp/repo-analysis";
        public bool CleanupTempFiles { get; set; } = true;
    }

    /// <summary>
    /// Serviço de orquestração para análise de repositórios
    /// </summary>
    public class RepositoryAnalysisOrchestrator
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly RepositoryAnalysisOrchestratorOptions _options;
        private readonly ILogger<RepositoryAnalysisOrchestrator> _logger;
        private readonly SemaphoreSlim _analysisSemaphore;

        public RepositoryAnalysisOrchestrator(
            IServiceProvider serviceProvider,
            IOptions<RepositoryAnalysisOrchestratorOptions> options,
            ILogger<RepositoryAnalysisOrchestrator> logger)
        {
            _serviceProvider = serviceProvider;
            _options = options.Value;
            _logger = logger;
            _analysisSemaphore = new SemaphoreSlim(_options.MaxConcurrentAnalyses);
        }

        /// <summary>
        /// Inicia análise de um repositório específico
        /// </summary>
        public async Task<RepositoryAnalysisResponse> AnalyzeRepositoryAsync(
            RepositoryAnalysisRequest request)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

            try
            {
                // Verificar se o repositório já existe
                var existingRepository = await dbContext.Repositories
                    .FirstOrDefaultAsync(r => r.Url == request.RepositoryUrl);

                Repository repository;

                if (existingRepository != null)
                {
                    repository = existingRepository;

                    // Verificar se precisa reanalisar
                    if (!request.ForceReanalysis)
                    {
                        var lastAnalysis = await dbContext.AnalysisReports
                            .Where(ar => ar.RepositoryId == repository.Id)
                            .OrderByDescending(ar => ar.AnalysisDate)
                            .FirstOrDefaultAsync();

                        if (lastAnalysis != null &&
                            lastAnalysis.AnalysisDate > DateTime.UtcNow.AddHours(-24))
                        {
                            return new RepositoryAnalysisResponse
                            {
                                RepositoryId = repository.Id,
                                AnalysisReportId = lastAnalysis.Id,
                                RepositoryName = repository.Name,
                                RepositoryUrl = repository.Url,
                                Status = "Already Analyzed Recently",
                                AnalysisDate = lastAnalysis.AnalysisDate,
                                QualityScore = lastAnalysis.QualityScore,
                                Message = "Repositório já foi analisado recentemente"
                            };
                        }
                    }
                }
                else
                {
                    // Criar novo repositório
                    repository = await CreateRepositoryFromUrlAsync(request.RepositoryUrl, dbContext);
                    await dbContext.SaveChangesAsync();
                }

                // Iniciar análise assíncrona
                var analysisTask = Task.Run(() => PerformAnalysisAsync(repository, scope.ServiceProvider));

                return new RepositoryAnalysisResponse
                {
                    RepositoryId = repository.Id,
                    RepositoryName = repository.Name,
                    RepositoryUrl = repository.Url,
                    Status = "Analysis Started",
                    Message = "Análise iniciada em segundo plano"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao iniciar análise do repositório: {Url}", request.RepositoryUrl);
                return new RepositoryAnalysisResponse
                {
                    RepositoryUrl = request.RepositoryUrl,
                    Status = "Error",
                    Message = $"Erro ao iniciar análise: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Executa a análise completa de um repositório
        /// </summary>
        private async Task PerformAnalysisAsync(Repository repository, IServiceProvider serviceProvider)
        {
            await _analysisSemaphore.WaitAsync();

            try
            {
                var dbContext = serviceProvider.GetRequiredService<IApplicationDbContext>();
                var qualityAnalysisService = serviceProvider.GetRequiredService<QualityAnalysisService>();

                _logger.LogInformation("Iniciando análise completa do repositório: {Name}", repository.Name);

                // Clonar repositório temporariamente
                var tempPath = await CloneRepositoryAsync(repository.Url);

                if (string.IsNullOrEmpty(tempPath))
                {
                    _logger.LogError("Falha ao clonar repositório: {Url}", repository.Url);
                    return;
                }

                try
                {
                    // Executar análise de qualidade
                    var analysisReport = await qualityAnalysisService.AnalyzeRepositoryAsync(repository, tempPath);

                    // Salvar relatório no banco
                    dbContext.AnalysisReports.Add(analysisReport);
                    await dbContext.SaveChangesAsync();

                    // Atualizar timestamp do repositório
                    repository.LastAnalyzedAt = DateTime.UtcNow;
                    await dbContext.SaveChangesAsync();

                    _logger.LogInformation("Análise concluída para {Name}. Score: {Score}",
                        repository.Name, analysisReport.QualityScore);
                }
                finally
                {
                    // Limpar arquivos temporários
                    if (_options.CleanupTempFiles)
                    {
                        await CleanupTempDirectoryAsync(tempPath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante análise do repositório {Name}", repository.Name);
            }
            finally
            {
                _analysisSemaphore.Release();
            }
        }

        /// <summary>
        /// Coleta repositórios automaticamente
        /// </summary>
        public async Task CollectAndAnalyzeRepositoriesAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var collectionService = scope.ServiceProvider.GetRequiredService<RepositoryCollectionService>();

            try
            {
                _logger.LogInformation("Iniciando coleta automática de repositórios");

                // Coletar repositórios
                var repositories = await collectionService.CollectRepositoriesBatchAsync();

                foreach (var gitHubRepo in repositories)
                {
                    try
                    {
                        // Verificar se já existe
                        var existingRepo = await dbContext.Repositories
                            .FirstOrDefaultAsync(r => r.Url == gitHubRepo.HtmlUrl);

                        if (existingRepo == null)
                        {
                            // Criar novo repositório
                            var newRepo = new Repository
                            {
                                Name = gitHubRepo.Name,
                                Url = gitHubRepo.HtmlUrl,
                                Owner = gitHubRepo.Owner.Login,
                                Language = gitHubRepo.Language,
                                Description = gitHubRepo.Description,
                                Stars = gitHubRepo.StargazersCount,
                                Forks = gitHubRepo.ForksCount,
                                OpenIssues = gitHubRepo.OpenIssuesCount,
                                CreatedAt = gitHubRepo.CreatedAt,
                                UpdatedAt = gitHubRepo.UpdatedAt,
                                LastAnalyzedAt = DateTime.MinValue,
                                IsActive = !gitHubRepo.Archived && !gitHubRepo.Disabled
                            };

                            dbContext.Repositories.Add(newRepo);
                            await dbContext.SaveChangesAsync();

                            // Iniciar análise em background
                            _ = Task.Run(() => PerformAnalysisAsync(newRepo, scope.ServiceProvider));

                            _logger.LogInformation("Novo repositório adicionado: {Name}", newRepo.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Erro ao processar repositório {Name}", gitHubRepo.Name);
                    }
                }

                await dbContext.SaveChangesAsync();
                _logger.LogInformation("Coleta automática concluída");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na coleta automática de repositórios");
            }
        }

        /// <summary>
        /// Obtém estatísticas da plataforma
        /// </summary>
        public async Task<PlatformStatsDto> GetPlatformStatsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

            var stats = new PlatformStatsDto
            {
                LastUpdated = DateTime.UtcNow
            };

            try
            {
                stats.TotalRepositories = await dbContext.Repositories.CountAsync();
                stats.AnalyzedRepositories = await dbContext.Repositories
                    .CountAsync(r => r.LastAnalyzedAt > DateTime.MinValue);

                stats.PendingAnalyses = await dbContext.AnalysisReports
                    .CountAsync(ar => ar.Status == AnalysisStatus.Pending || ar.Status == AnalysisStatus.InProgress);

                var qualityScores = await dbContext.AnalysisReports
                    .Where(ar => ar.Status == AnalysisStatus.Completed)
                    .Select(ar => ar.QualityScore)
                    .ToListAsync();

                if (qualityScores.Any())
                {
                    stats.AverageQualityScore = qualityScores.Average();
                }

                // Distribuição por linguagem
                var languageStats = await dbContext.Repositories
                    .Where(r => !string.IsNullOrEmpty(r.Language))
                    .GroupBy(r => r.Language)
                    .Select(g => new { Language = g.Key, Count = g.Count() })
                    .ToListAsync();

                foreach (var stat in languageStats)
                {
                    stats.LanguageDistribution[stat.Language] = stat.Count;
                }

                // Distribuição por score de qualidade
                var scoreRanges = new[] { 0, 20, 40, 60, 80, 100 };
                foreach (var score in qualityScores)
                {
                    var range = scoreRanges.LastOrDefault(r => score >= r);
                    var rangeKey = $"{range}-{Math.Min(range + 19, 100)}";
                    if (!stats.QualityScoreDistribution.ContainsKey(rangeKey))
                    {
                        stats.QualityScoreDistribution[rangeKey] = 0;
                    }
                    stats.QualityScoreDistribution[rangeKey]++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular estatísticas da plataforma");
            }

            return stats;
        }

        /// <summary>
        /// Lista repositórios com filtros
        /// </summary>
        public async Task<List<RepositorySummaryDto>> GetRepositoriesAsync(RepositoryFilterDto filter)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

            var query = dbContext.Repositories.AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrEmpty(filter.Language))
            {
                query = query.Where(r => r.Language == filter.Language);
            }

            if (filter.MinStars.HasValue)
            {
                query = query.Where(r => r.Stars >= filter.MinStars.Value);
            }

            if (filter.MaxStars.HasValue)
            {
                query = query.Where(r => r.Stars <= filter.MaxStars.Value);
            }

            if (filter.MinQualityScore.HasValue)
            {
                query = query.Where(r => r.LastAnalyzedAt > DateTime.MinValue &&
                    dbContext.AnalysisReports
                        .Where(ar => ar.RepositoryId == r.Id)
                        .OrderByDescending(ar => ar.AnalysisDate)
                        .Select(ar => ar.QualityScore)
                        .FirstOrDefault() >= filter.MinQualityScore.Value);
            }

            // Aplicar ordenação
            var sortBy = filter.SortBy?.ToLower() ?? "stars";
            var sortOrder = filter.SortOrder?.ToLower() ?? "desc";

            query = sortBy switch
            {
                "quality" => sortOrder == "asc"
                    ? query.OrderBy(r => dbContext.AnalysisReports
                        .Where(ar => ar.RepositoryId == r.Id)
                        .OrderByDescending(ar => ar.AnalysisDate)
                        .Select(ar => ar.QualityScore)
                        .FirstOrDefault())
                    : query.OrderByDescending(r => dbContext.AnalysisReports
                        .Where(ar => ar.RepositoryId == r.Id)
                        .OrderByDescending(ar => ar.AnalysisDate)
                        .Select(ar => ar.QualityScore)
                        .FirstOrDefault()),
                "lastanalyzed" => sortOrder == "asc"
                    ? query.OrderBy(r => r.LastAnalyzedAt)
                    : query.OrderByDescending(r => r.LastAnalyzedAt),
                _ => sortOrder == "asc"
                    ? query.OrderBy(r => r.Stars)
                    : query.OrderByDescending(r => r.Stars)
            };

            // Aplicar paginação
            var repositories = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(r => new RepositorySummaryDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Owner = r.Owner,
                    Language = r.Language,
                    Stars = r.Stars,
                    LastAnalyzedAt = r.LastAnalyzedAt,
                    Status = r.LastAnalyzedAt > DateTime.MinValue ? "Analyzed" : "Not Analyzed"
                })
                .ToListAsync();

            // Adicionar scores de qualidade
            foreach (var repo in repositories)
            {
                var lastAnalysis = await dbContext.AnalysisReports
                    .Where(ar => ar.RepositoryId == repo.Id)
                    .OrderByDescending(ar => ar.AnalysisDate)
                    .Select(ar => ar.QualityScore)
                    .FirstOrDefaultAsync();

                repo.LastQualityScore = lastAnalysis;
            }

            return repositories;
        }

        /// <summary>
        /// Cria repositório a partir da URL do GitHub
        /// </summary>
        private async Task<Repository> CreateRepositoryFromUrlAsync(string url, IApplicationDbContext dbContext)
        {
            // Extrair owner e name da URL
            var uri = new Uri(url);
            var parts = uri.AbsolutePath.Trim('/').Split('/');
            var owner = parts[0];
            var name = parts[1];

            // Buscar detalhes na API do GitHub
            using var scope = _serviceProvider.CreateScope();
            var collectionService = scope.ServiceProvider.GetRequiredService<RepositoryCollectionService>();
            var details = await collectionService.GetRepositoryDetailsAsync(owner, name);

            if (details != null)
            {
                return new Repository
                {
                    Name = details.Name,
                    Url = details.HtmlUrl,
                    Owner = details.Owner.Login,
                    Language = details.Language,
                    Description = details.Description,
                    Stars = details.StargazersCount,
                    Forks = details.ForksCount,
                    OpenIssues = details.OpenIssuesCount,
                    CreatedAt = details.CreatedAt,
                    UpdatedAt = details.UpdatedAt,
                    LastAnalyzedAt = DateTime.MinValue,
                    IsActive = !details.Archived && !details.Disabled
                };
            }
            else
            {
                // Fallback se não conseguir detalhes
                return new Repository
                {
                    Name = name,
                    Url = url,
                    Owner = owner,
                    Language = "Unknown",
                    Description = "Repository details not available",
                    Stars = 0,
                    Forks = 0,
                    OpenIssues = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    LastAnalyzedAt = DateTime.MinValue,
                    IsActive = true
                };
            }
        }

        /// <summary>
        /// Clona repositório para análise temporária
        /// </summary>
        private async Task<string> CloneRepositoryAsync(string url)
        {
            var tempDir = Path.Combine(_options.TempDirectory, Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "git",
                        Arguments = $"clone --depth 1 {url} {tempDir}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                await process.WaitForExitAsync();

                if (process.ExitCode == 0)
                {
                    _logger.LogInformation("Repositório clonado: {Url} -> {Path}", url, tempDir);
                    return tempDir;
                }
                else
                {
                    _logger.LogWarning("Falha ao clonar repositório: {Url}", url);
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao clonar repositório: {Url}", url);
                return string.Empty;
            }
        }

        /// <summary>
        /// Busca repositórios no GitHub
        /// </summary>
        public async Task<Dto.GitHubSearchResponse> SearchGitHubRepositoriesAsync(
            string query, string language = null, string sort = "stars", int page = 1, int perPage = 20)
        {
            using var scope = _serviceProvider.CreateScope();
            var collectionService = scope.ServiceProvider.GetRequiredService<RepositoryCollectionService>();

            try
            {
                var result = await collectionService.SearchRepositoriesAsync(query, language, sort, page, perPage);

                return new Dto.GitHubSearchResponse
                {
                    TotalCount = result.TotalCount,
                    IncompleteResults = result.IncompleteResults,
                    Items = result.Items.Select(item => new Dto.GitHubRepositoryDto
                    {
                        Id = item.Id,
                        Name = item.Name,
                        FullName = item.FullName,
                        HtmlUrl = item.HtmlUrl,
                        Description = item.Description,
                        Language = item.Language,
                        Stars = item.StargazersCount,
                        Forks = item.ForksCount,
                        OpenIssues = item.OpenIssuesCount,
                        CreatedAt = item.CreatedAt,
                        UpdatedAt = item.UpdatedAt,
                        Owner = new Dto.GitHubOwnerDto
                        {
                            Id = item.Owner.Id,
                            Login = item.Owner.Login,
                            AvatarUrl = item.Owner.AvatarUrl,
                            HtmlUrl = item.Owner.HtmlUrl,
                            Type = item.Owner.Type
                        }
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar repositórios no GitHub: {Query}", query);
                throw;
            }
        }

        /// <summary>
        /// Análise em lote de múltiplos repositórios
        /// </summary>
        public async Task<BatchAnalysisResponse> AnalyzeBatchAsync(BatchAnalysisRequest request)
        {
            var response = new BatchAnalysisResponse
            {
                TotalRequested = request.RepositoryUrls.Count,
                StartedAt = DateTime.UtcNow,
                BatchId = Guid.NewGuid().ToString(),
                Errors = new List<string>()
            };

            var validUrls = new List<string>();
            var invalidUrls = new List<string>();

            // Validar URLs
            foreach (var url in request.RepositoryUrls)
            {
                if (IsValidGitHubUrl(url))
                {
                    validUrls.Add(url);
                }
                else
                {
                    invalidUrls.Add($"URL inválida: {url}");
                }
            }

            response.SuccessfullyQueued = validUrls.Count;
            response.FailedToQueue = invalidUrls.Count;
            response.Errors.AddRange(invalidUrls);

            if (!validUrls.Any())
            {
                response.Message = "Nenhuma URL válida encontrada para análise";
                return response;
            }

            // Iniciar análises em lote
            _ = Task.Run(async () =>
            {
                try
                {
                    var semaphore = new SemaphoreSlim(request.MaxConcurrentAnalyses);
                    var tasks = validUrls.Select(async url =>
                    {
                        await semaphore.WaitAsync();
                        try
                        {
                            var analysisRequest = new RepositoryAnalysisRequest
                            {
                                RepositoryUrl = url,
                                ForceReanalysis = request.ForceReanalysis
                            };

                            await AnalyzeRepositoryAsync(analysisRequest);
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    });

                    await Task.WhenAll(tasks);
                    _logger.LogInformation("Análise em lote concluída: {BatchId}", response.BatchId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro na análise em lote: {BatchId}", response.BatchId);
                }
            });

            response.Message = $"{response.SuccessfullyQueued} análises iniciadas em segundo plano";
            return response;
        }

        /// <summary>
        /// Valida se uma URL é um repositório GitHub válido
        /// </summary>
        private bool IsValidGitHubUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;

            try
            {
                var uri = new Uri(url);
                return uri.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase) &&
                       uri.AbsolutePath.Split('/').Length >= 3; // owner/repo
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Limpa diretório temporário
        /// </summary>
        private async Task CleanupTempDirectoryAsync(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                    _logger.LogInformation("Diretório temporário limpo: {Path}", path);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao limpar diretório temporário: {Path}", path);
            }
        }

        /// <summary>
        /// Obtém um repositório por ID
        /// </summary>
        public async Task<Repository?> GetRepositoryByIdAsync(int repositoryId)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

            return await dbContext.Repositories
                .FirstOrDefaultAsync(r => r.Id == repositoryId);
        }

        /// <summary>
        /// Obtém a última análise de um repositório
        /// </summary>
        public async Task<AnalysisReportDto?> GetLatestAnalysisAsync(int repositoryId)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

            var analysis = await dbContext.AnalysisReports
                .Where(ar => ar.RepositoryId == repositoryId)
                .OrderByDescending(ar => ar.AnalysisDate)
                .FirstOrDefaultAsync();

            if (analysis == null)
                return null;

            return new AnalysisReportDto
            {
                Id = analysis.Id,
                RepositoryId = analysis.RepositoryId,
                AnalysisDate = analysis.AnalysisDate,
                QualityScore = analysis.QualityScore,
                LintianFindings = analysis.LintianFindings?.Select(f => new LintianFindingDto
                {
                    Severity = f.Severity,
                    Tag = f.Tag,
                    Description = f.Description,
                    FilePath = f.FilePath,
                    LineNumber = f.LineNumber
                }).ToList() ?? new List<LintianFindingDto>(),
                CodeMetrics = analysis.CodeMetrics?.Select(m => new CodeMetricDto
                {
                    Language = m.Language,
                    Files = m.Files,
                    Lines = m.Lines,
                    CodeLines = m.CodeLines,
                    CommentLines = m.CommentLines,
                    BlankLines = m.BlankLines
                }).ToList() ?? new List<CodeMetricDto>()
            };
        }

        /// <summary>
        /// Obtém o histórico de análises de um repositório
        /// </summary>
        public async Task<List<AnalysisReportDto>> GetAnalysisHistoryAsync(int repositoryId)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

            var analyses = await dbContext.AnalysisReports
                .Where(ar => ar.RepositoryId == repositoryId)
                .OrderByDescending(ar => ar.AnalysisDate)
                .ToListAsync();

            return analyses.Select(analysis => new AnalysisReportDto
            {
                Id = analysis.Id,
                RepositoryId = analysis.RepositoryId,
                AnalysisDate = analysis.AnalysisDate,
                QualityScore = analysis.QualityScore,
                LintianFindings = analysis.LintianFindings?.Select(f => new LintianFindingDto
                {
                    Severity = f.Severity,
                    Tag = f.Tag,
                    Description = f.Description,
                    FilePath = f.FilePath,
                    LineNumber = f.LineNumber
                }).ToList() ?? new List<LintianFindingDto>(),
                CodeMetrics = analysis.CodeMetrics?.Select(m => new CodeMetricDto
                {
                    Language = m.Language,
                    Files = m.Files,
                    Lines = m.Lines,
                    CodeLines = m.CodeLines,
                    CommentLines = m.CommentLines,
                    BlankLines = m.BlankLines
                }).ToList() ?? new List<CodeMetricDto>()
            }).ToList();
        }
    }
}
