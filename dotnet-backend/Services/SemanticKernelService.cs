using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Threading.Tasks;

namespace TutorCopiloto.Services
{
    public class SemanticKernelService
    {
        private readonly Kernel _kernel;

        public SemanticKernelService(string openAiApiKey, string model = "gpt-4")
        {
            _kernel = Kernel.CreateBuilder()
                .AddOpenAIChatCompletion(model, openAiApiKey)
                .Build();
        }

        public async Task<string> RunPromptAsync(string prompt)
        {
            var result = await _kernel.InvokePromptAsync(prompt);
            return result.GetValue<string>() ?? string.Empty;
        }
    }
}
