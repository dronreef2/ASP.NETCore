using System.ComponentModel.DataAnnotations;

namespace TutorCopiloto.Models;

/// <summary>
/// Relatório de progresso individual do estudante
/// </summary>
public class RelatorioProgresso
{
    public string UserId { get; set; } = string.Empty;
    public DateTime PeriodoInicio { get; set; }
    public DateTime PeriodoFim { get; set; }
    public int TotalSessoes { get; set; }
    public int TempoTotalMinutos { get; set; }
    public Dictionary<string, int> FerramentasUsadas { get; set; } = new();
    public List<ProgressoTema> ProgressoPorTema { get; set; } = new();
    public List<string> RecomendacoesMelhoria { get; set; } = new();
    public DateTime GeradoEm { get; set; }
}

/// <summary>
/// Progresso em um tema específico
/// </summary>
public class ProgressoTema
{
    public string Tema { get; set; } = string.Empty;
    public double NotaMedia { get; set; }
    public int TotalAvaliacoes { get; set; }
    public double Evolucao { get; set; } // Percentual de evolução
}

/// <summary>
/// Relatório de performance da turma
/// </summary>
public class RelatorioTurma
{
    public string TurmaId { get; set; } = string.Empty;
    public DateTime Periodo { get; set; }
    public EstatisticasTurma EstatisticasGerais { get; set; } = new();
    public List<DesempenhoEstudante> DesempenhoIndividual { get; set; } = new();
    public List<string> TemasMaisDesafiadores { get; set; } = new();
    public List<string> SugestoesMelhorias { get; set; } = new();
    public DateTime GeradoEm { get; set; }
}

/// <summary>
/// Estatísticas gerais da turma
/// </summary>
public class EstatisticasTurma
{
    public int TotalEstudantes { get; set; }
    public int EstudantesAtivos { get; set; }
    public double MediaSessoesPorEstudante { get; set; }
    public double TempoMedioSessaoMinutos { get; set; }
}

