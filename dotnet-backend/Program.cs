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

            // 1. Entity Framework + PostgreSQL
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                ?? Environment.GetEnvironmentVariable("POSTGRES_URL")
                ?? "Host=localhost;Database=tutordb;Username=tutor;Password=copiloto123";

            builder.Services.AddDbContext<TutorDbContext>(options =>
            {
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure();
                });
                
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

            // 3. Injeção de Dependência - Serviços customizados
            builder.Services.AddScoped<IRelatorioService, RelatorioService>();

            // 4. SignalR para tempo real
            builder.Services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = builder.Environment.IsDevelopment();
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
            });

            // 5. Razor Pages
            builder.Services.AddRazorPages();

            // 6. API Controllers
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
                });

            // 7. Swagger/OpenAPI
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
                    Description = "JWT Authorization header using the Bearer scheme",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
            });

            // 8. Internacionalização (i18n)
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

            // 9. CORS
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    var corsOrigins = builder.Configuration["CORS:Origins"]?.Split(',')
                        ?? Environment.GetEnvironmentVariable("CORS_ORIGINS")?.Split(',')
                        ?? new[] { "http://localhost:3000", "http://localhost:5173" };

                    policy.WithOrigins(corsOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            // 10. Health Checks
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

            // 5. Mapping
            app.MapRazorPages();
            app.MapControllers();
            app.MapHub<ChatHub>("/chathub");

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
