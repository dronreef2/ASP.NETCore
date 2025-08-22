using Microsoft.EntityFrameworkCore;
using TutorCopiloto.Models;

namespace TutorCopiloto.Data;

/// <summary>
/// Contexto de banco de dados para o Tutor Copiloto
/// </summary>
public class TutorDbContext : DbContext
{
    public TutorDbContext(DbContextOptions<TutorDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Sessao> Sessoes => Set<Sessao>();
    public DbSet<Interacao> Interacoes => Set<Interacao>();
    public DbSet<AvaliacaoCodigo> AvaliacoesCodigo => Set<AvaliacaoCodigo>();
    
    // Learning Path Management
    public DbSet<LearningPath> LearningPaths => Set<LearningPath>();
    public DbSet<LearningModule> LearningModules => Set<LearningModule>();
    public DbSet<StudentProgress> StudentProgresses => Set<StudentProgress>();
    public DbSet<ModuleCompletion> ModuleCompletions => Set<ModuleCompletion>();

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

        // Configuração da entidade LearningPath
        modelBuilder.Entity<LearningPath>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(300);
            entity.Property(e => e.Descricao).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Categoria).HasConversion<string>();
            entity.Property(e => e.Dificuldade).HasConversion<string>();
            entity.Property(e => e.OrdemSequencia).IsRequired();
            entity.Property(e => e.DuracaoEstimadaHoras).IsRequired();
            
