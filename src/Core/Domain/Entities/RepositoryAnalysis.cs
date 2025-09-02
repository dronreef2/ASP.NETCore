using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TutorCopiloto.Domain.Entities
{
    /// <summary>
    /// Representa um repositório GitHub analisado
    /// </summary>
    public class Repository
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Url { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Owner { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Language { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        public int Stars { get; set; }
        public int Forks { get; set; }
        public int OpenIssues { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime LastAnalyzedAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Relacionamentos
        public virtual ICollection<AnalysisReport> AnalysisReports { get; set; } = new List<AnalysisReport>();
    }

    /// <summary>
    /// Relatório de análise de um repositório
    /// </summary>
    public class AnalysisReport
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RepositoryId { get; set; }

        [ForeignKey("RepositoryId")]
        public virtual Repository Repository { get; set; } = null!;

        public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;

        // Métricas gerais
        public int TotalLinesOfCode { get; set; }
        public int FilesCount { get; set; }
        public decimal QualityScore { get; set; } // 0-100

        // Conformidade Debian
        public bool HasDebianPackaging { get; set; }
        public int LintianErrors { get; set; }
        public int LintianWarnings { get; set; }
        public int LintianInfo { get; set; }

        // Segurança
        public int SecurityIssues { get; set; }
        public int CriticalSecurityIssues { get; set; }

        // Testes e CI/CD
        public bool HasTests { get; set; }
        public bool HasCI { get; set; }
        public decimal TestCoverage { get; set; }

        // Documentação
        public bool HasReadme { get; set; }
        public bool HasDocumentation { get; set; }

        // Status da análise
        public AnalysisStatus Status { get; set; } = AnalysisStatus.Pending;

        [MaxLength(2000)]
        public string ErrorMessage { get; set; } = string.Empty;

        // Relacionamentos
        public virtual ICollection<LintianFinding> LintianFindings { get; set; } = new List<LintianFinding>();
        public virtual ICollection<BugReport> BugReports { get; set; } = new List<BugReport>();
        public virtual ICollection<CodeMetric> CodeMetrics { get; set; } = new List<CodeMetric>();
    }

    /// <summary>
    /// Finding específico do Lintian
    /// </summary>
    public class LintianFinding
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AnalysisReportId { get; set; }

        [ForeignKey("AnalysisReportId")]
        public virtual AnalysisReport AnalysisReport { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Severity { get; set; } = string.Empty; // error, warning, info

        [Required]
        [MaxLength(100)]
        public string Tag { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        public int LineNumber { get; set; }
    }

    /// <summary>
    /// Relatório de bug encontrado
    /// </summary>
    public class BugReport
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AnalysisReportId { get; set; }

        [ForeignKey("AnalysisReportId")]
        public virtual AnalysisReport AnalysisReport { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Severity { get; set; } = string.Empty; // critical, high, medium, low

        [MaxLength(100)]
        public string Category { get; set; } = string.Empty; // security, quality, packaging, etc.

        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        public int LineNumber { get; set; }

        public bool IsFixed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Métricas de código por linguagem
    /// </summary>
    public class CodeMetric
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AnalysisReportId { get; set; }

        [ForeignKey("AnalysisReportId")]
        public virtual AnalysisReport AnalysisReport { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Language { get; set; } = string.Empty;

        public int Files { get; set; }
        public int Lines { get; set; }
        public int CodeLines { get; set; }
        public int CommentLines { get; set; }
        public int BlankLines { get; set; }
        public int Complexity { get; set; }
    }

    /// <summary>
    /// Status da análise
    /// </summary>
    public enum AnalysisStatus
    {
        Pending,
        InProgress,
        Completed,
        Failed,
        Cancelled
    }
}
