using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace TutorCopiloto.Services
{
    /// <summary>
    /// Configurações para múltiplos provedores de IA
    /// </summary>
    public class AIServiceOptions
    {
        public LlamaIndexOptions LlamaIndex { get; set; } = new();
        public OpenAIOptions OpenAI { get; set; } = new();
        public AnthropicOptions Anthropic { get; set; } = new();
        public string FallbackMessage { get; set; } = "O serviço de IA está temporariamente indisponível. Por favor, tente novamente mais tarde.";
    }

    /// <summary>
    /// Serviço de orquestração que gerencia múltiplos provedores de IA com fallback automático
    /// </summary>
    public class AIServiceOrchestrator : IAIService
    {
        public string ProviderName => "AIServiceOrchestrator";

        private readonly IEnumerable<IAIService> _aiServices;
        private readonly AIServiceOptions _options;
        private readonly ILogger<AIServiceOrchestrator> _logger;

        public AIServiceOrchestrator(
            IEnumerable<IAIService> aiServices,
            IOptions<AIServiceOptions> options,
            ILogger<AIServiceOrchestrator> logger)
        {
            _aiServices = aiServices.OrderBy(s => GetProviderPriority(s.ProviderName));
            _options = options.Value;
            _logger = logger;
        }

        public async Task<bool> IsAvailableAsync()
        {
            foreach (var service in _aiServices)
            {
                try
                {
                    if (await service.IsAvailableAsync())
                    {
                        _logger.LogInformation("Provedor {Provider} está disponível", service.ProviderName);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Erro ao verificar disponibilidade do provedor {Provider}", service.ProviderName);
                }
            }

            _logger.LogWarning("Nenhum provedor de IA está disponível");
            return false;
        }

        public async Task<string> GetChatResponseAsync(string message, string userId = "anonymous")
        {
            foreach (var service in _aiServices)
            {
                try
                {
                    _logger.LogInformation("Tentando usar provedor {Provider} para usuário {UserId}", service.ProviderName, userId);

                    var response = await service.GetChatResponseAsync(message, userId);

                    if (!string.IsNullOrEmpty(response))
                    {
                        _logger.LogInformation("Resposta obtida com sucesso do provedor {Provider} para usuário {UserId}", service.ProviderName, userId);
                        return response;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Falha no provedor {Provider} para usuário {UserId}: {Message}", service.ProviderName, userId, ex.Message);
                    continue;
                }
            }

            _logger.LogError("Todos os provedores de IA falharam para usuário {UserId}", userId);
            return _options.FallbackMessage;
        }

        public async Task<string> GetCompletionAsync(string prompt)
        {
            return await GetChatResponseAsync(prompt);
        }

        /// <summary>
        /// Obtém a prioridade de um provedor (menor número = maior prioridade)
        /// </summary>
        private int GetProviderPriority(string providerName)
        {
            return providerName switch
            {
                "LlamaIndex" => _options.LlamaIndex.Priority,
                "OpenAI" => _options.OpenAI.Priority,
                "Anthropic" => _options.Anthropic.Priority,
                _ => 999 // Prioridade baixa para provedores desconhecidos
            };
        }

        /// <summary>
        /// Obtém informações sobre a disponibilidade de todos os provedores
        /// </summary>
        public async Task<Dictionary<string, bool>> GetProvidersStatusAsync()
        {
            var status = new Dictionary<string, bool>();

            foreach (var service in _aiServices)
            {
                try
                {
                    status[service.ProviderName] = await service.IsAvailableAsync();
                }
                catch
                {
                    status[service.ProviderName] = false;
                }
            }

            return status;
        }
    }
}
