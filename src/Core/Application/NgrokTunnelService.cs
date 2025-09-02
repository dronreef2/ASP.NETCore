using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TutorCopiloto.Services
{
    public interface INgrokTunnelService
    {
        Task<string?> GetPublicUrlAsync();
        Task<bool> IsRunningAsync();
        Task StartTunnelAsync(int port);
        Task StopTunnelAsync();
    }

    // Implementação dummy para quando ngrok não está disponível
    public class DummyNgrokTunnelService : INgrokTunnelService
    {
        public Task<string?> GetPublicUrlAsync() => Task.FromResult<string?>(null);
        public Task<bool> IsRunningAsync() => Task.FromResult(false);
        public Task StartTunnelAsync(int port) => Task.CompletedTask;
        public Task StopTunnelAsync() => Task.CompletedTask;
    }

    public class NgrokTunnelService : BackgroundService, INgrokTunnelService
    {
        private readonly ILogger<NgrokTunnelService> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private Process? _ngrokProcess;
        private string? _publicUrl;
        private readonly int _targetPort;

        public NgrokTunnelService(
            ILogger<NgrokTunnelService> logger, 
            IConfiguration configuration,
            HttpClient httpClient)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClient;
            _targetPort = _configuration.GetValue<int>("Ngrok:Port", 5000);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_configuration.GetValue<bool>("Ngrok:AutoStart", true))
            {
                await StartTunnelAsync(_targetPort);
            }

            // Monitora o túnel periodicamente
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdatePublicUrlAsync();
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao monitorar túnel ngrok");
                    await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
                }
            }
        }

        public async Task<string?> GetPublicUrlAsync()
        {
            await UpdatePublicUrlAsync();
            return _publicUrl;
        }

        public async Task<bool> IsRunningAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("http://localhost:4040/api/tunnels");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task StartTunnelAsync(int port)
        {
            try
            {
                if (_ngrokProcess != null && !_ngrokProcess.HasExited)
                {
                    _logger.LogInformation("Ngrok já está rodando");
                    return;
                }

                _logger.LogInformation("Iniciando túnel ngrok na porta {Port}", port);

                var ngrokConfig = _configuration["Ngrok:ConfigPath"] ?? "/workspaces/ASP.NETCore/ngrok.yml";
                var endpointName = _configuration["Ngrok:EndpointName"] ?? "tutor-copiloto-aspnet";

                var startInfo = new ProcessStartInfo
                {
                    FileName = "ngrok",
                    Arguments = $"tunnel --config={ngrokConfig} {endpointName}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                _ngrokProcess = Process.Start(startInfo);

                if (_ngrokProcess == null)
                {
                    throw new Exception("Falha ao iniciar processo ngrok");
                }

                _logger.LogInformation("Processo ngrok iniciado com PID: {ProcessId}", _ngrokProcess.Id);

                // Aguarda um tempo para o ngrok inicializar
                await Task.Delay(3000);

                // Tenta obter a URL pública
                await UpdatePublicUrlAsync();

                if (!string.IsNullOrEmpty(_publicUrl))
                {
                    _logger.LogInformation("🌐 Túnel ngrok ativo: {PublicUrl}", _publicUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao iniciar túnel ngrok");
                throw;
            }
        }

        public async Task StopTunnelAsync()
        {
            try
            {
                if (_ngrokProcess != null && !_ngrokProcess.HasExited)
                {
                    _logger.LogInformation("Parando túnel ngrok...");
                    _ngrokProcess.Kill();
                    await _ngrokProcess.WaitForExitAsync();
                    _ngrokProcess.Dispose();
                    _ngrokProcess = null;
                    _publicUrl = null;
                    _logger.LogInformation("Túnel ngrok parado");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao parar túnel ngrok");
            }
        }

        private async Task UpdatePublicUrlAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("http://localhost:4040/api/tunnels");
                
                if (!response.IsSuccessStatusCode)
                {
                    return;
                }

                var content = await response.Content.ReadAsStringAsync();
                var tunnelsResponse = JsonSerializer.Deserialize<NgrokTunnelsResponse>(content);

                var httpsTunnel = tunnelsResponse?.Tunnels?.FirstOrDefault(t => 
                    t.Proto == "https" && t.Config?.Addr?.Contains(_targetPort.ToString()) == true);

                if (httpsTunnel != null)
                {
                    _publicUrl = httpsTunnel.PublicUrl;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Erro ao buscar URL pública do ngrok");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await StopTunnelAsync();
            await base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            _ngrokProcess?.Dispose();
            _httpClient?.Dispose();
            base.Dispose();
        }
    }

    // DTOs para API do ngrok
    public class NgrokTunnelsResponse
    {
        public List<NgrokTunnel>? Tunnels { get; set; }
    }

    public class NgrokTunnel
    {
        public string? Name { get; set; }
        public string? PublicUrl { get; set; }
        public string? Proto { get; set; }
        public NgrokConfig? Config { get; set; }
    }

    public class NgrokConfig
    {
        public string? Addr { get; set; }
    }
}
