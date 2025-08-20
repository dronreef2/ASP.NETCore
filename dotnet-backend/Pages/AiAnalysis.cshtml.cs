using Microsoft.AspNetCore.Mvc.RazorPages;
using TutorCopiloto.Services;

namespace TutorCopiloto.Pages
{
    public class AiAnalysisModel : PageModel
    {
        private readonly IIntelligentAnalysisService _intelligentAnalysis;
        private readonly IOnnxInferenceService _onnxInference;
        private readonly IDeploymentService _deploymentService;
        private readonly ILogger<AiAnalysisModel> _logger;

        public AiAnalysisModel(
            IIntelligentAnalysisService intelligentAnalysis,
            IOnnxInferenceService onnxInference,
            IDeploymentService deploymentService,
            ILogger<AiAnalysisModel> logger)
        {
            _intelligentAnalysis = intelligentAnalysis;
            _onnxInference = onnxInference;
            _deploymentService = deploymentService;
            _logger = logger;
        }

        public List<Deployment> RecentDeployments { get; set; } = new();
        public AiMetrics Metrics { get; set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                // Carregar deployments recentes
                RecentDeployments = _deploymentService.GetDeployments()
                    .OrderByDescending(d => d.CreatedAt)
                    .Take(10)
                    .ToList();

                // Calcular métricas básicas
                await CalculateMetricsAsync();

                _logger.LogInformation("Página de análise de IA carregada com {Count} deployments", RecentDeployments.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar página de análise de IA");
                // Continuar com dados vazios em caso de erro
            }
        }

        private async Task CalculateMetricsAsync()
        {
            try
            {
                var deployments = _deploymentService.GetDeployments();
                var recentDeployments = deployments.Where(d => d.CreatedAt >= DateTime.UtcNow.AddDays(-30)).ToList();

                Metrics.TotalDeployments = recentDeployments.Count;
                Metrics.SuccessfulDeployments = recentDeployments.Count(d => d.Status == DeploymentStatus.Success);
                Metrics.FailedDeployments = recentDeployments.Count(d => d.Status == DeploymentStatus.Failed);
                Metrics.SuccessRate = Metrics.TotalDeployments > 0 
                    ? (double)Metrics.SuccessfulDeployments / Metrics.TotalDeployments 
                    : 0;

                // Calcular duração média
                var deploymentsWithDuration = recentDeployments.Where(d => d.Duration.HasValue).ToList();
                if (deploymentsWithDuration.Any())
                {
                    Metrics.AverageDuration = TimeSpan.FromTicks(
                        (long)deploymentsWithDuration.Average(d => d.Duration!.Value.Ticks));
                }

                // Simular outras métricas (em produção, viria dos serviços de IA)
                Metrics.PredictionsGenerated = Random.Shared.Next(20, 100);
                Metrics.AnomaliesDetected = Random.Shared.Next(0, 10);
                Metrics.SecurityScore = Random.Shared.Next(70, 95);
                Metrics.OptimizationsSuggested = Random.Shared.Next(5, 25);

                // Gerar resumo com IA se disponível
                if (recentDeployments.Any())
                {
                    try
                    {
                        Metrics.AiSummary = await _intelligentAnalysis.GenerateDeploymentSummaryAsync(recentDeployments.FirstOrDefault()?.Id ?? "recent", "Dados de deployments recentes para análise");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Erro ao gerar resumo com IA, usando resumo padrão");
                        Metrics.AiSummary = GenerateDefaultSummary(recentDeployments);
                    }
                }
                else
                {
                    Metrics.AiSummary = "Nenhum deployment encontrado nos últimos 30 dias.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular métricas");
                // Usar valores padrão em caso de erro
                Metrics = new AiMetrics
                {
                    AiSummary = "Erro ao calcular métricas. Verifique os logs do sistema."
                };
            }
        }

        private string GenerateDefaultSummary(List<Deployment> deployments)
        {
            var successRate = deployments.Count > 0 
                ? (double)deployments.Count(d => d.Status == DeploymentStatus.Success) / deployments.Count 
                : 0;

            var summary = $"Análise dos últimos 30 dias: {deployments.Count} deployments realizados. ";

            if (successRate >= 0.9)
            {
                summary += "Excelente taxa de sucesso! Sistema operando de forma muito estável.";
            }
            else if (successRate >= 0.7)
            {
                summary += "Boa taxa de sucesso, mas há oportunidades de melhoria.";
            }
            else
            {
                summary += "Taxa de sucesso abaixo do ideal. Recomenda-se investigação dos problemas recorrentes.";
            }

            // Adicionar insights sobre tendências
            var recentFailures = deployments
                .Where(d => d.Status == DeploymentStatus.Failed && d.CreatedAt >= DateTime.UtcNow.AddDays(-7))
                .Count();

            if (recentFailures > 0)
            {
                summary += $" Foram identificadas {recentFailures} falhas na última semana.";
            }

            // Insights sobre duração
            var avgDuration = deployments
                .Where(d => d.Duration.HasValue)
                .Select(d => d.Duration!.Value.TotalMinutes)
                .DefaultIfEmpty(0)
                .Average();

            if (avgDuration > 15)
            {
                summary += " Os deployments estão levando mais tempo que o esperado.";
            }
            else if (avgDuration < 5)
            {
                summary += " Os deployments estão sendo executados rapidamente.";
            }

            return summary;
        }
    }

    public class AiMetrics
    {
        public int TotalDeployments { get; set; }
        public int SuccessfulDeployments { get; set; }
        public int FailedDeployments { get; set; }
        public double SuccessRate { get; set; }
        public TimeSpan AverageDuration { get; set; }
        public string AiSummary { get; set; } = string.Empty;

        // Métricas específicas de IA/ML
        public int PredictionsGenerated { get; set; }
        public int AnomaliesDetected { get; set; }
        public int SecurityScore { get; set; }
        public int OptimizationsSuggested { get; set; }

        // Dados para gráficos (podem ser expandidos)
        public List<DeploymentTrend> Trends { get; set; } = new();
    }

    public class DeploymentTrend
    {
        public DateTime Date { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public double AverageDuration { get; set; }
    }
}
