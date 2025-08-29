using Microsoft.EntityFrameworkCore;
using TutorCopiloto.Models;

namespace TutorCopiloto.Services;

public interface IApplicationDbContext
{
    DbSet<Sessao> Sessoes { get; }
    DbSet<AvaliacaoCodigo> AvaliacoesCodigo { get; }
    DbSet<Usuario> Usuarios { get; }
    DbSet<Interacao> Interacoes { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
