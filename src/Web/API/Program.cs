using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using StackExchange.Redis;
using System.Globalization;
using System.Text;
using TutorCopiloto.Data;
using TutorCopiloto.Hubs;
using TutorCopiloto.Services;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace TutorCopiloto
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ===== CONFIGURAÇÃO DE LOGGING =====
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/tutor-copiloto-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            builder.Host.UseSerilog();

            // ===== CONFIGURAÇÃO DE SERVIÇOS =====

            // 1. Entity Framework + SQLite
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                ?? Environment.GetEnvironmentVariable("DATABASE_URL")
                ?? "Data Source=tutorcopiloto.db";

            builder.Services.AddDbContext<TutorDbContext>(options =>
            {
                options.UseSqlite(connectionString);
                
                if (builder.Environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
            });

            // 2. Redis Cache (optional)
            var redisConnectionString = builder.Configuration.GetConnectionString("Redis")
                ?? Environment.GetEnvironmentVariable("REDIS_URL")
                ?? "localhost:6379";

            try 
            {
                builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
                {
                    var configuration = ConfigurationOptions.Parse(redisConnectionString);
                    configuration.AbortOnConnectFail = false;
                    configuration.ConnectRetry = 3;
                    configuration.ConnectTimeout = 5000;
                    return ConnectionMultiplexer.Connect(configuration);
                });

                builder.Services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                });
            }
            catch (Exception ex)
            {
                Log.Warning("Redis não configurado, usando cache em memória: {Error}", ex.Message);
                builder.Services.AddMemoryCache();
            }

            // 3. LlamaIndex AI Service
            builder.Services.Configure<LlamaIndexOptions>(
                builder.Configuration.GetSection("AI:LlamaIndex"));

            builder.Services.AddHttpClient<LlamaIndexService>((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<LlamaIndexOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", options.ApiKey);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            });

            builder.Services.AddScoped<LlamaIndexService>();

            // 4. Injeção de Dependência - Serviços customizados
            builder.Services.AddScoped<IRelatorioService, RelatorioService>();
            builder.Services.AddScoped<IDeploymentService, DeploymentService>();
            builder.Services.AddScoped<IRepositoryAnalysisService, RepositoryAnalysisService>();
            builder.Services.AddScoped<IIntelligentAnalysisService, IntelligentAnalysisService>();
            builder.Services.AddScoped<IOnnxInferenceService, OnnxInferenceService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IGitService, GitService>();
            builder.Services.AddScoped<IGitHubChatIntegrationService, GitHubChatIntegrationService>();
            builder.Services.AddScoped<GitHubMcpService>();
            builder.Services.AddSingleton<INgrokTunnelService, NgrokTunnelService>();
            builder.Services.AddHostedService<NgrokTunnelService>(provider => 
                (NgrokTunnelService)provider.GetRequiredService<INgrokTunnelService>());
            builder.Services.AddHttpClient();

            // 4. SignalR para tempo real
            builder.Services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = builder.Environment.IsDevelopment();
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
                options.MaximumReceiveMessageSize = 1024 * 1024; // 1MB
            });

            // 5. JWT Authentication
            var jwtSecretKey = builder.Configuration["JWT:SecretKey"] ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY");

            if (string.IsNullOrEmpty(jwtSecretKey))
            {
                if (builder.Environment.IsDevelopment())
                {
                    // In development, we can allow a default key but it's not recommended for production.
                    // For a real application, use .NET Secret Manager or environment variables.
                    // DO NOT USE THIS KEY IN PRODUCTION.
                    jwtSecretKey = "STRONG_DEFAULT_DEV_KEY_CHANGE_ME_#@$!_2024";
                    Log.Warning("Using a default JWT secret key. Please configure a proper secret for development using .NET Secret Manager.");
                }
                else
                {
                    throw new InvalidOperationException("JWT Secret Key is not configured. Please set the 'JWT:SecretKey' in configuration or 'JWT_SECRET_KEY' environment variable.");
                }
            }

            var jwtIssuer = builder.Configuration["JWT:Issuer"] ?? "TutorCopiloto";
            var jwtAudience = builder.Configuration["JWT:Audience"] ?? "TutorCopiloto-Users";

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
                    ClockSkew = TimeSpan.Zero
                };

                // Eventos especiais para SignalR
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // Se a requisição for para o SignalR hub
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && 
                            (path.StartsWithSegments("/chathub")))
                        {
                            // Ler o token da query string
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddAuthorization();

            // 6. Razor Pages
            builder.Services.AddRazorPages();

            // 7. API Controllers
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
                });

            // 8. Swagger/OpenAPI
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Tutor Copiloto API",
                    Version = "v1",
                    Description = "API para plataforma educacional com IA",
                    Contact = new OpenApiContact
                    {
                        Name = "Tutor Copiloto",
                        Email = "api@tutorcopiloto.com",
                        Url = new Uri("https://tutorcopiloto.com")
                    }
                });

                // JWT Authentication
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // 9. Internacionalização (i18n)
            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("pt-BR"),
                    new CultureInfo("en"),
                    new CultureInfo("es")
                };

                options.DefaultRequestCulture = new RequestCulture("pt-BR");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;

                // Providers de localização
                options.RequestCultureProviders.Clear();
                options.RequestCultureProviders.Add(new QueryStringRequestCultureProvider());
                options.RequestCultureProviders.Add(new CookieRequestCultureProvider());
                options.RequestCultureProviders.Add(new AcceptLanguageHeaderRequestCultureProvider());
            });

            // 10. CORS
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    var corsOrigins = builder.Configuration["CORS:Origins"]?.Split(',')
                        ?? Environment.GetEnvironmentVariable("CORS_ORIGINS")?.Split(',')
                        ?? new[] { "http://localhost:3000", "http://localhost:5173", "http://localhost:5000" };

                    policy.WithOrigins(corsOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });

                // Política específica para SignalR
                options.AddPolicy("SignalRPolicy", policy =>
                {
                    var corsOrigins = builder.Configuration["CORS:Origins"]?.Split(',')
                        ?? Environment.GetEnvironmentVariable("CORS_ORIGINS")?.Split(',')
                        ?? new[] { "http://localhost:3000", "http://localhost:5173", "http://localhost:5000" };

                    policy.WithOrigins(corsOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials()
                          .WithExposedHeaders("X-Connection-Id");
                });
            });

            // 11. Health Checks
            builder.Services.AddHealthChecks();

            // ===== CONFIGURAÇÃO DA APLICAÇÃO =====
            var app = builder.Build();

            // 1. Request Localization
            app.UseRequestLocalization();

            // 2. Development/Production middleware
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tutor Copiloto API v1");
                    c.RoutePrefix = "swagger";
                });
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            // 3. Core middleware
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            // 4. CORS
            app.UseCors();

            // 5. Authentication & Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // 6. Mapping
            app.MapRazorPages();
            app.MapControllers();
            app.MapHub<ChatHub>("/chathub").RequireCors("SignalRPolicy");

            // 6. Health Checks
            app.MapHealthChecks("/health");

            // 7. API Info endpoint
            app.MapGet("/api/info", () => new
            {
                name = "Tutor Copiloto API",
                version = "1.0.0",
                environment = app.Environment.EnvironmentName,
                timestamp = DateTime.UtcNow,
                features = new
                {
                    signalr = true,
                    swagger = app.Environment.IsDevelopment(),
                    internationalization = true,
                    database = true
                }
            }).WithTags("Info");

            Log.Information("🎓 Tutor Copiloto ASP.NET Core iniciando...");
            Log.Information("📡 SignalR Hub disponível em /chathub");
            Log.Information("📖 Documentação Swagger disponível em /swagger");
            Log.Information("🏥 Health checks disponíveis em /health");

            app.Run();
        }
    }
}
