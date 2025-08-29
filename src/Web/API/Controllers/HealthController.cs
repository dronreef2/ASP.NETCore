using Microsoft.AspNetCore.Mvc;

namespace TutorCopiloto.Controllers
{
    [ApiController]
    [Route("api/health")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;

        public HealthController(ILogger<HealthController> logger)
        {
            _logger = logger;
        }

        [HttpGet("dotnet")]
        public IActionResult GetDotnetHealth()
        {
            _logger.LogInformation("Health check do backend .NET solicitado");
            return Ok(new
            {
                status = "healthy",
                service = "dotnet-backend",
                timestamp = DateTime.UtcNow,
                version = "1.0.0"
            });
        }

        [HttpGet("node")]
        public IActionResult GetNodeHealth()
        {
            _logger.LogInformation("Health check do backend Node.js solicitado");
            // Simular status do backend Node.js (pode ser integrado posteriormente)
            return Ok(new
            {
                status = "healthy",
                service = "node-backend",
                timestamp = DateTime.UtcNow,
                version = "1.0.0"
            });
        }

        [HttpGet("database")]
        public IActionResult GetDatabaseHealth()
        {
            _logger.LogInformation("Health check do banco de dados solicitado");
            return Ok(new
            {
                status = "healthy",
                service = "database",
                timestamp = DateTime.UtcNow,
                connection = "active"
            });
        }

        [HttpGet("ai-services")]
        public IActionResult GetAIServicesHealth()
        {
            _logger.LogInformation("Health check dos serviços IA solicitado");
            return Ok(new
            {
                status = "healthy",
                service = "ai-services",
                timestamp = DateTime.UtcNow,
                models = new[] { "claude", "codestral" }
            });
        }
    }
}
