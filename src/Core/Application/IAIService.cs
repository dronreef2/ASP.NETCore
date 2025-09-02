using System.Threading.Tasks;

namespace TutorCopiloto.Services
{
    /// <summary>
    /// Interface comum para provedores de IA
    /// </summary>
    public interface IAIService
    {
        /// <summary>
        /// Nome do provedor de IA
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// Verifica se o provedor está disponível
        /// </summary>
        Task<bool> IsAvailableAsync();

        /// <summary>
        /// Envia uma mensagem e recebe uma resposta
        /// </summary>
        Task<string> GetChatResponseAsync(string message, string userId = "anonymous");

        /// <summary>
        /// Obtém uma resposta de completion
        /// </summary>
        Task<string> GetCompletionAsync(string prompt);
    }

    /// <summary>
    /// Configurações comuns para provedores de IA
    /// </summary>
    public class AIProviderOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int MaxTokens { get; set; } = 1000;
        public double Temperature { get; set; } = 0.7;
        public int TimeoutSeconds { get; set; } = 30;
        public bool Enabled { get; set; } = true;
        public int Priority { get; set; } = 1; // Ordem de prioridade (menor número = maior prioridade)
    }
}
