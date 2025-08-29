using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TutorCopiloto.Services
{
    // DTOs para API do Claude
    public class ClaudeRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; }

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        [JsonPropertyName("messages")]
        public List<ClaudeMessage> Messages { get; set; } = new();

        [JsonPropertyName("stream")]
        public bool Stream { get; set; }
    }

    public class ClaudeMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    public class ClaudeResponse
    {
        [JsonPropertyName("content")]
        public List<ClaudeContent> Content { get; set; } = new();

        [JsonPropertyName("usage")]
        public ClaudeUsage? Usage { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;
    }

    public class ClaudeContent
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
    }

    public class ClaudeUsage
    {
        [JsonPropertyName("input_tokens")]
        public int InputTokens { get; set; }

        [JsonPropertyName("output_tokens")]
        public int OutputTokens { get; set; }
    }

    public class ClaudeChatCompletionService : IClaudeChatCompletionService, IChatCompletionAdapter
    {
    private readonly HttpClient _httpClient;
    private readonly ILogger<ClaudeChatCompletionService> _logger;
    private readonly string _modelId;
    private readonly int _maxTokens;
    private readonly double _temperature;
    private readonly JsonSerializerOptions _jsonOptions;

        public IReadOnlyDictionary<string, object?> Attributes { get; }

        public ClaudeChatCompletionService(
            HttpClient httpClient,
            string apiKey,
            string modelId = "claude-3-5-sonnet-20241022",
            int maxTokens = 4096,
            double temperature = 0.7,
            ILogger<ClaudeChatCompletionService>? logger = null)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _modelId = modelId;
            _maxTokens = maxTokens;
            _temperature = temperature;

            // Observação: não mutamos headers/base address do HttpClient recebido.
            // Espera-se que o client seja registrado como named/typed via AddHttpClient("Claude") em Program.cs.

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            Attributes = new Dictionary<string, object?>
            {
                ["ModelId"] = modelId,
                ["Provider"] = "Claude",
                ["MaxTokens"] = maxTokens,
                ["Temperature"] = temperature
            };
        }

        public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
            ChatHistory chatHistory,
            PromptExecutionSettings? executionSettings = null,
            Kernel? kernel = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new ClaudeRequest
                {
                    Model = _modelId,
                    MaxTokens = _maxTokens,
                    Temperature = _temperature,
                    Messages = ConvertChatHistoryToClaudeMessages(chatHistory),
                    Stream = false
                };

                var jsonRequest = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                _logger.LogInformation("Enviando requisição para Claude com {MessageCount} mensagens", request.Messages.Count);

                var response = await _httpClient.PostAsync("v1/messages", content, cancellationToken);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Erro na API do Claude: {StatusCode} - {Content}", response.StatusCode, errorContent);
                    return new List<ChatMessageContent>
                    {
                        new(AuthorRole.Assistant, "Desculpe, houve um erro ao processar sua solicitação.")
                    };
                }

                var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                var claudeResponse = JsonSerializer.Deserialize<ClaudeResponse>(responseJson, _jsonOptions);

                if (claudeResponse?.Content?.FirstOrDefault()?.Text != null)
                {
                    var responseText = claudeResponse.Content.First().Text;
                    _logger.LogInformation("Resposta recebida do Claude com {Length} caracteres", responseText.Length);

                    return new List<ChatMessageContent>
                    {
                        new(AuthorRole.Assistant, responseText)
                        {
                            Metadata = new Dictionary<string, object?>
                            {
                                ["TokensUsed"] = claudeResponse.Usage?.OutputTokens ?? 0,
                                ["InputTokens"] = claudeResponse.Usage?.InputTokens ?? 0,
                                ["Provider"] = "Claude",
                                ["Model"] = _modelId
                            }
                        }
                    };
                }

                _logger.LogWarning("Claude retornou resposta vazia");
                return new List<ChatMessageContent>
                {
                    new(AuthorRole.Assistant, "Desculpe, não consegui gerar uma resposta no momento.")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao chamar API do Claude");
                return new List<ChatMessageContent>
                {
                    new(AuthorRole.Assistant, $"Erro ao processar sua solicitação: {ex.Message}")
                };
            }
        }

        public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
            ChatHistory chatHistory,
            PromptExecutionSettings? executionSettings = null,
            Kernel? kernel = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Para simplificar, vamos usar a resposta não-streaming
            var response = await GetChatMessageContentsAsync(chatHistory, executionSettings, kernel, cancellationToken);
            
            foreach (var message in response)
            {
                if (!string.IsNullOrEmpty(message.Content))
                {
                    // Simular streaming dividindo a resposta em chunks
                    var words = message.Content.Split(' ');
                    foreach (var word in words)
                    {
                        yield return new StreamingChatMessageContent(AuthorRole.Assistant, word + " ");
                        await Task.Delay(50, cancellationToken); // Simular delay do streaming
                    }
                }
            }
        }

        // --- Adapter-friendly API ---
        // Implementação simples que envia um prompt e retorna a primeira resposta textual do modelo.
        public async Task<string?> GetChatResponseAsync(string prompt, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(prompt)) return null;

            var chatHistory = new ChatHistory();
            chatHistory.AddUserMessage(prompt);

            try
            {
                var response = await GetChatMessageContentsAsync(chatHistory, cancellationToken: cancellationToken);
                var first = response.FirstOrDefault();
                return first?.Content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no adapter GetChatResponseAsync");
                return null;
            }
        }

        private List<ClaudeMessage> ConvertChatHistoryToClaudeMessages(ChatHistory chatHistory)
        {
            var messages = new List<ClaudeMessage>();

            foreach (var message in chatHistory)
            {
                var role = message.Role.Label.ToLowerInvariant() switch
                {
                    "user" => "user",
                    "assistant" => "assistant",
                    "system" => "user", // Claude não tem role system separado, incorporamos no user
                    _ => "user"
                };

                // Se for uma mensagem do sistema, prefixamos com contexto
                var content = message.Role.Label.ToLowerInvariant() == "system" 
                    ? $"[Instruções do Sistema]: {message.Content}"
                    : message.Content ?? "";

                messages.Add(new ClaudeMessage
                {
                    Role = role,
                    Content = content
                });
            }

            return messages;
        }
    }

    // Service para integração com Semantic Kernel
    public interface IClaudeService
    {
        Task<string> GetResponseAsync(string prompt, CancellationToken cancellationToken = default);
        Task<string> AnalyzeLogsAsync(string logs, string analysisType, CancellationToken cancellationToken = default);
        Task<string> GenerateCodeAsync(string description, string language, CancellationToken cancellationToken = default);
        IAsyncEnumerable<string> GetStreamingResponseAsync(string prompt, CancellationToken cancellationToken = default);
    }

    public class ClaudeService : IClaudeService
    {
        private readonly ClaudeChatCompletionService _chatService;
        private readonly ILogger<ClaudeService> _logger;

        public ClaudeService(ClaudeChatCompletionService chatService, ILogger<ClaudeService> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        public async Task<string> GetResponseAsync(string prompt, CancellationToken cancellationToken = default)
        {
            try
            {
                var chatHistory = new ChatHistory();
                chatHistory.AddUserMessage(prompt);

                var response = await _chatService.GetChatMessageContentsAsync(chatHistory, cancellationToken: cancellationToken);
                return response.FirstOrDefault()?.Content ?? "Sem resposta disponível.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter resposta do Claude");
                return $"Erro: {ex.Message}";
            }
        }

        public async Task<string> AnalyzeLogsAsync(string logs, string analysisType, CancellationToken cancellationToken = default)
        {
            var analysisPrompts = new Dictionary<string, string>
            {
                ["issues"] = $"Analise os seguintes logs e identifique problemas, erros e possíveis soluções:\n\n{logs}",
                ["performance"] = $"Analise os seguintes logs e forneça recomendações de performance:\n\n{logs}",
                ["security"] = $"Analise os seguintes logs em busca de vulnerabilidades de segurança:\n\n{logs}",
                ["anomalies"] = $"Detecte anomalias e comportamentos incomuns nos seguintes logs:\n\n{logs}"
            };

            var prompt = analysisPrompts.GetValueOrDefault(analysisType.ToLower()) 
                ?? analysisPrompts["issues"];

            return await GetResponseAsync(prompt, cancellationToken);
        }

        public async Task<string> GenerateCodeAsync(string description, string language, CancellationToken cancellationToken = default)
        {
            var prompt = $"Gere código {language} limpo e bem documentado para: {description}\n\n" +
                        "Inclua comentários explicativos e siga as melhores práticas da linguagem.";

            return await GetResponseAsync(prompt, cancellationToken);
        }

        public async IAsyncEnumerable<string> GetStreamingResponseAsync(string prompt, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var chatHistory = new ChatHistory();
            chatHistory.AddUserMessage(prompt);

            await foreach (var chunk in _chatService.GetStreamingChatMessageContentsAsync(chatHistory, cancellationToken: cancellationToken))
            {
                if (!string.IsNullOrEmpty(chunk.Content))
                {
                    yield return chunk.Content;
                }
            }
        }
    }
}