            entity.HasIndex(e => e.Categoria);
            entity.HasIndex(e => e.Dificuldade);
            entity.HasIndex(e => e.OrdemSequencia);
            entity.HasIndex(e => e.Ativo);
        });

        // Configuração da entidade LearningModule
        modelBuilder.Entity<LearningModule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LearningPathId).IsRequired();
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(300);
            entity.Property(e => e.Descricao).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.ConteudoMarkdown).IsRequired();
            entity.Property(e => e.Ordem).IsRequired();
            entity.Property(e => e.TempoEstimadoMinutos).IsRequired();
            entity.Property(e => e.RecursosAdicionais).HasMaxLength(2000);
            
            entity.HasOne(e => e.LearningPath)
                  .WithMany(lp => lp.Modulos)
                  .HasForeignKey(e => e.LearningPathId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => e.LearningPathId);
            entity.HasIndex(e => e.Ordem);
        });

        // Configuração da entidade StudentProgress
        modelBuilder.Entity<StudentProgress>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.LearningPathId).IsRequired();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.ProgressoPercentual).HasPrecision(5, 2);
            
            entity.HasOne(e => e.Usuario)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.LearningPath)
                  .WithMany(lp => lp.ProgressosEstudantes)
                  .HasForeignKey(e => e.LearningPathId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.LearningPathId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.UserId, e.LearningPathId }).IsUnique();
        });

        // Configuração da entidade ModuleCompletion
        modelBuilder.Entity<ModuleCompletion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StudentProgressId).IsRequired();
            entity.Property(e => e.LearningModuleId).IsRequired();
            entity.Property(e => e.TempoGastoMinutos).IsRequired();
            entity.Property(e => e.NotaAvaliacao).HasPrecision(5, 2);
            entity.Property(e => e.FeedbackEstudante).HasMaxLength(1000);
            
            entity.HasOne(e => e.StudentProgress)
                  .WithMany(sp => sp.ModulosCompletados)
                  .HasForeignKey(e => e.StudentProgressId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.LearningModule)
                  .WithMany(lm => lm.Completoes)
                  .HasForeignKey(e => e.LearningModuleId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => e.StudentProgressId);
            entity.HasIndex(e => e.LearningModuleId);
            entity.HasIndex(e => e.ConcluidoEm);
            entity.HasIndex(e => new { e.StudentProgressId, e.LearningModuleId }).IsUnique();
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

        // Novos índices para Learning Path Management
        modelBuilder.Entity<LearningPath>()
            .HasIndex(e => new { e.Categoria, e.Dificuldade, e.Ativo })
            .HasDatabaseName("IX_LearningPaths_Categoria_Dificuldade_Ativo");

        modelBuilder.Entity<LearningModule>()
            .HasIndex(e => new { e.LearningPathId, e.Ordem })
            .HasDatabaseName("IX_LearningModules_LearningPathId_Ordem");

        modelBuilder.Entity<StudentProgress>()
            .HasIndex(e => new { e.UserId, e.Status, e.UltimaAtividadeEm })
            .HasDatabaseName("IX_StudentProgress_UserId_Status_UltimaAtividade");

        modelBuilder.Entity<ModuleCompletion>()
            .HasIndex(e => new { e.StudentProgressId, e.ConcluidoEm })
            .HasDatabaseName("IX_ModuleCompletion_StudentProgressId_ConcluidoEm");
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

        // Dados de exemplo para Learning Paths
        var learningPathJavascript = new LearningPath
        {
            Id = "lp-javascript-basics",
            Nome = "JavaScript Fundamentals",
            Descricao = "Aprenda os conceitos fundamentais de JavaScript do zero",
            Categoria = LearningPathCategory.Frontend,
            Dificuldade = LearningPathDifficulty.Beginner,
            OrdemSequencia = 1,
            DuracaoEstimadaHoras = 20,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        var learningPathReact = new LearningPath
        {
            Id = "lp-react-fundamentals",
            Nome = "React Fundamentals",
            Descricao = "Domine os conceitos básicos do React para desenvolvimento frontend",
            Categoria = LearningPathCategory.Frontend,
            Dificuldade = LearningPathDifficulty.Intermediate,
            OrdemSequencia = 2,
            DuracaoEstimadaHoras = 35,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        modelBuilder.Entity<LearningPath>().HasData(learningPathJavascript, learningPathReact);

        // Módulos para JavaScript Basics
        var moduleJSVariables = new LearningModule
        {
            Id = "mod-js-variables",
            LearningPathId = learningPathJavascript.Id,
            Nome = "Variáveis e Tipos de Dados",
            Descricao = "Entenda como declarar e usar variáveis em JavaScript",
            ConteudoMarkdown = "# Variáveis em JavaScript\n\nVariáveis são containers para armazenar dados...",
            Ordem = 1,
            ObrigatorioParaProgresso = true,
            TempoEstimadoMinutos = 45,
            RecursosAdicionais = "https://developer.mozilla.org/pt-BR/docs/Web/JavaScript/Guide/Grammar_and_types",
            CriadoEm = DateTime.UtcNow
        };

        var moduleJSFunctions = new LearningModule
        {
            Id = "mod-js-functions",
            LearningPathId = learningPathJavascript.Id,
            Nome = "Funções",
            Descricao = "Aprenda a criar e usar funções em JavaScript",
            ConteudoMarkdown = "# Funções em JavaScript\n\nFunções são blocos de código reutilizáveis...",
            Ordem = 2,
            ObrigatorioParaProgresso = true,
            TempoEstimadoMinutos = 60,
            RecursosAdicionais = "https://developer.mozilla.org/pt-BR/docs/Web/JavaScript/Guide/Functions",
            CriadoEm = DateTime.UtcNow
        };

        // Módulos para React Fundamentals
        var moduleReactIntro = new LearningModule
        {
            Id = "mod-react-intro",
            LearningPathId = learningPathReact.Id,
            Nome = "Introdução ao React",
            Descricao = "Conceitos básicos e configuração do ambiente React",
            ConteudoMarkdown = "# Introdução ao React\n\nReact é uma biblioteca JavaScript para construir interfaces de usuário...",
            Ordem = 1,
            ObrigatorioParaProgresso = true,
            TempoEstimadoMinutos = 90,
            RecursosAdicionais = "https://react.dev/learn",
            CriadoEm = DateTime.UtcNow
        };

        modelBuilder.Entity<LearningModule>().HasData(
            moduleJSVariables, moduleJSFunctions, moduleReactIntro
        );

        // Progresso do usuário demo
        var progressDemo = new StudentProgress
        {
            Id = "progress-demo-1",
            UserId = usuarioDemo.Id,
            LearningPathId = learningPathJavascript.Id,
            IniciadoEm = DateTime.UtcNow.AddDays(-5),
            Status = LearningProgressStatus.InProgress,
            ProgressoPercentual = 50.0,
            TempoGastoMinutos = 45,
            UltimaAtividadeEm = DateTime.UtcNow.AddHours(-2)
        };

        modelBuilder.Entity<StudentProgress>().HasData(progressDemo);

        // Conclusão de módulo
        var completionDemo = new ModuleCompletion
        {
            Id = "completion-demo-1",
            StudentProgressId = progressDemo.Id,
            LearningModuleId = moduleJSVariables.Id,
            ConcluidoEm = DateTime.UtcNow.AddDays(-3),
            TempoGastoMinutos = 45,
            NotaAvaliacao = 8.5,
            FeedbackEstudante = "Conteúdo muito claro e bem explicado!"
        };

        modelBuilder.Entity<ModuleCompletion>().HasData(completionDemo);
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
