using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using TutorCopiloto.Models;
using TutorCopiloto.Services;

namespace TutorCopiloto.Controllers;

/// <summary>
/// API Controller para geração e gerenciamento de relatórios educacionais
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class RelatorioController : ControllerBase
{
    private readonly IRelatorioService _relatorioService;
    private readonly IStringLocalizer<RelatorioController> _localizer;
    private readonly ILogger<RelatorioController> _logger;

    public RelatorioController(
        IRelatorioService relatorioService,
        IStringLocalizer<RelatorioController> localizer,
        ILogger<RelatorioController> logger)
    {
        _relatorioService = relatorioService;
        _localizer = localizer;
        _logger = logger;
    }

    /// <summary>
    /// Gera relatório de progresso para um estudante específico
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <param name="inicio">Data de início do período (opcional)</param>
    /// <param name="fim">Data de fim do período (opcional)</param>
    /// <returns>Relatório de progresso do estudante</returns>
    [HttpGet("progresso/{userId}")]
    [ProducesResponseType(typeof(RelatorioProgresso), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<RelatorioProgresso>> GerarRelatorioProgresso(
        string userId,
        [FromQuery] DateTime? inicio = null,
        [FromQuery] DateTime? fim = null)
    {
        try
        {
            _logger.LogInformation("Solicitação de relatório de progresso para usuário {UserId}", userId);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(_localizer["UserIdObrigatorio"]);
            }

            var relatorio = await _relatorioService.GerarRelatorioProgressoAsync(userId, inicio, fim);
            
            return Ok(relatorio);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Parâmetros inválidos: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar relatório de progresso para usuário {UserId}", userId);
            return StatusCode(500, _localizer["ErroInternoServidor"]);
        }
    }

    /// <summary>
    /// Gera relatório de performance para uma turma
    /// </summary>
    /// <param name="turmaId">ID da turma</param>
    /// <param name="ano">Ano do período</param>
    /// <param name="mes">Mês do período</param>
    /// <returns>Relatório da turma</returns>
    [HttpGet("turma/{turmaId}")]
    [ProducesResponseType(typeof(RelatorioTurma), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<RelatorioTurma>> GerarRelatorioTurma(
        string turmaId,
        [FromQuery] int ano = 0,
        [FromQuery] int mes = 0)
    {
        try
        {
            _logger.LogInformation("Solicitação de relatório da turma {TurmaId}", turmaId);

            if (string.IsNullOrWhiteSpace(turmaId))
            {
                return BadRequest(_localizer["TurmaIdObrigatorio"]);
            }

            // Se não especificado, usar mês atual
            if (ano == 0) ano = DateTime.Now.Year;
            if (mes == 0) mes = DateTime.Now.Month;

            if (mes < 1 || mes > 12)
            {
                return BadRequest(_localizer["MesInvalido"]);
            }

            var periodo = new DateTime(ano, mes, 1);
            var relatorio = await _relatorioService.GerarRelatorioTurmaAsync(turmaId, periodo);
            
            return Ok(relatorio);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Parâmetros inválidos: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar relatório da turma {TurmaId}", turmaId);
            return StatusCode(500, _localizer["ErroInternoServidor"]);
        }
    }

    /// <summary>
    /// Gera relatório de uso de ferramentas
    /// </summary>
    /// <param name="inicio">Data de início do período</param>
    /// <param name="fim">Data de fim do período</param>
    /// <returns>Relatório de ferramentas</returns>
    [HttpGet("ferramentas")]
    [ProducesResponseType(typeof(RelatorioFerramentas), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<RelatorioFerramentas>> GerarRelatorioFerramentas(
        [FromQuery] DateTime inicio,
        [FromQuery] DateTime fim)
    {
        try
        {
            _logger.LogInformation("Solicitação de relatório de ferramentas para período {Inicio} - {Fim}", inicio, fim);

            if (inicio == default || fim == default)
            {
                return BadRequest(_localizer["DatasObrigatorias"]);
            }

            if (inicio >= fim)
            {
                return BadRequest(_localizer["DataInicioMenorQueFim"]);
            }

            if ((fim - inicio).TotalDays > 365)
            {
                return BadRequest(_localizer["PeriodoMuitoLongo"]);
            }

            var relatorio = await _relatorioService.GerarRelatorioFerramentasAsync(inicio, fim);
            
            return Ok(relatorio);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Parâmetros inválidos: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar relatório de ferramentas");
            return StatusCode(500, _localizer["ErroInternoServidor"]);
        }
    }

    /// <summary>
    /// Exporta um relatório em diferentes formatos
    /// </summary>
    /// <param name="relatorioId">ID do relatório</param>
    /// <param name="formato">Formato de exportação (PDF, Excel, CSV)</param>
    /// <returns>Arquivo do relatório</returns>
    [HttpGet("exportar/{relatorioId}")]
    [ProducesResponseType(typeof(FileResult), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ExportarRelatorio(
        string relatorioId,
        [FromQuery] FormatoExportacao formato = FormatoExportacao.PDF)
    {
        try
        {
            _logger.LogInformation("Solicitação de exportação do relatório {RelatorioId} no formato {Formato}", 
                relatorioId, formato);

            if (string.IsNullOrWhiteSpace(relatorioId))
            {
                return BadRequest(_localizer["RelatorioIdObrigatorio"]);
            }

            var arquivo = await _relatorioService.ExportarRelatorioAsync(relatorioId, formato);
            
            var (contentType, extensao) = formato switch
            {
                FormatoExportacao.PDF => ("application/pdf", "pdf"),
                FormatoExportacao.Excel => ("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "xlsx"),
                FormatoExportacao.CSV => ("text/csv", "csv"),
                _ => ("application/octet-stream", "bin")
            };

            var nomeArquivo = $"relatorio_{relatorioId}_{DateTime.Now:yyyyMMdd}.{extensao}";
            
            return File(arquivo, contentType, nomeArquivo);
        }
        catch (NotSupportedException ex)
        {
            _logger.LogWarning("Formato não suportado: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (FileNotFoundException)
        {
            return NotFound(_localizer["RelatorioNaoEncontrado"]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao exportar relatório {RelatorioId}", relatorioId);
            return StatusCode(500, _localizer["ErroInternoServidor"]);
        }
    }

    /// <summary>
    /// Obtém lista de relatórios disponíveis para um usuário
    /// </summary>
    /// <param name="userId">ID do usuário (opcional, se não informado, retorna todos)</param>
    /// <returns>Lista de relatórios</returns>
    [HttpGet("lista")]
    [ProducesResponseType(typeof(IEnumerable<ResumoRelatorio>), 200)]
    public async Task<ActionResult<IEnumerable<ResumoRelatorio>>> ListarRelatorios(
        [FromQuery] string? userId = null)
    {
        try
        {
            _logger.LogInformation("Solicitação de lista de relatórios para usuário {UserId}", userId ?? "todos");

            // Implementação placeholder - em produção, buscar do banco
            var relatorios = new List<ResumoRelatorio>
            {
                new ResumoRelatorio
                {
                    Id = Guid.NewGuid().ToString(),
                    Tipo = "Progresso",
                    Titulo = _localizer["RelatorioProgressoIndividual"],
                    CriadoEm = DateTime.Now.AddDays(-1),
                    Status = StatusRelatorio.Concluido
                },
                new ResumoRelatorio
                {
                    Id = Guid.NewGuid().ToString(),
                    Tipo = "Turma",
                    Titulo = _localizer["RelatorioPerformanceTurma"],
                    CriadoEm = DateTime.Now.AddDays(-2),
                    Status = StatusRelatorio.Concluido
                }
            };

            if (!string.IsNullOrWhiteSpace(userId))
            {
                relatorios = relatorios.Where(r => r.UserId == userId).ToList();
            }

            return Ok(relatorios);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar relatórios");
            return StatusCode(500, _localizer["ErroInternoServidor"]);
        }
    }
}