/// <summary>
/// Desempenho individual do estudante
/// </summary>
public class DesempenhoEstudante
{
    public string EstudanteId { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public int TotalSessoes { get; set; }
    public int TempoTotalMinutos { get; set; }
    public double NotaMediaGeral { get; set; }
    public List<string> TemasComMaiorDificuldade { get; set; } = new();
}

/// <summary>
/// Relatório de uso de ferramentas
/// </summary>
public class RelatorioFerramentas
{
    public DateTime PeriodoInicio { get; set; }
    public DateTime PeriodoFim { get; set; }
    public List<UsoFerramenta> UsoFerramentas { get; set; } = new();
    public string FerramentaMaisUsada { get; set; } = string.Empty;
    public double TaxaSucessoGeral { get; set; }
    public int TotalInteracoes { get; set; }
    public DateTime GeradoEm { get; set; }
}

/// <summary>
/// Estatísticas de uso de uma ferramenta
/// </summary>
public class UsoFerramenta
{
    public string Nome { get; set; } = string.Empty;
    public int TotalUsos { get; set; }
    public int UsuariosUnicos { get; set; }
    public double TaxaSucesso { get; set; }
    public double TempoMedioExecucaoMs { get; set; }
    public Dictionary<string, int> TiposErroComuns { get; set; } = new();
}

/// <summary>
/// Formatos de exportação suportados
/// </summary>
public enum FormatoExportacao
{
    PDF,
    Excel,
    CSV
}

/// <summary>
/// Resumo de relatório para listagem
/// </summary>
public class ResumoRelatorio
{
    public string Id { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public DateTime CriadoEm { get; set; }
    public StatusRelatorio Status { get; set; }
}

/// <summary>
/// Status do relatório
/// </summary>
public enum StatusRelatorio
{
    Pendente,
    Processando,
    Concluido,
    Erro
}

/// <summary>
/// Mensagem do chat
/// </summary>
public class ChatMessage
{
    public string Id { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public ChatMessageType Type { get; set; }
    public string? SessionId { get; set; }
    public string? TargetUserId { get; set; }
    public string? GroupId { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Tipos de mensagem do chat
/// </summary>
public enum ChatMessageType
{
    GeneralMessage,
    DirectMessage,
    GroupMessage,
    TutorInteraction,
    TutorResponse,
    SystemMessage
}

/// <summary>
/// Entidades do banco de dados
/// </summary>
public class Usuario
{
    public string Id { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? TurmaId { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime UltimoAcesso { get; set; }
    
    // Navegação
    public List<Sessao> Sessoes { get; set; } = new();
    public List<AvaliacaoCodigo> Avaliacoes { get; set; } = new();
}

public class Sessao
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
    public DateTime? FinalizadoEm { get; set; }
    public int? DuracaoMinutos { get; set; }
    
    // Navegação
    public Usuario Usuario { get; set; } = null!;
    public List<Interacao> Interacoes { get; set; } = new();
}

public class Interacao
{
    public string Id { get; set; } = string.Empty;
    public string SessaoId { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty; // explain, test, fix, assess
    public string? FerramentaUsada { get; set; }
    public bool Sucesso { get; set; }
    public string? MensagemErro { get; set; }
    public int? TempoExecucaoMs { get; set; }
    public DateTime CriadoEm { get; set; }
    
    // Navegação
    public Sessao Sessao { get; set; } = null!;
}

public class AvaliacaoCodigo
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Tema { get; set; } = string.Empty;
    public double NotaFinal { get; set; }
    public string? Feedback { get; set; }
    public DateTime CriadoEm { get; set; }
    
    // Navegação
    public Usuario Usuario { get; set; } = null!;
}

/// <summary>
/// Trilha de aprendizado personalizada para estudantes
/// </summary>
public class LearningPath
{
    public string Id { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public LearningPathCategory Categoria { get; set; }
    public LearningPathDifficulty Dificuldade { get; set; }
    public int OrdemSequencia { get; set; }
    public int DuracaoEstimadaHoras { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }
    
    // Navegação
    public List<LearningModule> Modulos { get; set; } = new();
    public List<StudentProgress> ProgressosEstudantes { get; set; } = new();
}

/// <summary>
/// Módulo individual dentro de uma trilha de aprendizado
/// </summary>
public class LearningModule
{
    public string Id { get; set; } = string.Empty;
    public string LearningPathId { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string ConteudoMarkdown { get; set; } = string.Empty;
    public int Ordem { get; set; }
    public bool ObrigatorioParaProgresso { get; set; } = true;
    public int TempoEstimadoMinutos { get; set; }
    public string? RecursosAdicionais { get; set; } // URLs, links externos
    public DateTime CriadoEm { get; set; }
    
    // Navegação
    public LearningPath LearningPath { get; set; } = null!;
    public List<ModuleCompletion> Completoes { get; set; } = new();
}

/// <summary>
/// Progresso do estudante em uma trilha de aprendizado
/// </summary>
public class StudentProgress
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string LearningPathId { get; set; } = string.Empty;
    public DateTime IniciadoEm { get; set; }
    public DateTime? ConcluidoEm { get; set; }
    public LearningProgressStatus Status { get; set; } = LearningProgressStatus.NotStarted;
    public double ProgressoPercentual { get; set; } = 0.0;
    public int TempoGastoMinutos { get; set; } = 0;
    public DateTime UltimaAtividadeEm { get; set; }
    
    // Navegação
    public Usuario Usuario { get; set; } = null!;
    public LearningPath LearningPath { get; set; } = null!;
    public List<ModuleCompletion> ModulosCompletados { get; set; } = new();
}

/// <summary>
/// Conclusão de um módulo por um estudante
/// </summary>
public class ModuleCompletion
{
    public string Id { get; set; } = string.Empty;
    public string StudentProgressId { get; set; } = string.Empty;
    public string LearningModuleId { get; set; } = string.Empty;
    public DateTime ConcluidoEm { get; set; }
    public int TempoGastoMinutos { get; set; }
    public double? NotaAvaliacao { get; set; }
    public string? FeedbackEstudante { get; set; }
    
    // Navegação
    public StudentProgress StudentProgress { get; set; } = null!;
    public LearningModule LearningModule { get; set; } = null!;
}

/// <summary>
/// Categorias de trilhas de aprendizado
/// </summary>
public enum LearningPathCategory
{
    Frontend,
    Backend,
    FullStack,
    DevOps,
    DataScience,
    MachineLearning,
    MobileDevelopment,
    GameDevelopment,
    Cybersecurity,
    SoftwareArchitecture
}

/// <summary>
/// Níveis de dificuldade das trilhas
/// </summary>
public enum LearningPathDifficulty
{
    Beginner,
    Intermediate,
    Advanced,
    Expert
}

/// <summary>
/// Status do progresso do estudante
/// </summary>
public enum LearningProgressStatus
{
    NotStarted,
    InProgress,
    Completed,
    Paused,
    Abandoned
}
