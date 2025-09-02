using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace TutorCopiloto.Services
{
    /// <summary>
    /// Configurações específicas do Anthropic
    /// </summary>
    public class AnthropicOptions : AIProviderOptions
    {
        public AnthropicOptions()
        {
            BaseUrl = "https://api.anthropic.com/v1";
            Model = "claude-3-sonnet-20240229";
        }
    }

    /// <summary>
    /// Provedor de IA usando Anthropic Claude
    /// </summary>
    public class AnthropicService : IAIService
    {
        public string ProviderName => "Anthropic";

        private readonly HttpClient _httpClient;
        private readonly AnthropicOptions _options;
        private readonly ILogger<AnthropicService> _logger;

        public AnthropicService(
            HttpClient httpClient,
            IOptions<AnthropicOptions> options,
            ILogger<AnthropicService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;

            // Configurar headers padrão
            _httpClient.BaseAddress = new Uri(_options.BaseUrl);
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _options.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
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
                // Teste simples de conectividade - Anthropic não tem endpoint de listagem de modelos
                // Vamos fazer um teste com uma mensagem curta
                var testMessage = new
                {
                    model = _options.Model,
                    max_tokens = 10,
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = "Test"
                        }
                    }
                };

                var jsonContent = JsonSerializer.Serialize(testMessage);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/messages", content);
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
                throw new InvalidOperationException("Anthropic service está desabilitado");
            }

            try
            {
                var request = new
                {
                    model = _options.Model,
                    max_tokens = _options.MaxTokens,
                    temperature = _options.Temperature,
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = message
                        }
                    }
                };

                var jsonContent = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation("Enviando mensagem para Anthropic API: {UserId}", userId);

                var response = await _httpClient.PostAsync("/messages", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var responseData = JsonSerializer.Deserialize<AnthropicResponse>(responseJson);

                    if (responseData?.content?.FirstOrDefault()?.text != null)
                    {
                        var responseText = responseData.content.First().text;
                        _logger.LogInformation("Resposta recebida do Anthropic para usuário: {UserId}", userId);
                        return responseText;
                    }

                    _logger.LogWarning("Anthropic retornou resposta vazia para usuário: {UserId}", userId);
                    return "Desculpe, não consegui gerar uma resposta no momento.";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Erro na API do Anthropic: {StatusCode} - {Content}",
                        response.StatusCode, errorContent);
                    throw new HttpRequestException($"Anthropic API error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao chamar API do Anthropic para usuário: {UserId}", userId);
                throw;
            }
        }

        public async Task<string> GetCompletionAsync(string prompt)
        {
            return await GetChatResponseAsync(prompt);
        }
    }

    // DTOs para resposta da API do Anthropic
    public class AnthropicResponse
    {
        public string id { get; set; } = string.Empty;
        public string type { get; set; } = string.Empty;
        public string role { get; set; } = string.Empty;
        public List<AnthropicContent> content { get; set; } = new();
        public string model { get; set; } = string.Empty;
        public string stop_reason { get; set; } = string.Empty;
        public AnthropicUsage usage { get; set; } = new();
    }

    public class AnthropicContent
    {
        public string type { get; set; } = string.Empty;
        public string text { get; set; } = string.Empty;
    }

    public class AnthropicUsage
    {
        public int input_tokens { get; set; }
        public int output_tokens { get; set; }
    }
}
