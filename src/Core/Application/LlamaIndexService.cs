using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace TutorCopiloto.Services
{
    public class LlamaIndexOptions : AIProviderOptions
    {
        public LlamaIndexOptions()
        {
            BaseUrl = "https://api.llamaindex.ai/v1";
            Model = "gpt-3.5-turbo";
            Priority = 1; // Prioridade alta
        }
    }

    public class LlamaIndexService : IAIService
    {
        public string ProviderName => "LlamaIndex";
        private readonly HttpClient _httpClient;
        private readonly LlamaIndexOptions _options;
        private readonly ILogger<LlamaIndexService> _logger;

        public LlamaIndexService(
            HttpClient httpClient,
            IOptions<LlamaIndexOptions> options,
            ILogger<LlamaIndexService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;

            // Configurar headers padrão
            _httpClient.BaseAddress = new Uri(_options.BaseUrl);
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _options.ApiKey);
            _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
        }

        public async Task<bool> IsAvailableAsync()
        {
            if (!_options.Enabled || string.IsNullOrEmpty(_options.ApiKey))
            {
                return false;
            }

            // Lista de URLs para testar
            var urlsToTest = new[]
            {
                _options.BaseUrl,
                "https://api.llamaindex.ai/v1",
                "https://api.cloud.llamaindex.ai/v1"
            };

            foreach (var baseUrl in urlsToTest)
            {
                try
                {
                    // Atualizar a URL base do HttpClient
                    _httpClient.BaseAddress = new Uri(baseUrl);

                    // Fazer uma requisição simples de teste
                    var testRequest = new
                    {
                        model = _options.Model,
                        messages = new[]
                        {
                            new { role = "user", content = "test" }
                        },
                        max_tokens = 10,
                        temperature = _options.Temperature,
                        stream = false
                    };

                    var jsonContent = JsonSerializer.Serialize(testRequest);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PostAsync("/chat/completions", content);
                    return response.IsSuccessStatusCode;
                }
                catch
                {
                    continue;
                }
            }

            return false;
        }

        public async Task<string> GetChatResponseAsync(string message, string userId = "anonymous")
        {
            // Verificar se o serviço está habilitado
            if (!_options.Enabled)
            {
                _logger.LogInformation("LlamaIndex service está desabilitado. Retornando resposta padrão.");
                return "O serviço de IA está temporariamente indisponível. Por favor, tente novamente mais tarde.";
            }

            // Lista de URLs para tentar (fallback)
            var urlsToTry = new[]
            {
                _options.BaseUrl,
                "https://api.llamaindex.ai/v1",
                "https://api.cloud.llamaindex.ai/v1"
            };

            foreach (var baseUrl in urlsToTry)
            {
                try
                {
                    _logger.LogInformation("Tentando conectar à API do LlamaIndex: {BaseUrl}", baseUrl);

                    // Atualizar a URL base do HttpClient
                    _httpClient.BaseAddress = new Uri(baseUrl);

                    var request = new
                    {
                        model = _options.Model,
                        messages = new[]
                        {
                            new
                            {
                                role = "user",
                                content = message
                            }
                        },
                        max_tokens = _options.MaxTokens,
                        temperature = _options.Temperature,
                        stream = false
                    };

                    var jsonContent = JsonSerializer.Serialize(request);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    _logger.LogInformation("Enviando mensagem para LlamaIndex API: {UserId}", userId);

                    var response = await _httpClient.PostAsync("/chat/completions", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        var responseData = JsonSerializer.Deserialize<LlamaIndexResponse>(responseJson);

                        if (responseData?.choices?.FirstOrDefault()?.message?.content != null)
                        {
                            var responseText = responseData.choices.First().message.content;
                            _logger.LogInformation("Resposta recebida do LlamaIndex para usuário: {UserId}", userId);
                            return responseText;
                        }

                        _logger.LogWarning("LlamaIndex retornou resposta vazia para usuário: {UserId}", userId);
                        return "Desculpe, não consegui gerar uma resposta no momento.";
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning("Tentativa falhou para {BaseUrl}: {StatusCode} - {Content}",
                            baseUrl, response.StatusCode, errorContent);

                        // Se não for a última URL, continuar tentando
                        if (baseUrl != urlsToTry.Last())
                        {
                            continue;
                        }

                        // Se for erro 404, pode indicar que a API mudou ou chave expirou
                        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            _logger.LogWarning("API do LlamaIndex retornou 404. Verifique se a URL da API está correta ou se a chave expirou.");
                            return "Desculpe, o serviço de IA está temporariamente indisponível. Tente novamente mais tarde ou entre em contato com o suporte.";
                        }

                        return "Desculpe, houve um erro ao processar sua mensagem. Tente novamente.";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Erro ao tentar {BaseUrl} para usuário: {UserId}", baseUrl, userId);

                    // Se não for a última URL, continuar tentando
                    if (baseUrl != urlsToTry.Last())
                    {
                        continue;
                    }

                    _logger.LogError(ex, "Erro ao chamar API do LlamaIndex para usuário: {UserId}", userId);
                    return "Desculpe, ocorreu um erro ao processar sua mensagem. Tente novamente.";
                }
            }

            // Se todas as tentativas falharam
            return "O serviço de IA está temporariamente indisponível. Por favor, tente novamente mais tarde.";
        }

        public async Task<string> GetCompletionAsync(string prompt)
        {
            return await GetChatResponseAsync(prompt);
        }
    }

    // DTOs para resposta da API do LlamaIndex
    public class LlamaIndexResponse
    {
        public string id { get; set; } = string.Empty;
        public string @object { get; set; } = string.Empty;
        public long created { get; set; }
        public string model { get; set; } = string.Empty;
        public List<LlamaIndexChoice> choices { get; set; } = new();
        public LlamaIndexUsage usage { get; set; } = new();
    }

    public class LlamaIndexChoice
    {
        public int index { get; set; }
        public LlamaIndexMessage message { get; set; } = new();
        public string finish_reason { get; set; } = string.Empty;
    }

    public class LlamaIndexMessage
    {
        public string role { get; set; } = string.Empty;
        public string content { get; set; } = string.Empty;
    }

    public class LlamaIndexUsage
    {
        public int prompt_tokens { get; set; }
        public int completion_tokens { get; set; }
        public int total_tokens { get; set; }
    }
}
