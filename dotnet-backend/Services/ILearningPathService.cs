using TutorCopiloto.Models;

namespace TutorCopiloto.Services;

/// <summary>
/// Interface para serviços de gerenciamento de trilhas de aprendizado
/// </summary>
public interface ILearningPathService
{
    // Learning Path Operations
    Task<List<LearningPath>> GetAllLearningPathsAsync(bool includeInactive = false);
    Task<List<LearningPath>> GetLearningPathsByCategoryAsync(LearningPathCategory category);
    Task<List<LearningPath>> GetLearningPathsByDifficultyAsync(LearningPathDifficulty difficulty);
    Task<LearningPath?> GetLearningPathByIdAsync(string id, bool includeModules = true);
    Task<LearningPath> CreateLearningPathAsync(LearningPath learningPath);
    Task<LearningPath?> UpdateLearningPathAsync(string id, LearningPath learningPath);
    Task<bool> DeleteLearningPathAsync(string id);
    Task<bool> ToggleLearningPathStatusAsync(string id);

    // Learning Module Operations
    Task<List<LearningModule>> GetModulesByPathIdAsync(string learningPathId);
    Task<LearningModule?> GetModuleByIdAsync(string moduleId);
    Task<LearningModule> CreateModuleAsync(LearningModule module);
    Task<LearningModule?> UpdateModuleAsync(string id, LearningModule module);
    Task<bool> DeleteModuleAsync(string id);
    Task<bool> ReorderModulesAsync(string learningPathId, List<string> moduleIds);

    // Student Progress Operations
    Task<StudentProgress?> GetStudentProgressAsync(string userId, string learningPathId);
    Task<List<StudentProgress>> GetStudentProgressByUserIdAsync(string userId);
    Task<List<StudentProgress>> GetStudentProgressByPathIdAsync(string learningPathId);
    Task<StudentProgress> StartLearningPathAsync(string userId, string learningPathId);
    Task<StudentProgress?> UpdateProgressAsync(string userId, string learningPathId, double progressPercent);
    Task<StudentProgress?> CompleteLearningPathAsync(string userId, string learningPathId);
    Task<bool> PauseLearningPathAsync(string userId, string learningPathId);
    Task<bool> AbandonLearningPathAsync(string userId, string learningPathId);

    // Module Completion Operations
    Task<ModuleCompletion?> CompleteModuleAsync(string userId, string learningPathId, string moduleId, int timeSpentMinutes, double? grade = null, string? feedback = null);
    Task<List<ModuleCompletion>> GetCompletedModulesAsync(string userId, string learningPathId);
    Task<bool> IsModuleCompletedAsync(string userId, string moduleId);

    // Analytics and Recommendations
    Task<LearningAnalytics> GetLearningAnalyticsAsync(string userId);
    Task<List<LearningPath>> GetRecommendedPathsAsync(string userId);
    Task<LearningPathStatistics> GetPathStatisticsAsync(string learningPathId);
    Task<List<StudentPerformance>> GetClassPerformanceAsync(string classId);
}

/// <summary>
/// Analytics de aprendizado para um estudante
/// </summary>
public class LearningAnalytics
{
    public string UserId { get; set; } = string.Empty;
    public int TotalPathsStarted { get; set; }
    public int TotalPathsCompleted { get; set; }
    public int TotalModulesCompleted { get; set; }
    public int TotalTimeSpentMinutes { get; set; }
    public double AverageGrade { get; set; }
    public LearningPathCategory? FavoriteCategory { get; set; }
    public List<string> CompletedPaths { get; set; } = new();
    public List<string> InProgressPaths { get; set; } = new();
    public DateTime LastActivity { get; set; }
}

/// <summary>
/// Estatísticas de uma trilha de aprendizado
/// </summary>
public class LearningPathStatistics
{
    public string LearningPathId { get; set; } = string.Empty;
    public int TotalStudentsEnrolled { get; set; }
    public int TotalStudentsCompleted { get; set; }
    public double CompletionRate { get; set; }
    public double AverageCompletionTimeHours { get; set; }
    public double AverageGrade { get; set; }
    public List<ModuleStatistics> ModuleStats { get; set; } = new();
}

/// <summary>
/// Estatísticas de um módulo
/// </summary>
public class ModuleStatistics
{
    public string ModuleId { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public int CompletionCount { get; set; }
    public double AverageGrade { get; set; }
    public double AverageTimeMinutes { get; set; }
    public double DropoffRate { get; set; }
}

/// <summary>
/// Performance de estudante para análise de turma
/// </summary>
public class StudentPerformance
{
    public string UserId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public int PathsStarted { get; set; }
    public int PathsCompleted { get; set; }
    public double CompletionRate { get; set; }
    public double AverageGrade { get; set; }
    public int TotalTimeSpentMinutes { get; set; }
    public DateTime LastActivity { get; set; }
    public List<string> StrugglingTopics { get; set; } = new();
}