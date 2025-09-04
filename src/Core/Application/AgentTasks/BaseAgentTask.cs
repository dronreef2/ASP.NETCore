using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace TutorCopiloto.Services.AgentTasks
{
    /// <summary>
    /// Classe base para implementação de tarefas de agente
    /// </summary>
    public abstract class BaseAgentTask : IAgentTask
    {
        protected readonly ILogger Logger;

        protected BaseAgentTask(ILogger logger)
        {
            Logger = logger;
        }

        public abstract string AgentName { get; }
        public abstract string Description { get; }
        public virtual int Priority => 10;

        public virtual async Task<bool> CanExecuteAsync(string repositoryPath, RepositoryAnalysisContext context)
        {
            try
            {
                if (string.IsNullOrEmpty(repositoryPath) || !Directory.Exists(repositoryPath))
                    return false;

                return await CanExecuteInternalAsync(repositoryPath, context);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Erro ao verificar se agente {AgentName} pode executar", AgentName);
                return false;
            }
        }

        public virtual async Task<AgentTaskResult> ExecuteAsync(string repositoryPath, RepositoryAnalysisContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new AgentTaskResult
            {
                AgentName = AgentName,
                ExecutedAt = DateTime.UtcNow
            };

            try
            {
                Logger.LogInformation("Iniciando execução do agente {AgentName} para {Repository}", 
                    AgentName, context.RepositoryName);

                // Verifica se pode executar
                if (!await CanExecuteAsync(repositoryPath, context))
                {
                    result.Status = "Skipped";
                    result.Summary = "Agente não pode ser executado para este repositório";
                    return result;
                }

                // Executa a lógica específica do agente
                result = await ExecuteInternalAsync(repositoryPath, context);
                result.AgentName = AgentName;
                result.ExecutedAt = DateTime.UtcNow;

                if (string.IsNullOrEmpty(result.Status))
                    result.Status = "Success";

                Logger.LogInformation("Agente {AgentName} executado com sucesso. Encontrados {FindingsCount} achados", 
                    AgentName, result.Findings.Count);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Erro durante execução do agente {AgentName}", AgentName);
                result.Status = "Failed";
                result.ErrorMessage = ex.Message;
                result.Summary = $"Falha na execução: {ex.Message}";
            }
            finally
            {
                stopwatch.Stop();
                result.ExecutionTime = stopwatch.Elapsed;
            }

            return result;
        }

        public virtual TimeSpan EstimateExecutionTime()
        {
            return TimeSpan.FromMinutes(5); // Padrão de 5 minutos
        }

        /// <summary>
        /// Verifica se o agente pode executar (implementação específica)
        /// </summary>
        protected abstract Task<bool> CanExecuteInternalAsync(string repositoryPath, RepositoryAnalysisContext context);

        /// <summary>
        /// Executa a lógica específica do agente
        /// </summary>
        protected abstract Task<AgentTaskResult> ExecuteInternalAsync(string repositoryPath, RepositoryAnalysisContext context);

        /// <summary>
        /// Utilitário para buscar arquivos por padrão
        /// </summary>
        protected List<string> FindFiles(string repositoryPath, string pattern, bool recursive = true)
        {
            try
            {
                var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                return Directory.GetFiles(repositoryPath, pattern, searchOption).ToList();
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Erro ao buscar arquivos com padrão {Pattern} em {Path}", pattern, repositoryPath);
                return new List<string>();
            }
        }

        /// <summary>
        /// Utilitário para verificar se arquivo contém texto
        /// </summary>
        protected async Task<bool> FileContainsAsync(string filePath, string text, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            try
            {
                var content = await File.ReadAllTextAsync(filePath);
                return content.Contains(text, comparison);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Erro ao ler arquivo {FilePath}", filePath);
                return false;
            }
        }

        /// <summary>
        /// Utilitário para contar linhas de código em arquivo
        /// </summary>
        protected async Task<int> CountLinesAsync(string filePath)
        {
            try
            {
                var lines = await File.ReadAllLinesAsync(filePath);
                return lines.Where(line => !string.IsNullOrWhiteSpace(line) && !line.Trim().StartsWith("//")).Count();
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Erro ao contar linhas em {FilePath}", filePath);
                return 0;
            }
        }

        /// <summary>
        /// Utilitário para obter caminho relativo
        /// </summary>
        protected string GetRelativePath(string repositoryPath, string filePath)
        {
            try
            {
                return Path.GetRelativePath(repositoryPath, filePath);
            }
            catch
            {
                return filePath;
            }
        }

        /// <summary>
        /// Cria um finding padrão
        /// </summary>
        protected AgentFinding CreateFinding(string type, string severity, string title, string description, 
            string filePath = "", int lineNumber = 0, string rule = "", bool canAutoFix = false)
        {
            return new AgentFinding
            {
                Type = type,
                Severity = severity,
                Title = title,
                Description = description,
                FilePath = filePath,
                LineNumber = lineNumber,
                Rule = rule,
                CanAutoFix = canAutoFix
            };
        }

        /// <summary>
        /// Cria uma recomendação padrão
        /// </summary>
        protected AgentRecommendation CreateRecommendation(string type, string priority, string title, 
            string description, List<string>? actionItems = null, string estimatedEffort = "Medium")
        {
            return new AgentRecommendation
            {
                Type = type,
                Priority = priority,
                Title = title,
                Description = description,
                ActionItems = actionItems ?? new List<string>(),
                EstimatedEffort = estimatedEffort
            };
        }
    }
}