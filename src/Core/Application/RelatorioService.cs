using Microsoft.Extensions.Localization;
using TutorCopiloto.Data;
using TutorCopiloto.Models;
using Microsoft.EntityFrameworkCore;

namespace TutorCopiloto.Services;

/// <summary>
/// Implementação do serviço de relatórios educacionais
/// </summary>
public class RelatorioService : IRelatorioService
{
    private readonly TutorDbContext _context;
    private readonly IStringLocalizer<RelatorioService> _localizer;
    private readonly ILogger<RelatorioService> _logger;

    public RelatorioService(
        TutorDbContext context,
        IStringLocalizer<RelatorioService> localizer,
        ILogger<RelatorioService> logger)
    {
        _context = context;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<RelatorioProgresso> GerarRelatorioProgressoAsync(string userId, DateTime? inicio = null, DateTime? fim = null)
    {
        _logger.LogInformation("Gerando relatório de progresso para usuário {UserId}", userId);

        inicio ??= DateTime.Now.AddMonths(-1);
        fim ??= DateTime.Now;

        var sessoes = await _context.Sessoes
            .Where(s => s.UserId == userId && s.CriadoEm >= inicio && s.CriadoEm <= fim)
            .Include(s => s.Interacoes)
            .ToListAsync();

        var totalSessoes = sessoes.Count;
        var tempoTotalMinutos = sessoes.Sum(s => s.DuracaoMinutos ?? 0);
        var ferramentasUsadas = sessoes
            .SelectMany(s => s.Interacoes)
            .Where(i => !string.IsNullOrEmpty(i.FerramentaUsada))
            .GroupBy(i => i.FerramentaUsada)
            .ToDictionary(g => g.Key!, g => g.Count());

        var progressoPorTema = await _context.AvaliacoesCodigo
            .Where(a => a.UserId == userId && a.CriadoEm >= inicio && a.CriadoEm <= fim)
            .GroupBy(a => a.Tema)
            .Select(g => new ProgressoTema
            {
                Tema = g.Key,
                NotaMedia = g.Average(a => a.NotaFinal),
                TotalAvaliacoes = g.Count(),
                Evolucao = CalcularEvolucao(g.OrderBy(a => a.CriadoEm).Select(a => a.NotaFinal))
            })
            .ToListAsync();

        return new RelatorioProgresso
        {
            UserId = userId,
            PeriodoInicio = inicio.Value,
            PeriodoFim = fim.Value,
            TotalSessoes = totalSessoes,
            TempoTotalMinutos = tempoTotalMinutos,
            FerramentasUsadas = ferramentasUsadas,
            ProgressoPorTema = progressoPorTema,
            RecomendacoesMelhoria = GerarRecomendacoes(progressoPorTema, ferramentasUsadas),
            GeradoEm = DateTime.Now
        };
    }

    public async Task<RelatorioTurma> GerarRelatorioTurmaAsync(string turmaId, DateTime periodo)
    {
        _logger.LogInformation("Gerando relatório da turma {TurmaId} para o período {Periodo}", turmaId, periodo);

        var inicioMes = new DateTime(periodo.Year, periodo.Month, 1);
        var fimMes = inicioMes.AddMonths(1).AddDays(-1);

        var estudantes = await _context.Usuarios
            .Where(u => u.TurmaId == turmaId)
            .ToListAsync();

        var estatisticasGerais = new EstatisticasTurma
        {
            TotalEstudantes = estudantes.Count,
            EstudantesAtivos = await _context.Sessoes
                .Where(s => estudantes.Select(e => e.Id).Contains(s.UserId) 
                           && s.CriadoEm >= inicioMes && s.CriadoEm <= fimMes)
                .Select(s => s.UserId)
                .Distinct()
                .CountAsync(),
            MediaSessoesPorEstudante = await _context.Sessoes
                .Where(s => estudantes.Select(e => e.Id).Contains(s.UserId) 
                           && s.CriadoEm >= inicioMes && s.CriadoEm <= fimMes)
                .GroupBy(s => s.UserId)
                .Select(g => g.Count())
                .DefaultIfEmpty(0)
                .AverageAsync(),
            TempoMedioSessaoMinutos = await _context.Sessoes
                .Where(s => estudantes.Select(e => e.Id).Contains(s.UserId) 
                           && s.CriadoEm >= inicioMes && s.CriadoEm <= fimMes)
                .AverageAsync(s => s.DuracaoMinutos ?? 0)
        };

        var desempenhoIndividual = new List<DesempenhoEstudante>();
        foreach (var estudante in estudantes)
        {
            var relatorioIndividual = await GerarRelatorioProgressoAsync(estudante.Id, inicioMes, fimMes);
            desempenhoIndividual.Add(new DesempenhoEstudante
            {
                EstudanteId = estudante.Id,
                Nome = estudante.Nome,
                TotalSessoes = relatorioIndividual.TotalSessoes,
                TempoTotalMinutos = relatorioIndividual.TempoTotalMinutos,
                NotaMediaGeral = relatorioIndividual.ProgressoPorTema.Any() 
                    ? relatorioIndividual.ProgressoPorTema.Average(p => p.NotaMedia) 
                    : 0,
                TemasComMaiorDificuldade = relatorioIndividual.ProgressoPorTema
                    .Where(p => p.NotaMedia < 7)
                    .Select(p => p.Tema)
                    .ToList()
            });
        }

        return new RelatorioTurma
        {
            TurmaId = turmaId,
            Periodo = periodo,
            EstatisticasGerais = estatisticasGerais,
            DesempenhoIndividual = desempenhoIndividual,
            TemasMaisDesafiadores = IdentificarTemasMaisDesafiadores(desempenhoIndividual),
            SugestoesMelhorias = GerarSugestoesTurma(estatisticasGerais, desempenhoIndividual),
            GeradoEm = DateTime.Now
        };
    }

    public async Task<RelatorioFerramentas> GerarRelatorioFerramentasAsync(DateTime inicio, DateTime fim)
    {
        _logger.LogInformation("Gerando relatório de ferramentas para o período {Inicio} - {Fim}", inicio, fim);

        var interacoes = await _context.Interacoes
            .Where(i => i.CriadoEm >= inicio && i.CriadoEm <= fim && !string.IsNullOrEmpty(i.FerramentaUsada))
            .ToListAsync();

        var usoFerramentas = interacoes
            .GroupBy(i => i.FerramentaUsada)
            .Select(g => new UsoFerramenta
            {
                Nome = g.Key!,
                TotalUsos = g.Count(),
                UsuariosUnicos = g.Select(i => i.Sessao.UserId).Distinct().Count(),
                TaxaSucesso = g.Count(i => i.Sucesso) / (double)g.Count() * 100,
                TempoMedioExecucaoMs = g.Where(i => i.TempoExecucaoMs.HasValue)
                                       .Average(i => i.TempoExecucaoMs!.Value),
                TiposErroComuns = g.Where(i => !i.Sucesso && !string.IsNullOrEmpty(i.MensagemErro))
                                   .GroupBy(i => i.MensagemErro)
                                   .OrderByDescending(eg => eg.Count())
                                   .Take(5)
                                   .ToDictionary(eg => eg.Key!, eg => eg.Count())
            })
            .OrderByDescending(u => u.TotalUsos)
            .ToList();

        return new RelatorioFerramentas
        {
            PeriodoInicio = inicio,
            PeriodoFim = fim,
            UsoFerramentas = usoFerramentas,
            FerramentaMaisUsada = usoFerramentas.FirstOrDefault()?.Nome ?? _localizer["NenhumaFerramentaUsada"],
            TaxaSucessoGeral = usoFerramentas.Any() ? usoFerramentas.Average(u => u.TaxaSucesso) : 0,
            TotalInteracoes = interacoes.Count,
            GeradoEm = DateTime.Now
        };
    }

    public async Task<byte[]> ExportarRelatorioAsync(string relatorioId, FormatoExportacao formato)
    {
        _logger.LogInformation("Exportando relatório {RelatorioId} no formato {Formato}", relatorioId, formato);

        // Implementação simplificada - em produção, usar bibliotecas específicas
        return formato switch
        {
            FormatoExportacao.PDF => await GerarPdfAsync(relatorioId),
            FormatoExportacao.Excel => await GerarExcelAsync(relatorioId),
            FormatoExportacao.CSV => await GerarCsvAsync(relatorioId),
            _ => throw new NotSupportedException(_localizer["FormatoNaoSuportado", formato])
        };
    }

    private static double CalcularEvolucao(IEnumerable<double> notas)
    {
        var listNotas = notas.ToList();
        if (listNotas.Count < 2) return 0;

        var primeira = listNotas.Take(listNotas.Count / 2).Average();
        var segunda = listNotas.Skip(listNotas.Count / 2).Average();
        
        return ((segunda - primeira) / primeira) * 100;
    }

    private List<string> GerarRecomendacoes(List<ProgressoTema> progresso, Dictionary<string, int> ferramentas)
    {
        var recomendacoes = new List<string>();

        var temasComDificuldade = progresso.Where(p => p.NotaMedia < 7).ToList();
        if (temasComDificuldade.Any())
        {
            recomendacoes.Add(_localizer["RecomendacaoTemasComDificuldade", 
                string.Join(", ", temasComDificuldade.Select(t => t.Tema))]);
        }

        if (!ferramentas.ContainsKey("test-runner"))
        {
            recomendacoes.Add(_localizer["RecomendacaoUsarTestes"]);
        }

        if (progresso.Any(p => p.Evolucao < 0))
        {
            recomendacoes.Add(_localizer["RecomendacaoRevisarConceitos"]);
        }

        return recomendacoes;
    }

    private List<string> IdentificarTemasMaisDesafiadores(List<DesempenhoEstudante> desempenho)
    {
        return desempenho
            .SelectMany(d => d.TemasComMaiorDificuldade)
            .GroupBy(tema => tema)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => g.Key)
            .ToList();
    }

