using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TutorCopiloto.Services;
using TutorCopiloto.Services.Dto;

namespace TutorCopiloto.Pages.RepositoryAnalysis
{
    public class DetailsModel : PageModel
    {
        private readonly RepositoryAnalysisOrchestrator _orchestrator;

        public DetailsModel(RepositoryAnalysisOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        public Repository? Repository { get; set; }
        public AnalysisReportDto? LatestAnalysis { get; set; }
        public List<AnalysisReportDto> AnalysisHistory { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                Repository = await _orchestrator.GetRepositoryByIdAsync(id);
                if (Repository == null)
                {
                    return NotFound();
                }

                LatestAnalysis = await _orchestrator.GetLatestAnalysisAsync(id);
                AnalysisHistory = await _orchestrator.GetAnalysisHistoryAsync(id);

                return Page();
            }
            catch (Exception ex)
            {
                // Log error
                return RedirectToPage("./Index");
            }
        }
    }
}
