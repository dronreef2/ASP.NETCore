using System.Threading.Tasks;

namespace TutorCopiloto.Services
{
    public class MockAIService : IAIService
    {
        public string ProviderName => "Mock";

        public Task<bool> IsAvailableAsync()
        {
            return Task.FromResult(true);
        }

        public Task<string> GetChatResponseAsync(string message, string userId = "anonymous")
        {
            return Task.FromResult("This is a mock response to your message: " + message);
        }

        public Task<string> GetCompletionAsync(string prompt)
        {
            return Task.FromResult("This is a mock completion for your prompt: " + prompt);
        }
    }
}
