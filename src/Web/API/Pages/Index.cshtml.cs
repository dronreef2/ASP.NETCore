using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using TutorCopiloto.Data;
using Microsoft.EntityFrameworkCore;

namespace TutorCopiloto.Pages;

/// <summary>
/// Página principal da aplicação Tutor Copiloto
/// </summary>
public class IndexModel : PageModel
{
    private readonly TutorDbContext _context;
    private readonly IStringLocalizer<IndexModel> _localizer;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        TutorDbContext context,
        IStringLocalizer<IndexModel> localizer,
        ILogger<IndexModel> logger)
    {
        _context = context;
        _localizer = localizer;
        _logger = logger;
    }

    // Propriedades para estatísticas da página
    public int TotalUsuarios { get; set; }
    public int TotalSessoes { get; set; }
    public int TotalInteracoes { get; set; }
    public string TempoMedioResposta { get; set; } = "0ms";
    public string CurrentCulture { get; set; } = "pt-BR";

    /// <summary>
    /// Carrega dados para a página inicial
    /// </summary>
    public async Task OnGetAsync(string? culture = null)
    {
        try
        {
            // Definir cultura se fornecida
            if (!string.IsNullOrEmpty(culture))
            {
                CurrentCulture = culture;
                // Em produção, definir cookie de cultura aqui
                Response.Cookies.Append("Culture", culture, new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    SameSite = SameSiteMode.Lax
                });
            }
            else
            {
                // Ler cultura do cookie se existir
                CurrentCulture = Request.Cookies["Culture"] ?? "pt-BR";
            }

            _logger.LogInformation("Carregando página inicial com cultura: {Culture}", CurrentCulture);

            // Carregar estatísticas
            await CarregarEstatisticas();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar página inicial");
            
            // Valores padrão em caso de erro
            TotalUsuarios = 0;
            TotalSessoes = 0;
            TotalInteracoes = 0;
            TempoMedioResposta = "N/A";
        }
    }

    /// <summary>
    /// Carrega estatísticas da plataforma
    /// </summary>
    private async Task CarregarEstatisticas()
    {
        try
        {
            var hoje = DateTime.Today;
            var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);

            // Total de usuários ativos no mês
            TotalUsuarios = await _context.Usuarios
                .Where(u => u.UltimoAcesso >= inicioMes)
                .CountAsync();

            // Total de sessões hoje
            TotalSessoes = await _context.Sessoes
                .Where(s => s.CriadoEm.Date == hoje)
                .CountAsync();

            // Total de interações hoje
            TotalInteracoes = await _context.Interacoes
                .Where(i => i.CriadoEm.Date == hoje)
                .CountAsync();

            // Tempo médio de resposta (últimas 100 interações)
            var temposResposta = await _context.Interacoes
                .Where(i => i.TempoExecucaoMs.HasValue && i.CriadoEm >= hoje.AddDays(-1))
                .OrderByDescending(i => i.CriadoEm)
                .Take(100)
                .Select(i => i.TempoExecucaoMs!.Value)
                .ToListAsync();

            if (temposResposta.Any())
            {
                var mediaMs = temposResposta.Average();
                TempoMedioResposta = FormatarTempo(mediaMs);
            }

            _logger.LogInformation("Estatísticas carregadas: {Usuarios} usuários, {Sessoes} sessões, {Interacoes} interações",
                TotalUsuarios, TotalSessoes, TotalInteracoes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar estatísticas");
            throw;
        }
    }

    /// <summary>
    /// Formata tempo em milissegundos para exibição
    /// </summary>
    private static string FormatarTempo(double milissegundos)
    {
        return milissegundos switch
        {
            < 1000 => $"{milissegundos:F0}ms",
            < 60000 => $"{milissegundos / 1000:F1}s",
            _ => $"{milissegundos / 60000:F1}min"
        };
    }

    /// <summary>
    /// Handler para mudança de idioma via AJAX
    /// </summary>
    public async Task<IActionResult> OnPostChangeCultureAsync(string culture)
    {
        try
        {
            if (string.IsNullOrEmpty(culture) || !IsValidCulture(culture))
            {
                return BadRequest(_localizer["CulturaInvalida"]);
            }

            _logger.LogInformation("Alterando cultura para: {Culture}", culture);

            // Definir cookie de cultura
            Response.Cookies.Append("Culture", culture, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                SameSite = SameSiteMode.Lax,
                HttpOnly = false // Permitir acesso via JavaScript se necessário
            });

            // Recarregar dados com nova cultura
            CurrentCulture = culture;
            await CarregarEstatisticas();

            return new JsonResult(new
            {
                success = true,
                message = _localizer["CulturaAlterada", culture],
                newCulture = culture,
                statistics = new
                {
                    totalUsuarios = TotalUsuarios,
                    totalSessoes = TotalSessoes,
                    totalInteracoes = TotalInteracoes,
                    tempoMedioResposta = TempoMedioResposta
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao alterar cultura para: {Culture}", culture);
            return StatusCode(500, _localizer["ErroAlterarCultura"]);
        }
    }

    /// <summary>
    /// Valida se a cultura é suportada
    /// </summary>
    private static bool IsValidCulture(string culture)
    {
        var supportedCultures = new[] { "pt-BR", "en", "es" };
        return supportedCultures.Contains(culture);
    }

    /// <summary>
    /// Handler para obter estatísticas atualizadas via AJAX
    /// </summary>
    public async Task<IActionResult> OnGetStatisticsAsync()
    {
        try
        {
            await CarregarEstatisticas();

            return new JsonResult(new
            {
                totalUsuarios = TotalUsuarios,
                totalSessoes = TotalSessoes,
                totalInteracoes = TotalInteracoes,
                tempoMedioResposta = TempoMedioResposta,
                timestamp = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter estatísticas atualizadas");
            return StatusCode(500, _localizer["ErroCarregarEstatisticas"]);
        }
    }
}
