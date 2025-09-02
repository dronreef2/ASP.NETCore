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

            // ===== CARREGAR VARIÁVEIS DE AMBIENTE =====
            builder.Configuration
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddUserSecrets<Program>(optional: true);

            // Carregar arquivo .env se existir (desenvolvimento)
            var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
            if (File.Exists(envPath))
            {
                builder.Configuration.AddEnvironmentVariables();
                DotNetEnv.Env.Load(envPath);
            }

            // ===== CONFIGURAÇÃO DE LOGGING PARA RAILWAY =====
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/tutor-copiloto-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            builder.Host.UseSerilog();

            // ===== CONFIGURAÇÃO DE SERVIÇOS =====

            // 1. Entity Framework - Suporte Railway PostgreSQL
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            var connectionString = databaseUrl ??
                builder.Configuration.GetConnectionString("DefaultConnection") ??
                "Data Source=tutorcopiloto.db";

            builder.Services.AddDbContext<TutorDbContext>(options =>
            {
                if (!string.IsNullOrEmpty(databaseUrl) && databaseUrl.Contains("postgresql"))
                {
                    // Railway PostgreSQL
                    options.UseNpgsql(databaseUrl);
                }
                else
                {
                    // SQLite local (desenvolvimento/fallback)
                    options.UseSqlite(connectionString);
                }

                if (builder.Environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
            });

            // 2. Redis Cache - Suporte Railway Redis
            var redisUrl = Environment.GetEnvironmentVariable("REDIS_URL") ??
                builder.Configuration.GetConnectionString("Redis") ??
                "localhost:6379";

            try
            {
                builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
                {
                    var configuration = ConfigurationOptions.Parse(redisUrl);
                    configuration.AbortOnConnectFail = false;
                    configuration.ConnectRetry = 3;
                    configuration.ConnectTimeout = 5000;
                    return ConnectionMultiplexer.Connect(configuration);
                });

                builder.Services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisUrl;
                });
            }
            catch (Exception ex)
            {
                Log.Warning("Redis não configurado, usando cache em memória: {Error}", ex.Message);
                builder.Services.AddMemoryCache();
            }

            // 3. AI Services Configuration
            builder.Services.Configure<AIServiceOptions>(
                builder.Configuration.GetSection("AI"));

            // LlamaIndex Service
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

            // OpenAI Service
            builder.Services.Configure<OpenAIOptions>(
                builder.Configuration.GetSection("AI:OpenAI"));

            builder.Services.AddHttpClient<OpenAIService>((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<OpenAIOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", options.ApiKey);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            });

            // Anthropic Service
            builder.Services.Configure<AnthropicOptions>(
                builder.Configuration.GetSection("AI:Anthropic"));

            builder.Services.AddHttpClient<AnthropicService>((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<AnthropicOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.DefaultRequestHeaders.Add("x-api-key", options.ApiKey);
                client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            });

            // Register AI Services
            builder.Services.AddScoped<LlamaIndexService>();
            builder.Services.AddScoped<OpenAIService>();
            builder.Services.AddScoped<AnthropicService>();

            // Register AI Service Orchestrator
            builder.Services.AddScoped<AIServiceOrchestrator>();
            builder.Services.AddScoped<IAIService, AIServiceOrchestrator>(provider =>
                provider.GetRequiredService<AIServiceOrchestrator>());

            // 4. Repository Analysis Services
            builder.Services.Configure<RepositoryCollectionOptions>(
                builder.Configuration.GetSection("RepositoryAnalysis:Collection"));

            builder.Services.AddHttpClient<RepositoryCollectionService>((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<RepositoryCollectionOptions>>().Value;
                client.BaseAddress = new Uri(options.GitHubApiBaseUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
                client.DefaultRequestHeaders.Add("User-Agent", "TutorCopiloto-RepositoryAnalyzer/1.0");

                if (!string.IsNullOrEmpty(options.GitHubApiKey))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", options.GitHubApiKey);
                }
            });

            builder.Services.Configure<QualityAnalysisOptions>(
                builder.Configuration.GetSection("RepositoryAnalysis:Quality"));

            builder.Services.AddHttpClient<QualityAnalysisService>((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<QualityAnalysisOptions>>().Value;
                client.BaseAddress = new Uri(options.AnalysisServiceUrl);
                client.Timeout = TimeSpan.FromSeconds(options.AnalysisTimeoutSeconds);
            });

            builder.Services.Configure<RepositoryAnalysisOrchestratorOptions>(
                builder.Configuration.GetSection("RepositoryAnalysis:Orchestrator"));

            // Register Repository Analysis Services
            builder.Services.AddScoped<RepositoryCollectionService>();
            builder.Services.AddScoped<QualityAnalysisService>();
            builder.Services.AddScoped<RepositoryAnalysisOrchestrator>();

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

            // Ngrok Tunnel Service - apenas em desenvolvimento
            if (builder.Configuration.GetValue<bool>("Ngrok:Enabled", false))
            {
                builder.Services.AddSingleton<INgrokTunnelService, NgrokTunnelService>();
                builder.Services.AddHostedService<NgrokTunnelService>(provider =>
                    (NgrokTunnelService)provider.GetRequiredService<INgrokTunnelService>());
            }
            else
            {
                // Implementação dummy para produção
                builder.Services.AddSingleton<INgrokTunnelService, DummyNgrokTunnelService>();
            }

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
            var jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ??
                builder.Configuration["JWT:SecretKey"];

            if (string.IsNullOrEmpty(jwtSecretKey))
            {
                if (builder.Environment.IsDevelopment())
                {
                    jwtSecretKey = "STRONG_DEFAULT_DEV_KEY_CHANGE_ME_#@$!_2024";
                    Log.Warning("Using a default JWT secret key. Please configure a proper secret for development.");
                }
                else
                {
                    throw new InvalidOperationException("JWT Secret Key is not configured. Please set the 'JWT_SECRET_KEY' environment variable.");
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

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Log.Warning("JWT Authentication failed: {Error}", context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Log.Information("JWT Token validated for user: {User}", context.Principal?.Identity?.Name);
                        return Task.CompletedTask;
                    }
                };
            });

            // 6. CORS Configuration
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .WithExposedHeaders("Content-Disposition");
                });

                options.AddPolicy("AllowSpecificOrigins", policy =>
                {
                    var allowedOrigins = builder.Configuration.GetSection("CORS:AllowedOrigins").Get<string[]>() ??
                        new[] { "http://localhost:3000", "http://localhost:4200", "https://localhost:3000", "https://localhost:4200" };

                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials()
                          .WithExposedHeaders("Content-Disposition");
                });
            });

            // 7. Controllers e API
            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<GlobalExceptionFilter>();
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
                options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
            });

            // 8. Swagger/OpenAPI
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Tutor Copiloto API",
                    Version = "v1",
                    Description = "API para análise inteligente de repositórios GitHub",
                    Contact = new OpenApiContact
                    {
                        Name = "Tutor Copiloto Team",
                        Email = "support@tutorcopiloto.com"
                    }
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

            // 9. Health Checks
            builder.Services.AddHealthChecks()
                .AddDbContextCheck<TutorDbContext>("Database")
                .AddRedis("Redis")
                .AddUrlGroup(new Uri("https://api.github.com"), "GitHub API");

            // 10. Localization
            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("en-US"),
                    new CultureInfo("pt-BR"),
                    new CultureInfo("es-ES")
                };

                options.DefaultRequestCulture = new RequestCulture("pt-BR");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            // ===== CONFIGURAÇÃO DO PIPELINE =====
            var app = builder.Build();

            // Middleware de localização
            app.UseRequestLocalization();

            // Health Checks
            app.MapHealthChecks("/health");

            // Swagger
            if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:Enabled", false))
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Tutor Copiloto API v1");
                    options.RoutePrefix = "swagger";
                });
            }

            // CORS
            var corsPolicy = app.Configuration.GetValue<string>("CORS:Policy", "AllowSpecificOrigins");
            app.UseCors(corsPolicy);

            // HTTPS Redirection
            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            // Authentication & Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Static Files
            app.UseStaticFiles();

            // Routing
            app.UseRouting();

            // SignalR Hubs
            app.MapHub<AnalysisHub>("/analysisHub");
            app.MapHub<NotificationHub>("/notificationHub");

            // Controllers
            app.MapControllers();

            // Fallback to SPA
            app.MapFallbackToFile("index.html");

            // ===== INICIALIZAÇÃO DO BANCO =====
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<TutorDbContext>();
                    if (context.Database.IsRelational())
                    {
                        await context.Database.MigrateAsync();
                    }
                    else
                    {
                        await context.Database.EnsureCreatedAsync();
                    }

                    Log.Information("Database initialized successfully");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An error occurred while initializing the database");
                    throw;
                }
            }

            Log.Information("Tutor Copiloto API started successfully");
            app.Run();
        }
    }
}
