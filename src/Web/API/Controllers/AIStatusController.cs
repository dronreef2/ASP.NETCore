using Microsoft.AspNetCore.Mvc;
using TutorCopiloto.Services;

namespace TutorCopiloto.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIStatusController : ControllerBase
    {
        private readonly AIServiceOrchestrator _aiOrchestrator;
        private readonly ILogger<AIStatusController> _logger;

        public AIStatusController(
            AIServiceOrchestrator aiOrchestrator,
            ILogger<AIStatusController> logger)
        {
            _aiOrchestrator = aiOrchestrator;
            _logger = logger;
        }

        /// <summary>
        /// Obtém o status de todos os provedores de IA
        /// </summary>
        [HttpGet("status")]
        public async Task<ActionResult<AIStatusResponse>> GetStatus()
        {
            try
            {
                var providersStatus = await _aiOrchestrator.GetProvidersStatusAsync();
                var isAvailable = await _aiOrchestrator.IsAvailableAsync();

                var response = new AIStatusResponse
                {
                    IsAvailable = isAvailable,
                    Providers = providersStatus,
                    CurrentProvider = _aiOrchestrator.ProviderName,
                    Timestamp = DateTime.UtcNow
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter status dos provedores de IA");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Testa um provedor específico de IA
        /// </summary>
        [HttpPost("test/{provider}")]
        public async Task<ActionResult<AITestResponse>> TestProvider(string provider, [FromBody] AITestRequest request)
        {
            try
            {
                var testMessage = request?.Message ?? "Olá, este é um teste do sistema de IA. Por favor, confirme que você está funcionando.";

                var startTime = DateTime.UtcNow;
                var response = await _aiOrchestrator.GetChatResponseAsync(testMessage, "test-user");
                var endTime = DateTime.UtcNow;

                var testResponse = new AITestResponse
                {
                    Provider = provider,
                    TestMessage = testMessage,
                    Response = response,
                    ResponseTimeMs = (endTime - startTime).TotalMilliseconds,
                    Timestamp = DateTime.UtcNow,
                    Success = !string.IsNullOrEmpty(response) && !response.Contains("indisponível")
                };

                return Ok(testResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao testar provedor {Provider}", provider);
                return BadRequest(new AITestResponse
                {
                    Provider = provider,
                    TestMessage = request?.Message ?? "Teste",
                    Response = $"Erro: {ex.Message}",
                    ResponseTimeMs = 0,
                    Timestamp = DateTime.UtcNow,
                    Success = false
                });
            }
        }
    }

    // DTOs para as respostas da API
    public class AIStatusResponse
    {
        public bool IsAvailable { get; set; }
        public Dictionary<string, bool> Providers { get; set; } = new();
        public string CurrentProvider { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class AITestRequest
    {
        public string Message { get; set; } = string.Empty;
    }

    public class AITestResponse
    {
        public string Provider { get; set; } = string.Empty;
        public string TestMessage { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public double ResponseTimeMs { get; set; }
        public DateTime Timestamp { get; set; }
        public bool Success { get; set; }
    }
}
