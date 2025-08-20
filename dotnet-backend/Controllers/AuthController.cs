using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TutorCopiloto.Services;

namespace TutorCopiloto.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResult>> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Para desenvolvimento, aceitamos qualquer usuário/senha
                // Em produção, isso deve validar contra um sistema de autenticação real
                if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new { message = "Nome de usuário e senha são obrigatórios" });
                }

                var userId = Guid.NewGuid().ToString();
                var token = await _authService.GenerateJwtTokenAsync(userId, request.UserName, request.Email);

                var result = new LoginResult
                {
                    Token = token,
                    Expires = DateTime.UtcNow.AddHours(24),
                    UserId = userId,
                    UserName = request.UserName,
                    Email = request.Email
                };

                _logger.LogInformation("Login realizado para usuário: {UserName}", request.UserName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante login para usuário: {UserName}", request.UserName);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<LoginResult>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                // Para desenvolvimento, apenas criamos o token
                // Em produção, isso deve criar o usuário no banco de dados
                if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new { message = "Nome de usuário e senha são obrigatórios" });
                }

                var userId = Guid.NewGuid().ToString();
                var token = await _authService.GenerateJwtTokenAsync(userId, request.UserName, request.Email);

                var result = new LoginResult
                {
                    Token = token,
                    Expires = DateTime.UtcNow.AddHours(24),
                    UserId = userId,
                    UserName = request.UserName,
                    Email = request.Email
                };

                _logger.LogInformation("Registro realizado para usuário: {UserName}", request.UserName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante registro para usuário: {UserName}", request.UserName);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        [HttpPost("anonymous")]
        public async Task<ActionResult<AnonymousLoginResult>> LoginAnonymous()
        {
            try
            {
                var anonymousId = Guid.NewGuid().ToString();
                var token = await _authService.GenerateAnonymousTokenAsync(anonymousId);

                var result = new AnonymousLoginResult
                {
                    Token = token,
                    Expires = DateTime.UtcNow.AddHours(1),
                    AnonymousId = anonymousId
                };

                _logger.LogInformation("Login anônimo realizado para ID: {AnonymousId}", anonymousId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante login anônimo");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        [HttpPost("validate")]
        public async Task<ActionResult<TokenValidationResult>> ValidateToken([FromBody] string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest(new { message = "Token é obrigatório" });
                }

                var result = await _authService.GetTokenInfoAsync(token);
                if (result == null)
                {
                    return Unauthorized(new { message = "Token inválido" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante validação de token");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<TokenValidationResult>> GetCurrentUser()
        {
            try
            {
                var authHeader = Request.Headers.Authorization.FirstOrDefault();
                if (authHeader == null || !authHeader.StartsWith("Bearer "))
                {
                    return Unauthorized(new { message = "Token de autorização não encontrado" });
                }

                var token = authHeader.Substring("Bearer ".Length).Trim();
                var result = await _authService.GetTokenInfoAsync(token);
                
                if (result == null)
                {
                    return Unauthorized(new { message = "Token inválido" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter informações do usuário atual");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        [HttpPost("refresh")]
        [Authorize]
        public async Task<ActionResult<LoginResult>> RefreshToken()
        {
            try
            {
                var authHeader = Request.Headers.Authorization.FirstOrDefault();
                if (authHeader == null || !authHeader.StartsWith("Bearer "))
                {
                    return Unauthorized(new { message = "Token de autorização não encontrado" });
                }

                var token = authHeader.Substring("Bearer ".Length).Trim();
                var tokenInfo = await _authService.GetTokenInfoAsync(token);
                
                if (tokenInfo == null)
                {
                    return Unauthorized(new { message = "Token inválido" });
                }

                // Gerar novo token
                var newToken = await _authService.GenerateJwtTokenAsync(
                    tokenInfo.UserId, 
                    tokenInfo.UserName, 
                    tokenInfo.Email);

                var result = new LoginResult
                {
                    Token = newToken,
                    Expires = DateTime.UtcNow.AddHours(24),
                    UserId = tokenInfo.UserId,
                    UserName = tokenInfo.UserName,
                    Email = tokenInfo.Email
                };

                _logger.LogInformation("Token renovado para usuário: {UserId}", tokenInfo.UserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante renovação de token");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }
    }
}
