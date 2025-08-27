using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;

namespace TutorCopiloto.Services
{
    public interface IAuthService
    {
        Task<string> GenerateJwtTokenAsync(string userId, string userName, string? email = null);
        Task<ClaimsPrincipal?> ValidateTokenAsync(string token);
        Task<string> GenerateAnonymousTokenAsync(string anonymousId);
        Task<string> GenerateAnonymousTokenAsync(string anonymousId, string displayName);
        Task<bool> IsTokenValidAsync(string token);
        Task<TokenValidationResult?> GetTokenInfoAsync(string token);
    }

    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;

        public AuthService(IConfiguration configuration, ILogger<AuthService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            
            // Configurações JWT do appsettings
            _secretKey = _configuration["JWT:SecretKey"] ?? "DefaultSecretKeyForDevelopmentOnlyNotForProduction2024!@#$";
            _issuer = _configuration["JWT:Issuer"] ?? "TutorCopiloto";
            _audience = _configuration["JWT:Audience"] ?? "TutorCopiloto-Users";
        }

        public async Task<string> GenerateJwtTokenAsync(string userId, string userName, string? email = null)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_secretKey);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Name, userName),
                    new Claim("userId", userId),
                    new Claim("userName", userName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, 
                        new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
                };

                if (!string.IsNullOrEmpty(email))
                {
                    claims.Add(new Claim(ClaimTypes.Email, email));
                    claims.Add(new Claim("email", email));
                }

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddHours(24), // Token válido por 24 horas
                    Issuer = _issuer,
                    Audience = _audience,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                _logger.LogInformation("JWT token gerado para usuário: {UserId}", userId);
                return await Task.FromResult(tokenString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar token JWT para usuário: {UserId}", userId);
                throw;
            }
        }

        public async Task<string> GenerateAnonymousTokenAsync(string anonymousId)
        {
            return await GenerateAnonymousTokenAsync(anonymousId, $"Anonymous_{anonymousId}");
        }

        public async Task<string> GenerateAnonymousTokenAsync(string anonymousId, string displayName)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_secretKey);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, anonymousId),
                    new Claim(ClaimTypes.Name, displayName),
                    new Claim("anonymousId", anonymousId),
                    new Claim("displayName", displayName),
                    new Claim("userType", "anonymous"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, 
                        new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
                };

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddHours(1), // Token anônimo válido por 1 hora
                    Issuer = _issuer,
                    Audience = _audience,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                _logger.LogInformation("Token anônimo gerado para ID: {AnonymousId} com nome: {DisplayName}", anonymousId, displayName);
                return await Task.FromResult(tokenString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar token anônimo para ID: {AnonymousId}", anonymousId);
                throw;
            }
        }

        public async Task<ClaimsPrincipal?> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_secretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                
                if (validatedToken is JwtSecurityToken jwtToken && 
                    jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return await Task.FromResult(principal);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token inválido: {Token}", token.Substring(0, Math.Min(20, token.Length)));
                return null;
            }
        }

        public async Task<bool> IsTokenValidAsync(string token)
        {
            var principal = await ValidateTokenAsync(token);
            return principal != null;
        }

        public async Task<TokenValidationResult?> GetTokenInfoAsync(string token)
        {
            try
            {
                var principal = await ValidateTokenAsync(token);
                if (principal == null)
                    return null;

                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userName = principal.FindFirst(ClaimTypes.Name)?.Value;
                var email = principal.FindFirst(ClaimTypes.Email)?.Value;
                var anonymousId = principal.FindFirst("anonymousId")?.Value;
                var displayName = principal.FindFirst("displayName")?.Value;
                var userType = principal.FindFirst("userType")?.Value;

                return new TokenValidationResult
                {
                    IsValid = true,
                    UserId = userId ?? "",
                    UserName = userName ?? "",
                    Email = email ?? "",
                    AnonymousId = anonymousId ?? "",
                    DisplayName = displayName ?? "",
                    UserType = userType ?? "user",
                    Claims = principal.Claims.ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter informações do token");
                return null;
            }
        }
    }

    // DTOs para autenticação
    public class LoginResult
    {
        public required string Token { get; set; }
        public DateTime Expires { get; set; }
        public required string UserId { get; set; }
        public required string UserName { get; set; }
        public string? Email { get; set; }
    }

    public class LoginRequest
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public string? Email { get; set; }
    }

    public class RegisterRequest
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public string? Email { get; set; }
    }

    public class AnonymousLoginResult
    {
        public required string Token { get; set; }
        public DateTime Expires { get; set; }
        public required string AnonymousId { get; set; }
        public required string DisplayName { get; set; }
    }

    public class AnonymousLoginRequest
    {
        public string? DeviceId { get; set; }
        public string? DisplayName { get; set; }
    }

    public class TokenValidationResult
    {
        public bool IsValid { get; set; }
        public required string UserId { get; set; }
        public required string UserName { get; set; }
        public string? Email { get; set; }
        public string? AnonymousId { get; set; }
        public string? DisplayName { get; set; }
        public string UserType { get; set; } = "user";
        public List<Claim> Claims { get; set; } = new();
    }
}
