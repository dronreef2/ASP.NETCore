using TutorCopiloto.Models;

namespace TutorCopiloto.Services;

/// <summary>
/// Interface para serviços de relatórios educacionais
/// </summary>
public interface IRelatorioService
{
    /// <summary>
    /// Gera relatório de progresso do estudante
    /// </summary>
    Task<RelatorioProgresso> GerarRelatorioProgressoAsync(string userId, DateTime? inicio = null, DateTime? fim = null);
    
    /// <summary>
    /// Gera relatório de performance da turma
    /// </summary>
    Task<RelatorioTurma> GerarRelatorioTurmaAsync(string turmaId, DateTime periodo);
    
    /// <summary>
    /// Gera métricas de uso das ferramentas
    /// </summary>
    Task<RelatorioFerramentas> GerarRelatorioFerramentasAsync(DateTime inicio, DateTime fim);
    
    /// <summary>
    /// Exporta relatório em diferentes formatos
    /// </summary>
    Task<byte[]> ExportarRelatorioAsync(string relatorioId, FormatoExportacao formato);
}
