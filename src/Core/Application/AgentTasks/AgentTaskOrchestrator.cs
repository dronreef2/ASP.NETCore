using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using TutorCopiloto.Services.AgentTasks.Agents;

namespace TutorCopiloto.Services.AgentTasks
{
    /// <summary>
    /// Orquestrador para execução coordenada de tarefas de agente
    /// </summary>
    public class AgentTaskOrchestrator
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AgentTaskOrchestrator> _logger;
        private readonly List<IAgentTask> _availableAgents;

        public AgentTaskOrchestrator(IServiceProvider serviceProvider, ILogger<AgentTaskOrchestrator> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _availableAgents = new List<IAgentTask>();
            
            InitializeAgents();
        }

        private void InitializeAgents()
        {
            try
            {
                // Registra agentes disponíveis
                _availableAgents.Add(_serviceProvider.GetRequiredService<CodeReviewAgent>());
                _availableAgents.Add(_serviceProvider.GetRequiredService<SecurityAnalysisAgent>());
                _availableAgents.Add(_serviceProvider.GetRequiredService<DocumentationAgent>());
                
                _logger.LogInformation("Inicializados {Count} agentes: {Agents}", 
                    _availableAgents.Count, 
                    string.Join(", ", _availableAgents.Select(a => a.AgentName)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inicializar agentes");
            }
        }

        /// <summary>
        /// Executa análise completa com todos os agentes aplicáveis
        /// </summary>
        public async Task<AgentTaskExecutionResult> ExecuteFullAnalysisAsync(string repositoryPath, RepositoryAnalysisContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var executionResult = new AgentTaskExecutionResult
            {
                StartedAt = DateTime.UtcNow,
                RepositoryPath = repositoryPath,
                Context = context
            };

            try
            {
                _logger.LogInformation("Iniciando análise completa para {Repository} em {Path}", 
                    context.RepositoryName, repositoryPath);

                // Filtra agentes aplicáveis
                var applicableAgents = await GetApplicableAgentsAsync(repositoryPath, context);
                
                _logger.LogInformation("Agentes aplicáveis: {Agents}", 
                    string.Join(", ", applicableAgents.Select(a => a.AgentName)));

                // Executa agentes conforme configuração
                if (context.Options.IncludedAgents?.Any() == true)
                {
                    applicableAgents = applicableAgents.Where(a => 
                        context.Options.IncludedAgents.Contains(a.AgentName)).ToList();
                }

                if (context.Options.ExcludedAgents?.Any() == true)
                {
                    applicableAgents = applicableAgents.Where(a => 
                        !context.Options.ExcludedAgents.Contains(a.AgentName)).ToList();
                }

                // Ordena por prioridade
                applicableAgents = applicableAgents.OrderBy(a => a.Priority).ToList();

                // Executa agentes
                await ExecuteAgentsAsync(applicableAgents, repositoryPath, context, executionResult);

                // Gera relatório consolidado
                GenerateConsolidatedReport(executionResult);

                executionResult.Status = "Completed";
                _logger.LogInformation("Análise completa concluída para {Repository}. Executados {Count} agentes", 
                    context.RepositoryName, executionResult.AgentResults.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante análise completa");
                executionResult.Status = "Failed";
                executionResult.ErrorMessage = ex.Message;
            }
            finally
            {
                stopwatch.Stop();
                executionResult.TotalExecutionTime = stopwatch.Elapsed;
                executionResult.CompletedAt = DateTime.UtcNow;
            }

            return executionResult;
        }

        /// <summary>
        /// Executa agentes específicos
        /// </summary>
        public async Task<AgentTaskExecutionResult> ExecuteSpecificAgentsAsync(
            string repositoryPath, 
            RepositoryAnalysisContext context, 
            List<string> agentNames)
        {
            var stopwatch = Stopwatch.StartNew();
            var executionResult = new AgentTaskExecutionResult
            {
                StartedAt = DateTime.UtcNow,
                RepositoryPath = repositoryPath,
                Context = context
            };

            try
            {
                var selectedAgents = _availableAgents
                    .Where(a => agentNames.Contains(a.AgentName))
                    .OrderBy(a => a.Priority)
                    .ToList();

                await ExecuteAgentsAsync(selectedAgents, repositoryPath, context, executionResult);
                GenerateConsolidatedReport(executionResult);

                executionResult.Status = "Completed";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante execução de agentes específicos");
                executionResult.Status = "Failed";
                executionResult.ErrorMessage = ex.Message;
            }
            finally
            {
                stopwatch.Stop();
                executionResult.TotalExecutionTime = stopwatch.Elapsed;
                executionResult.CompletedAt = DateTime.UtcNow;
            }

            return executionResult;
        }

        private async Task<List<IAgentTask>> GetApplicableAgentsAsync(string repositoryPath, RepositoryAnalysisContext context)
        {
            var applicableAgents = new List<IAgentTask>();

            foreach (var agent in _availableAgents)
            {
                try
                {
                    var canExecute = await agent.CanExecuteAsync(repositoryPath, context);
                    if (canExecute)
                    {
                        applicableAgents.Add(agent);
                    }
                    else
                    {
                        _logger.LogDebug("Agente {AgentName} não pode ser executado para este repositório", agent.AgentName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Erro ao verificar aplicabilidade do agente {AgentName}", agent.AgentName);
                }
            }

            return applicableAgents;
        }

        private async Task ExecuteAgentsAsync(
            List<IAgentTask> agents, 
            string repositoryPath, 
            RepositoryAnalysisContext context, 
            AgentTaskExecutionResult executionResult)
        {
            var maxConcurrentAgents = Math.Max(1, Environment.ProcessorCount / 2);
            var semaphore = new SemaphoreSlim(maxConcurrentAgents);
            var tasks = new List<Task>();

            foreach (var agent in agents)
            {
                tasks.Add(ExecuteSingleAgentAsync(agent, repositoryPath, context, executionResult, semaphore));
            }

            await Task.WhenAll(tasks);
        }

        private async Task ExecuteSingleAgentAsync(
            IAgentTask agent, 
            string repositoryPath, 
            RepositoryAnalysisContext context, 
            AgentTaskExecutionResult executionResult, 
            SemaphoreSlim semaphore)
        {
            await semaphore.WaitAsync();
            
            try
            {
                var cancellationTokenSource = new CancellationTokenSource(
                    TimeSpan.FromMinutes(context.Options.MaxExecutionTimeMinutes));

                var agentTask = agent.ExecuteAsync(repositoryPath, context);
                var timeoutTask = Task.Delay(Timeout.Infinite, cancellationTokenSource.Token);

                var completedTask = await Task.WhenAny(agentTask, timeoutTask);

                AgentTaskResult result;
                if (completedTask == agentTask)
                {
                    result = await agentTask;
                }
                else
                {
                    result = new AgentTaskResult
                    {
                        AgentName = agent.AgentName,
                        Status = "Timeout",
                        ErrorMessage = "Agente excedeu tempo limite de execução",
                        ExecutionTime = TimeSpan.FromMinutes(context.Options.MaxExecutionTimeMinutes)
                    };
                }

                lock (executionResult.AgentResults)
                {
                    executionResult.AgentResults.Add(result);
                }

                _logger.LogInformation("Agente {AgentName} concluído com status {Status} em {Duration}", 
                    agent.AgentName, result.Status, result.ExecutionTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante execução do agente {AgentName}", agent.AgentName);
                
                var errorResult = new AgentTaskResult
                {
                    AgentName = agent.AgentName,
                    Status = "Failed",
                    ErrorMessage = ex.Message,
                    ExecutionTime = TimeSpan.Zero
                };

                lock (executionResult.AgentResults)
                {
                    executionResult.AgentResults.Add(errorResult);
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        private void GenerateConsolidatedReport(AgentTaskExecutionResult executionResult)
        {
            var report = new AgentTaskConsolidatedReport();

            // Estatísticas gerais
            report.TotalAgentsExecuted = executionResult.AgentResults.Count;
            report.SuccessfulAgents = executionResult.AgentResults.Count(r => r.Status == "Success");
            report.FailedAgents = executionResult.AgentResults.Count(r => r.Status == "Failed");
            report.SkippedAgents = executionResult.AgentResults.Count(r => r.Status == "Skipped");

            // Consolidação de findings
            var allFindings = executionResult.AgentResults.SelectMany(r => r.Findings).ToList();
            report.TotalFindings = allFindings.Count;
            report.CriticalFindings = allFindings.Count(f => f.Severity == "Critical");
            report.HighFindings = allFindings.Count(f => f.Severity == "High");
            report.MediumFindings = allFindings.Count(f => f.Severity == "Medium");
            report.LowFindings = allFindings.Count(f => f.Severity == "Low");

            // Findings por tipo
            report.FindingsByType = allFindings
                .GroupBy(f => f.Type)
                .ToDictionary(g => g.Key, g => g.Count());

            // Findings por agente
            report.FindingsByAgent = executionResult.AgentResults
                .ToDictionary(r => r.AgentName, r => r.Findings.Count);

            // Consolidação de recomendações
            var allRecommendations = executionResult.AgentResults.SelectMany(r => r.Recommendations).ToList();
            report.TotalRecommendations = allRecommendations.Count;
            report.HighPriorityRecommendations = allRecommendations.Count(r => r.Priority == "High");

            // Recomendações por tipo
            report.RecommendationsByType = allRecommendations
                .GroupBy(r => r.Type)
                .ToDictionary(g => g.Key, g => g.Count());

            // Tempo de execução
            report.TotalExecutionTime = executionResult.TotalExecutionTime;
            report.AverageExecutionTimePerAgent = executionResult.AgentResults.Any() 
                ? TimeSpan.FromTicks(executionResult.AgentResults.Sum(r => r.ExecutionTime.Ticks) / executionResult.AgentResults.Count)
                : TimeSpan.Zero;

            // Gera resumo executivo
            GenerateExecutiveSummary(report, executionResult);

            executionResult.ConsolidatedReport = report;
        }

        private void GenerateExecutiveSummary(AgentTaskConsolidatedReport report, AgentTaskExecutionResult executionResult)
        {
            var summary = new List<string>();

            summary.Add($"Análise executada por {report.TotalAgentsExecuted} agentes em {report.TotalExecutionTime:hh\\:mm\\:ss}");
            summary.Add($"Encontrados {report.TotalFindings} problemas ({report.CriticalFindings} críticos, {report.HighFindings} altos, {report.MediumFindings} médios, {report.LowFindings} baixos)");
            summary.Add($"Geradas {report.TotalRecommendations} recomendações ({report.HighPriorityRecommendations} de alta prioridade)");

            if (report.CriticalFindings > 0)
            {
                summary.Add("⚠️ Atenção: Foram encontrados problemas críticos que requerem ação imediata");
            }

            if (report.FailedAgents > 0)
            {
                summary.Add($"⚠️ {report.FailedAgents} agente(s) falharam durante a execução");
            }

            var topIssueTypes = report.FindingsByType
                .OrderByDescending(kvp => kvp.Value)
                .Take(3)
                .Select(kvp => $"{kvp.Key} ({kvp.Value})")
                .ToList();

            if (topIssueTypes.Any())
            {
                summary.Add($"Principais tipos de problemas: {string.Join(", ", topIssueTypes)}");
            }

            report.ExecutiveSummary = string.Join("\n", summary);
        }

        /// <summary>
        /// Lista agentes disponíveis
        /// </summary>
        public List<AgentInfo> GetAvailableAgents()
        {
            return _availableAgents.Select(a => new AgentInfo
            {
                Name = a.AgentName,
                Description = a.Description,
                Priority = a.Priority,
                EstimatedExecutionTime = a.EstimateExecutionTime()
            }).ToList();
        }

        /// <summary>
        /// Estima tempo total de execução
        /// </summary>
        public TimeSpan EstimateTotalExecutionTime(List<string>? agentNames = null)
        {
            var agents = agentNames?.Any() == true 
                ? _availableAgents.Where(a => agentNames.Contains(a.AgentName))
                : _availableAgents;

            return TimeSpan.FromTicks(agents.Sum(a => a.EstimateExecutionTime().Ticks));
        }
    }

    /// <summary>
    /// Resultado da execução de múltiplos agentes
    /// </summary>
    public class AgentTaskExecutionResult
    {
        public DateTime StartedAt { get; set; }
        public DateTime CompletedAt { get; set; }
        public TimeSpan TotalExecutionTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string RepositoryPath { get; set; } = string.Empty;
        public RepositoryAnalysisContext Context { get; set; } = new();
        public List<AgentTaskResult> AgentResults { get; set; } = new();
        public AgentTaskConsolidatedReport? ConsolidatedReport { get; set; }
    }

    /// <summary>
    /// Relatório consolidado de execução de agentes
    /// </summary>
    public class AgentTaskConsolidatedReport
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
    /// Informações sobre um agente disponível
    /// </summary>
    public class AgentInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Priority { get; set; }
        public TimeSpan EstimatedExecutionTime { get; set; }
    }
}