    private List<string> GerarSugestoesTurma(EstatisticasTurma estatisticas, List<DesempenhoEstudante> desempenho)
    {
        var sugestoes = new List<string>();

        if (estatisticas.EstudantesAtivos / (double)estatisticas.TotalEstudantes < 0.7)
        {
            sugestoes.Add(_localizer["SugestaoEngajamento"]);
        }

        if (estatisticas.TempoMedioSessaoMinutos < 15)
        {
            sugestoes.Add(_localizer["SugestaoTempoSessao"]);
        }

        var estudantesComDificuldade = desempenho.Count(d => d.NotaMediaGeral < 6);
        if (estudantesComDificuldade > estatisticas.TotalEstudantes * 0.3)
        {
            sugestoes.Add(_localizer["SugestaoApoioAdicional"]);
        }

        return sugestoes;
    }

    private async Task<byte[]> GerarPdfAsync(string relatorioId)
    {
        // Implementação placeholder - usar bibliotecas como iTextSharp ou PuppeteerSharp
        await Task.Delay(100);
        return System.Text.Encoding.UTF8.GetBytes("PDF placeholder");
    }

    private async Task<byte[]> GerarExcelAsync(string relatorioId)
    {
        // Implementação placeholder - usar EPPlus ou ClosedXML
        await Task.Delay(100);
        return System.Text.Encoding.UTF8.GetBytes("Excel placeholder");
    }

    private async Task<byte[]> GerarCsvAsync(string relatorioId)
    {
        // Implementação placeholder - usar CsvHelper
        await Task.Delay(100);
        return System.Text.Encoding.UTF8.GetBytes("CSV placeholder");
    }
}
