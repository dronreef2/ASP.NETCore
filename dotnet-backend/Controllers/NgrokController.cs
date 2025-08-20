using Microsoft.AspNetCore.Mvc;
using TutorCopiloto.Services;

namespace TutorCopiloto.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NgrokController : ControllerBase
    {
        private readonly INgrokTunnelService _ngrokService;
        private readonly ILogger<NgrokController> _logger;

        public NgrokController(INgrokTunnelService ngrokService, ILogger<NgrokController> logger)
        {
            _ngrokService = ngrokService;
            _logger = logger;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            try
            {
                var isRunning = await _ngrokService.IsRunningAsync();
                var publicUrl = await _ngrokService.GetPublicUrlAsync();

                return Ok(new
                {
                    isRunning,
                    publicUrl,
                    webhookUrl = publicUrl != null ? $"{publicUrl}/api/webhook/github" : null,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar status do ngrok");
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        [HttpPost("start")]
        public async Task<IActionResult> Start([FromBody] StartTunnelRequest request)
        {
            try
            {
                await _ngrokService.StartTunnelAsync(request.Port ?? 5000);
                
                // Aguarda um momento para estabilizar
                await Task.Delay(2000);
                
                var publicUrl = await _ngrokService.GetPublicUrlAsync();
                
                return Ok(new
                {
                    status = "started",
                    publicUrl,
                    webhookUrl = publicUrl != null ? $"{publicUrl}/api/webhook/github" : null,
                    message = "Túnel ngrok iniciado com sucesso"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao iniciar túnel ngrok");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("stop")]
        public async Task<IActionResult> Stop()
        {
            try
            {
                await _ngrokService.StopTunnelAsync();
                
                return Ok(new
                {
                    status = "stopped",
                    message = "Túnel ngrok parado com sucesso"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao parar túnel ngrok");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("webhook-url")]
        public async Task<IActionResult> GetWebhookUrl()
        {
            try
            {
                var publicUrl = await _ngrokService.GetPublicUrlAsync();
                
                if (string.IsNullOrEmpty(publicUrl))
                {
                    return NotFound(new { message = "Túnel ngrok não está ativo" });
                }

                var webhookUrl = $"{publicUrl}/api/webhook/github";
                
                return Ok(new
                {
                    webhookUrl,
                    publicUrl,
                    instructions = new
                    {
                        step1 = "Copie a URL do webhook acima",
                        step2 = "Vá para as configurações do seu repositório GitHub",
                        step3 = "Adicione um novo webhook com esta URL",
                        step4 = "Configure para enviar eventos de 'push'",
                        step5 = "Salve as configurações"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter URL do webhook");
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }
    }

    public class StartTunnelRequest
    {
        public int? Port { get; set; }
    }
}
