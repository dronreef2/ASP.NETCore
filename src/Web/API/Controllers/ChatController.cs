using Microsoft.AspNetCore.Mvc;
using TutorCopiloto.Services;
using System.ComponentModel.DataAnnotations;

namespace TutorCopiloto.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IAIService _aiService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(
            IAIService aiService,
            ILogger<ChatController> logger)
        {
            _aiService = aiService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ChatResponse>> SendMessage([FromBody] ChatRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Message))
                {
                    return BadRequest(new { message = "Mensagem não pode estar vazia" });
                }

                _logger.LogInformation("Processando mensagem de chat do usuário: {UserId}", request.UserId);

                // Usar serviço de IA para processar a mensagem
                var response = await _aiService.GetChatResponseAsync(
                    request.Message,
                    request.UserId ?? "anonymous"
                );

                var chatResponse = new ChatResponse
                {
                    Message = response ?? "Desculpe, não consegui processar sua mensagem.",
                    UserId = request.UserId,
                    Timestamp = DateTime.UtcNow,
                    Model = "llamaindex"
                };

                _logger.LogInformation("Mensagem processada com sucesso para usuário: {UserId}", request.UserId);

                return Ok(chatResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem de chat para usuário: {UserId}", request.UserId);

                return StatusCode(500, new ChatResponse
                {
                    Message = "Desculpe, ocorreu um erro ao processar sua mensagem. Tente novamente.",
                    UserId = request.UserId,
                    Timestamp = DateTime.UtcNow,
                    Model = "error"
                });
            }
        }

        [HttpGet("models")]
        public ActionResult<IEnumerable<ChatModel>> GetAvailableModels()
        {
            var models = new List<ChatModel>
            {
                new ChatModel { Id = "llamaindex", Name = "LlamaIndex", Provider = "LlamaIndex" }
            };

            return Ok(models);
        }
    }

    // DTOs
    public class ChatRequest
    {
        [Required]
        public string Message { get; set; } = string.Empty;

        public string? UserId { get; set; }
        public string? Model { get; set; }
        public int? MaxTokens { get; set; }
    }

    public class ChatResponse
    {
        public string Message { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Model { get; set; } = string.Empty;
    }

    public class ChatModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
    }
}
