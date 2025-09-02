using Microsoft.AspNetCore.Mvc.RazorPages;
using TutorCopiloto.Services;
using TutorCopiloto.Services.Dto;

namespace TutorCopiloto.Pages.RepositoryAnalysis
{
    public class IndexModel : PageModel
    {
        private readonly RepositoryAnalysisOrchestrator _orchestrator;

        public IndexModel(RepositoryAnalysisOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        public PlatformStatsDto? PlatformStats { get; set; }

        public async Task OnGetAsync()
        {
            // Carregar estatísticas iniciais
            PlatformStats = await _orchestrator.GetPlatformStatsAsync();
        }
    }
}
