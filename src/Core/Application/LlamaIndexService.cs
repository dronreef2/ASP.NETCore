using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace TutorCopiloto.Services
{
    public class LlamaIndexOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://api.cloud.llamaindex.ai/a
pi/v1";
        public string Model { get; set; } = "gpt-3.5-turbo";
        public int MaxTokens { get; set; } = 1000;
        public double Temperature { get; set; } = 0.7;
        public int TimeoutSeconds { get; set; } = 30;
    }

    public class LlamaIndexService
    {
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

        public async Task<string> GetChatResponseAsync(string message, string us
erId = "anonymous")
        {
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
                var content = new StringContent(jsonContent, Encoding.UTF8, "app
lication/json");

                _logger.LogInformation("Enviando mensagem para LlamaIndex API: {
UserId}", userId);

                var response = await _httpClient.PostAsync("/chat/completions",
content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(
);
                    _logger.LogError("Erro na API do LlamaIndex: {StatusCode} -
{Content}",
                        response.StatusCode, errorContent);
                    return "Desculpe, houve um erro ao processar sua mensagem. T
ente novamente.";
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var responseData = JsonSerializer.Deserialize<LlamaIndexResponse
>(responseJson);

                if (responseData?.choices?.FirstOrDefault()?.message?.content !=
 null)
                {
                    var responseText = responseData.choices.First().message.cont
ent;
                    _logger.LogInformation("Resposta recebida do LlamaIndex para
 usuário: {UserId}", userId);
                    return responseText;
                }

                _logger.LogWarning("LlamaIndex retornou resposta vazia para usu
ário: {UserId}", userId);
                return "Desculpe, não consegui gerar uma resposta no momento.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao chamar API do LlamaIndex para usu
ário: {UserId}", userId);
                return "Desculpe, ocorreu um erro ao processar sua mensagem. Ten
te novamente.";
            }
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
