using Microsoft.SemanticKernel.ChatCompletion;

namespace TutorCopiloto.Services
{
    public interface IClaudeChatCompletionService : Microsoft.SemanticKernel.ChatCompletion.IChatCompletionService
    {
        // Pode adicionar métodos específicos do provider aqui no futuro
    }
}
