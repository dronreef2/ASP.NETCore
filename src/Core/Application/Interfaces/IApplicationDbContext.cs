using Microsoft.EntityFrameworkCore;
using TutorCopiloto.Models;
using TutorCopiloto.Domain.Entities;

namespace TutorCopiloto.Services;

public interface IApplicationDbContext
{
    DbSet<Sessao> Sessoes { get; }
    DbSet<AvaliacaoCodigo> AvaliacoesCodigo { get; }
    DbSet<Usuario> Usuarios { get; }
    DbSet<Interacao> Interacoes { get; }
    
    // Repository Analysis entities
    DbSet<Repository> Repositories { get; }
    DbSet<AnalysisReport> AnalysisReports { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
