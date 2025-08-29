using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TutorCopiloto.Services
{
    public class CodestralOptions
    {
        public string? ApiKey { get; set; }
        public string? BaseUrl { get; set; }
        public string? Model { get; set; }
    }

    /// <summary>
    /// Adapter para o serviço Codestral (Mistral) que implementa IChatCompletionAdapter.
    /// Use a configuração em Codestral:ApiKey e Codestral:BaseUrl (opcional).
    /// </summary>
    public class CodestralChatCompletionAdapter : IChatCompletionAdapter
    {
        private readonly HttpClient _http;
        private readonly ILogger<CodestralChatCompletionAdapter> _logger;
        private readonly CodestralOptions _options;

        public CodestralChatCompletionAdapter(HttpClient http, IConfiguration config, ILogger<CodestralChatCompletionAdapter> logger)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _logger = logger;

            _options = new CodestralOptions
            {
                ApiKey = config["Codestral:ApiKey"],
                BaseUrl = config["Codestral:BaseUrl"] ?? "https://codestral.mistral.ai/",
                Model = config["Codestral:Model"] ?? "mistral-v1"
            };

            // Não sobrescrever cabeçalhos se já foram configurados no AddHttpClient
            if (string.IsNullOrEmpty(_http.DefaultRequestHeaders.Authorization?.Parameter) && !string.IsNullOrEmpty(_options.ApiKey))
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
            }

            if (_http.BaseAddress == null && !string.IsNullOrEmpty(_options.BaseUrl))
            {
                _http.BaseAddress = new Uri(_options.BaseUrl);
            }
        }

    public async Task<string?> GetChatResponseAsync(string prompt, System.Threading.CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                return string.Empty;

            try
            {
                // Tenta primeiro o endpoint de chat (mais adequado para prompts com histórico)
                var chatReq = new
                {
                    model = _options.Model,
                    messages = new[] { new { role = "user", content = prompt } },
                    temperature = 0.2,
                    max_tokens = 1200
                };

                var content = new StringContent(JsonSerializer.Serialize(chatReq), Encoding.UTF8, "application/json");
                var resp = await _http.PostAsync("/v1/chat/completions", content, cancellationToken);

                var body = await resp.Content.ReadAsStringAsync(cancellationToken);

                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Codestral chat endpoint returned {Status}. Body: {Body}", resp.StatusCode, body);
                }

                var extracted = ExtractTextFromResponse(body);
                if (!string.IsNullOrEmpty(extracted))
                    return extracted;

                // Fallback para endpoint de completions (fim/completions)
                var compReq = new
                {
                    model = _options.Model,
                    input = prompt,
                    temperature = 0.2,
                    max_tokens = 1200
                };

                var compContent = new StringContent(JsonSerializer.Serialize(compReq), Encoding.UTF8, "application/json");
                var compResp = await _http.PostAsync("/v1/fim/completions", compContent, cancellationToken);
                var compBody = await compResp.Content.ReadAsStringAsync(cancellationToken);

                if (!compResp.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Codestral completions endpoint returned {Status}. Body: {Body}", compResp.StatusCode, compBody);
                }

                return ExtractTextFromResponse(compBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao chamar Codestral");
                return null;
            }
        }

    private string? ExtractTextFromResponse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // common pattern: { choices: [ { message: { content: "..." } } ] }
                if (root.TryGetProperty("choices", out var choices) && choices.ValueKind == JsonValueKind.Array && choices.GetArrayLength() > 0)
                {
                    var first = choices[0];
                    if (first.TryGetProperty("message", out var message) && message.TryGetProperty("content", out var contentProp))
                    {
                        return contentProp.GetString();
                    }

                    if (first.TryGetProperty("text", out var textProp))
                    {
                        return textProp.GetString();
                    }
                }

                // pattern: { output: "..." } or { result: "..." }
                if (root.TryGetProperty("output", out var outProp) && outProp.ValueKind == JsonValueKind.String)
                    return outProp.GetString();

                if (root.TryGetProperty("result", out var resProp) && resProp.ValueKind == JsonValueKind.String)
                    return resProp.GetString();

                // pattern: { data: { text: "..." } }
                if (root.TryGetProperty("data", out var data) && data.TryGetProperty("text", out var text2))
                    return text2.GetString();

                // último recurso: retornar texto bruto
                return json;
            }
            catch (JsonException)
            {
                return json;
            }
        }
    }

    public static class CodestralServiceCollectionExtensions
    {
        /// <summary>
        /// Registra o adaptador Codestral como implementação de IChatCompletionAdapter.
        /// Configure Codestral:ApiKey e opcionalmente Codestral:BaseUrl e Codestral:Model em config.
        /// </summary>
        public static IServiceCollection AddCodestralChatCompletion(this IServiceCollection services)
        {
            services.AddHttpClient<IChatCompletionAdapter, CodestralChatCompletionAdapter>((sp, client) =>
            {
                var cfg = sp.GetRequiredService<IConfiguration>();
                var baseUrl = cfg["Codestral:BaseUrl"] ?? "https://codestral.mistral.ai/";
                client.BaseAddress = new Uri(baseUrl);

                var apiKey = cfg["Codestral:ApiKey"];
                if (!string.IsNullOrEmpty(apiKey))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                }
            });

            return services;
        }
    }
}
