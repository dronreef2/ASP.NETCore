using Microsoft.EntityFrameworkCore;
using TutorCopiloto.Models;
using TutorCopiloto.Services;

namespace TutorCopiloto.Data;

/// <summary>
/// Contexto de banco de dados para o Tutor Copiloto
/// </summary>
public class TutorDbContext : DbContext, IApplicationDbContext
{
    public TutorDbContext(DbContextOptions<TutorDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Sessao> Sessoes => Set<Sessao>();
    public DbSet<Interacao> Interacoes => Set<Interacao>();
    public DbSet<AvaliacaoCodigo> AvaliacoesCodigo => Set<AvaliacaoCodigo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração da entidade Usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(300);
            entity.Property(e => e.TurmaId).HasMaxLength(100);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.TurmaId);
        });

        // Configuração da entidade Sessao
        modelBuilder.Entity<Sessao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            
            entity.HasOne(e => e.Usuario)
                  .WithMany(u => u.Sessoes)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CriadoEm);
        });

        // Configuração da entidade Interacao
        modelBuilder.Entity<Interacao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessaoId).IsRequired();
            entity.Property(e => e.Tipo).IsRequired().HasMaxLength(50);
            entity.Property(e => e.FerramentaUsada).HasMaxLength(100);
            entity.Property(e => e.MensagemErro).HasMaxLength(1000);
            
            entity.HasOne(e => e.Sessao)
                  .WithMany(s => s.Interacoes)
                  .HasForeignKey(e => e.SessaoId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => e.SessaoId);
            entity.HasIndex(e => e.Tipo);
            entity.HasIndex(e => e.FerramentaUsada);
            entity.HasIndex(e => e.CriadoEm);
        });

        // Configuração da entidade AvaliacaoCodigo
        modelBuilder.Entity<AvaliacaoCodigo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Tema).IsRequired().HasMaxLength(200);
            entity.Property(e => e.NotaFinal).HasPrecision(5, 2);
            entity.Property(e => e.Feedback).HasMaxLength(2000);
            
            entity.HasOne(e => e.Usuario)
                  .WithMany(u => u.Avaliacoes)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Tema);
            entity.HasIndex(e => e.CriadoEm);
        });

        // Configurações adicionais
        ConfigurarIndicesCompostos(modelBuilder);
        ConfigurarDadosIniciais(modelBuilder);
    }

    private static void ConfigurarIndicesCompostos(ModelBuilder modelBuilder)
    {
        // Índices compostos para melhorar performance de consultas
        modelBuilder.Entity<Sessao>()
            .HasIndex(e => new { e.UserId, e.CriadoEm })
            .HasDatabaseName("IX_Sessoes_UserId_CriadoEm");

        modelBuilder.Entity<Interacao>()
            .HasIndex(e => new { e.SessaoId, e.Tipo, e.CriadoEm })
            .HasDatabaseName("IX_Interacoes_SessaoId_Tipo_CriadoEm");

        modelBuilder.Entity<AvaliacaoCodigo>()
            .HasIndex(e => new { e.UserId, e.Tema, e.CriadoEm })
            .HasDatabaseName("IX_AvaliacoesCodigo_UserId_Tema_CriadoEm");
    }

    private static void ConfigurarDadosIniciais(ModelBuilder modelBuilder)
    {
        // Dados iniciais para desenvolvimento/testes
        var usuarioDemo = new Usuario
        {
            Id = "demo-user-1",
            Nome = "Usuário Demo",
            Email = "demo@tutorcopiloto.com",
            TurmaId = "turma-demo",
            CriadoEm = DateTime.UtcNow,
            UltimoAcesso = DateTime.UtcNow
        };

        modelBuilder.Entity<Usuario>().HasData(usuarioDemo);

        var sessaoDemo = new Sessao
        {
            Id = "demo-session-1",
            UserId = usuarioDemo.Id,
            CriadoEm = DateTime.UtcNow,
            FinalizadoEm = DateTime.UtcNow.AddMinutes(30),
            DuracaoMinutos = 30
        };

        modelBuilder.Entity<Sessao>().HasData(sessaoDemo);

        var interacaoDemo = new Interacao
        {
            Id = "demo-interaction-1",
            SessaoId = sessaoDemo.Id,
            Tipo = "explain",
            FerramentaUsada = "code-analyzer",
            Sucesso = true,
            TempoExecucaoMs = 1500,
            CriadoEm = DateTime.UtcNow
        };

        modelBuilder.Entity<Interacao>().HasData(interacaoDemo);

        var avaliacaoDemo = new AvaliacaoCodigo
        {
            Id = "demo-assessment-1",
            UserId = usuarioDemo.Id,
            Tema = "JavaScript Básico",
            NotaFinal = 8.5,
            Feedback = "Bom entendimento dos conceitos fundamentais. Continue praticando loops e condicionais.",
            CriadoEm = DateTime.UtcNow
        };

        modelBuilder.Entity<AvaliacaoCodigo>().HasData(avaliacaoDemo);
    }

    /// <summary>
    /// Configura o contexto para desenvolvimento
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Configuração para desenvolvimento local se não configurado via DI
            optionsBuilder.UseNpgsql("Host=localhost;Database=tutordb;Username=tutor;Password=copiloto123");
        }

        // Configurações de desenvolvimento
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableDetailedErrors();
    }

    /// <summary>
    /// Aplica migrações automaticamente (apenas em desenvolvimento)
    /// </summary>
    public async Task EnsureDatabaseCreatedAsync()
    {
        try
        {
            await Database.EnsureCreatedAsync();
            
            // Em produção, usar migrações explícitas
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                await Database.MigrateAsync();
            }
        }
        catch (Exception ex)
        {
            // Log do erro - em produção, usar ILogger injetado
            Console.WriteLine($"Erro ao criar/migrar banco de dados: {ex.Message}");
            throw;
        }
    }
}
