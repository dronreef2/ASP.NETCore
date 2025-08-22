using System.Threading;
using System.Threading.Tasks;

namespace TutorCopiloto.Services
{
    public interface IChatCompletionAdapter
    {
        /// <summary>
        /// Envia um prompt e retorna o texto da primeira resposta do modelo (ou null).
        /// </summary>
        Task<string?> GetChatResponseAsync(string prompt, CancellationToken cancellationToken = default);
    }
}
