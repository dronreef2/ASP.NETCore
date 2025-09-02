using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace TutorCopiloto.Services
{
    /// <summary>
    /// Configurações específicas do OpenAI
    /// </summary>
    public class OpenAIOptions : AIProviderOptions
    {
        public OpenAIOptions()
        {
            BaseUrl = "https://api.openai.com/v1";
            Model = "gpt-3.5-turbo";
        }
    }

    /// <summary>
    /// Provedor de IA usando OpenAI
    /// </summary>
    public class OpenAIService : IAIService
    {
        public string ProviderName => "OpenAI";

        private readonly HttpClient _httpClient;
        private readonly OpenAIOptions _options;
        private readonly ILogger<OpenAIService> _logger;

        public OpenAIService(
            HttpClient httpClient,
            IOptions<OpenAIOptions> options,
            ILogger<OpenAIService> logger)
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

            try
            {
                // Teste simples de conectividade
                var response = await _httpClient.GetAsync("/models");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GetChatResponseAsync(string message, string userId = "anonymous")
        {
            if (!_options.Enabled)
            {
                throw new InvalidOperationException("OpenAI service está desabilitado");
            }

            try
            {
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

                _logger.LogInformation("Enviando mensagem para OpenAI API: {UserId}", userId);

                var response = await _httpClient.PostAsync("/chat/completions", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var responseData = JsonSerializer.Deserialize<OpenAIResponse>(responseJson);

                    if (responseData?.choices?.FirstOrDefault()?.message?.content != null)
                    {
                        var responseText = responseData.choices.First().message.content;
                        _logger.LogInformation("Resposta recebida do OpenAI para usuário: {UserId}", userId);
                        return responseText;
                    }

                    _logger.LogWarning("OpenAI retornou resposta vazia para usuário: {UserId}", userId);
                    return "Desculpe, não consegui gerar uma resposta no momento.";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Erro na API do OpenAI: {StatusCode} - {Content}",
                        response.StatusCode, errorContent);
                    throw new HttpRequestException($"OpenAI API error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao chamar API do OpenAI para usuário: {UserId}", userId);
                throw;
            }
        }

        public async Task<string> GetCompletionAsync(string prompt)
        {
            return await GetChatResponseAsync(prompt);
        }
    }

    // DTOs para resposta da API do OpenAI
    public class OpenAIResponse
    {
        public string id { get; set; } = string.Empty;
        public string @object { get; set; } = string.Empty;
        public long created { get; set; }
        public string model { get; set; } = string.Empty;
        public List<OpenAIChoice> choices { get; set; } = new();
        public OpenAIUsage usage { get; set; } = new();
    }

    public class OpenAIChoice
    {
        public int index { get; set; }
        public OpenAIMessage message { get; set; } = new();
        public string finish_reason { get; set; } = string.Empty;
    }

    public class OpenAIMessage
    {
        public string role { get; set; } = string.Empty;
        public string content { get; set; } = string.Empty;
    }

    public class OpenAIUsage
    {
        public int prompt_tokens { get; set; }
        public int completion_tokens { get; set; }
        public int total_tokens { get; set; }
    }
}
