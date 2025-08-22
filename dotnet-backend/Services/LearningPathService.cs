using Microsoft.EntityFrameworkCore;
using TutorCopiloto.Data;
using TutorCopiloto.Models;

namespace TutorCopiloto.Services;

/// <summary>
/// Implementação do serviço de gerenciamento de trilhas de aprendizado
/// </summary>
public class LearningPathService : ILearningPathService
{
    private readonly TutorDbContext _context;
    private readonly ILogger<LearningPathService> _logger;

    public LearningPathService(TutorDbContext context, ILogger<LearningPathService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Learning Path Operations

    public async Task<List<LearningPath>> GetAllLearningPathsAsync(bool includeInactive = false)
    {
        var query = _context.LearningPaths.AsQueryable();
        
        if (!includeInactive)
        {
            query = query.Where(lp => lp.Ativo);
        }

        return await query
            .Include(lp => lp.Modulos.OrderBy(m => m.Ordem))
            .OrderBy(lp => lp.OrdemSequencia)
            .ToListAsync();
    }

    public async Task<List<LearningPath>> GetLearningPathsByCategoryAsync(LearningPathCategory category)
    {
        return await _context.LearningPaths
            .Where(lp => lp.Categoria == category && lp.Ativo)
            .Include(lp => lp.Modulos.OrderBy(m => m.Ordem))
            .OrderBy(lp => lp.OrdemSequencia)
            .ToListAsync();
    }

    public async Task<List<LearningPath>> GetLearningPathsByDifficultyAsync(LearningPathDifficulty difficulty)
    {
        return await _context.LearningPaths
            .Where(lp => lp.Dificuldade == difficulty && lp.Ativo)
            .Include(lp => lp.Modulos.OrderBy(m => m.Ordem))
            .OrderBy(lp => lp.OrdemSequencia)
            .ToListAsync();
    }

    public async Task<LearningPath?> GetLearningPathByIdAsync(string id, bool includeModules = true)
    {
        var query = _context.LearningPaths.AsQueryable();
        
        if (includeModules)
        {
            query = query.Include(lp => lp.Modulos.OrderBy(m => m.Ordem));
        }

        return await query.FirstOrDefaultAsync(lp => lp.Id == id);
    }

    public async Task<LearningPath> CreateLearningPathAsync(LearningPath learningPath)
    {
        learningPath.Id = Guid.NewGuid().ToString();
        learningPath.CriadoEm = DateTime.UtcNow;
        
        _context.LearningPaths.Add(learningPath);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Created learning path {PathId}: {PathName}", learningPath.Id, learningPath.Nome);
        return learningPath;
    }

    public async Task<LearningPath?> UpdateLearningPathAsync(string id, LearningPath learningPath)
    {
        var existingPath = await _context.LearningPaths.FindAsync(id);
        if (existingPath == null) return null;

        existingPath.Nome = learningPath.Nome;
        existingPath.Descricao = learningPath.Descricao;
        existingPath.Categoria = learningPath.Categoria;
        existingPath.Dificuldade = learningPath.Dificuldade;
        existingPath.OrdemSequencia = learningPath.OrdemSequencia;
        existingPath.DuracaoEstimadaHoras = learningPath.DuracaoEstimadaHoras;
        existingPath.Ativo = learningPath.Ativo;
        existingPath.AtualizadoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Updated learning path {PathId}: {PathName}", id, learningPath.Nome);
        return existingPath;
    }

    public async Task<bool> DeleteLearningPathAsync(string id)
    {
        var learningPath = await _context.LearningPaths.FindAsync(id);
        if (learningPath == null) return false;

        _context.LearningPaths.Remove(learningPath);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Deleted learning path {PathId}", id);
        return true;
    }

    public async Task<bool> ToggleLearningPathStatusAsync(string id)
    {
        var learningPath = await _context.LearningPaths.FindAsync(id);
        if (learningPath == null) return false;

        learningPath.Ativo = !learningPath.Ativo;
        learningPath.AtualizadoEm = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Toggled learning path {PathId} status to {Status}", id, learningPath.Ativo);
        return true;
    }

    #endregion

    #region Learning Module Operations

    public async Task<List<LearningModule>> GetModulesByPathIdAsync(string learningPathId)
    {
        return await _context.LearningModules
            .Where(m => m.LearningPathId == learningPathId)
            .OrderBy(m => m.Ordem)
            .ToListAsync();
    }

    public async Task<LearningModule?> GetModuleByIdAsync(string moduleId)
    {
        return await _context.LearningModules
            .Include(m => m.LearningPath)
            .FirstOrDefaultAsync(m => m.Id == moduleId);
    }

    public async Task<LearningModule> CreateModuleAsync(LearningModule module)
    {
        module.Id = Guid.NewGuid().ToString();
        module.CriadoEm = DateTime.UtcNow;
        
        _context.LearningModules.Add(module);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Created learning module {ModuleId}: {ModuleName}", module.Id, module.Nome);
        return module;
    }

    public async Task<LearningModule?> UpdateModuleAsync(string id, LearningModule module)
    {
        var existingModule = await _context.LearningModules.FindAsync(id);
        if (existingModule == null) return null;

        existingModule.Nome = module.Nome;
        existingModule.Descricao = module.Descricao;
        existingModule.ConteudoMarkdown = module.ConteudoMarkdown;
        existingModule.Ordem = module.Ordem;
        existingModule.ObrigatorioParaProgresso = module.ObrigatorioParaProgresso;
        existingModule.TempoEstimadoMinutos = module.TempoEstimadoMinutos;
        existingModule.RecursosAdicionais = module.RecursosAdicionais;

        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Updated learning module {ModuleId}: {ModuleName}", id, module.Nome);
        return existingModule;
    }

    public async Task<bool> DeleteModuleAsync(string id)
    {
        var module = await _context.LearningModules.FindAsync(id);
        if (module == null) return false;

        _context.LearningModules.Remove(module);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Deleted learning module {ModuleId}", id);
        return true;
    }

    public async Task<bool> ReorderModulesAsync(string learningPathId, List<string> moduleIds)
    {
        var modules = await _context.LearningModules
            .Where(m => m.LearningPathId == learningPathId && moduleIds.Contains(m.Id))
            .ToListAsync();

        for (int i = 0; i < moduleIds.Count; i++)
        {
            var module = modules.FirstOrDefault(m => m.Id == moduleIds[i]);
            if (module != null)
            {
                module.Ordem = i + 1;
            }
        }

        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Reordered modules for learning path {PathId}", learningPathId);
        return true;
    }

    #endregion

    #region Student Progress Operations

    public async Task<StudentProgress?> GetStudentProgressAsync(string userId, string learningPathId)
    {
        return await _context.StudentProgresses
            .Include(sp => sp.LearningPath)
            .Include(sp => sp.ModulosCompletados)
                .ThenInclude(mc => mc.LearningModule)
            .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.LearningPathId == learningPathId);
    }

    public async Task<List<StudentProgress>> GetStudentProgressByUserIdAsync(string userId)
    {
        return await _context.StudentProgresses
            .Include(sp => sp.LearningPath)
            .Include(sp => sp.ModulosCompletados)
            .Where(sp => sp.UserId == userId)
            .OrderByDescending(sp => sp.UltimaAtividadeEm)
            .ToListAsync();
    }

    public async Task<List<StudentProgress>> GetStudentProgressByPathIdAsync(string learningPathId)
    {
        return await _context.StudentProgresses
            .Include(sp => sp.Usuario)
            .Include(sp => sp.ModulosCompletados)
            .Where(sp => sp.LearningPathId == learningPathId)
            .OrderByDescending(sp => sp.UltimaAtividadeEm)
            .ToListAsync();
    }

    public async Task<StudentProgress> StartLearningPathAsync(string userId, string learningPathId)
    {
        var existingProgress = await GetStudentProgressAsync(userId, learningPathId);
        if (existingProgress != null)
        {
            return existingProgress;
        }

        var progress = new StudentProgress
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            LearningPathId = learningPathId,
            IniciadoEm = DateTime.UtcNow,
            Status = LearningProgressStatus.InProgress,
            ProgressoPercentual = 0.0,
            TempoGastoMinutos = 0,
            UltimaAtividadeEm = DateTime.UtcNow
        };

        _context.StudentProgresses.Add(progress);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Started learning path {PathId} for user {UserId}", learningPathId, userId);
        return progress;
    }

    public async Task<StudentProgress?> UpdateProgressAsync(string userId, string learningPathId, double progressPercent)
    {
        var progress = await GetStudentProgressAsync(userId, learningPathId);
        if (progress == null) return null;

        progress.ProgressoPercentual = progressPercent;
        progress.UltimaAtividadeEm = DateTime.UtcNow;

        if (progressPercent >= 100.0 && progress.Status != LearningProgressStatus.Completed)
        {
            progress.Status = LearningProgressStatus.Completed;
            progress.ConcluidoEm = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return progress;
    }

    public async Task<StudentProgress?> CompleteLearningPathAsync(string userId, string learningPathId)
    {
        var progress = await GetStudentProgressAsync(userId, learningPathId);
        if (progress == null) return null;

        progress.Status = LearningProgressStatus.Completed;
        progress.ConcluidoEm = DateTime.UtcNow;
        progress.ProgressoPercentual = 100.0;
        progress.UltimaAtividadeEm = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Completed learning path {PathId} for user {UserId}", learningPathId, userId);
        return progress;
    }

    public async Task<bool> PauseLearningPathAsync(string userId, string learningPathId)
    {
        var progress = await GetStudentProgressAsync(userId, learningPathId);
        if (progress == null) return false;

        progress.Status = LearningProgressStatus.Paused;
        progress.UltimaAtividadeEm = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AbandonLearningPathAsync(string userId, string learningPathId)
    {
        var progress = await GetStudentProgressAsync(userId, learningPathId);
        if (progress == null) return false;

        progress.Status = LearningProgressStatus.Abandoned;
        progress.UltimaAtividadeEm = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Module Completion Operations

    public async Task<ModuleCompletion?> CompleteModuleAsync(string userId, string learningPathId, string moduleId, int timeSpentMinutes, double? grade = null, string? feedback = null)
    {
        var progress = await GetStudentProgressAsync(userId, learningPathId);
        if (progress == null)
        {
            progress = await StartLearningPathAsync(userId, learningPathId);
        }

        var existingCompletion = await _context.ModuleCompletions
            .FirstOrDefaultAsync(mc => mc.StudentProgressId == progress.Id && mc.LearningModuleId == moduleId);

        if (existingCompletion != null)
        {
            return existingCompletion;
        }

        var completion = new ModuleCompletion
        {
            Id = Guid.NewGuid().ToString(),
            StudentProgressId = progress.Id,
            LearningModuleId = moduleId,
            ConcluidoEm = DateTime.UtcNow,
            TempoGastoMinutos = timeSpentMinutes,
            NotaAvaliacao = grade,
            FeedbackEstudante = feedback
        };

        _context.ModuleCompletions.Add(completion);

        // Update total time and progress
        progress.TempoGastoMinutos += timeSpentMinutes;
        progress.UltimaAtividadeEm = DateTime.UtcNow;

        // Calculate new progress percentage
        var totalModules = await _context.LearningModules
            .CountAsync(m => m.LearningPathId == learningPathId && m.ObrigatorioParaProgresso);
        
        var completedModules = await _context.ModuleCompletions
            .Join(_context.LearningModules, 
                  mc => mc.LearningModuleId, 
                  m => m.Id, 
                  (mc, m) => new { mc, m })
            .Where(x => x.mc.StudentProgressId == progress.Id && x.m.ObrigatorioParaProgresso)
            .CountAsync() + 1; // +1 for the module we're completing now

        progress.ProgressoPercentual = totalModules > 0 ? (double)completedModules / totalModules * 100.0 : 0.0;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Completed module {ModuleId} for user {UserId} in path {PathId}", moduleId, userId, learningPathId);
        return completion;
    }

    public async Task<List<ModuleCompletion>> GetCompletedModulesAsync(string userId, string learningPathId)
    {
        return await _context.ModuleCompletions
            .Include(mc => mc.LearningModule)
            .Include(mc => mc.StudentProgress)
            .Where(mc => mc.StudentProgress.UserId == userId && mc.StudentProgress.LearningPathId == learningPathId)
            .OrderBy(mc => mc.ConcluidoEm)
            .ToListAsync();
    }

    public async Task<bool> IsModuleCompletedAsync(string userId, string moduleId)
    {
        return await _context.ModuleCompletions
            .Include(mc => mc.StudentProgress)
            .AnyAsync(mc => mc.StudentProgress.UserId == userId && mc.LearningModuleId == moduleId);
    }

    #endregion

    #region Analytics and Recommendations

    public async Task<LearningAnalytics> GetLearningAnalyticsAsync(string userId)
    {
        var progresses = await GetStudentProgressByUserIdAsync(userId);
        
        var completedPaths = progresses.Where(p => p.Status == LearningProgressStatus.Completed).ToList();
        var inProgressPaths = progresses.Where(p => p.Status == LearningProgressStatus.InProgress).ToList();
        
        var totalModulesCompleted = await _context.ModuleCompletions
            .Include(mc => mc.StudentProgress)
            .Where(mc => mc.StudentProgress.UserId == userId)
            .CountAsync();

        var averageGrade = await _context.ModuleCompletions
            .Include(mc => mc.StudentProgress)
            .Where(mc => mc.StudentProgress.UserId == userId && mc.NotaAvaliacao.HasValue)
            .Select(mc => mc.NotaAvaliacao!.Value)
            .DefaultIfEmpty(0.0)
            .AverageAsync();

        var favoriteCategory = progresses
            .GroupBy(p => p.LearningPath.Categoria)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault()?.Key;

        return new LearningAnalytics
        {
            UserId = userId,
            TotalPathsStarted = progresses.Count,
            TotalPathsCompleted = completedPaths.Count,
            TotalModulesCompleted = totalModulesCompleted,
            TotalTimeSpentMinutes = progresses.Sum(p => p.TempoGastoMinutos),
            AverageGrade = averageGrade,
            FavoriteCategory = favoriteCategory,
            CompletedPaths = completedPaths.Select(p => p.LearningPathId).ToList(),
            InProgressPaths = inProgressPaths.Select(p => p.LearningPathId).ToList(),
            LastActivity = progresses.Max(p => p.UltimaAtividadeEm)
        };
    }

    public async Task<List<LearningPath>> GetRecommendedPathsAsync(string userId)
    {
        var userProgress = await GetStudentProgressByUserIdAsync(userId);
        var completedCategories = userProgress
            .Where(p => p.Status == LearningProgressStatus.Completed)
            .Select(p => p.LearningPath.Categoria)
            .Distinct()
            .ToList();

        // Recommend paths from same categories or next difficulty level
        var recommendations = await _context.LearningPaths
            .Where(lp => lp.Ativo && !userProgress.Select(up => up.LearningPathId).Contains(lp.Id))
            .Where(lp => completedCategories.Contains(lp.Categoria) || 
                        completedCategories.Any() && lp.Dificuldade == LearningPathDifficulty.Intermediate)
            .OrderBy(lp => lp.Dificuldade)
            .ThenBy(lp => lp.OrdemSequencia)
            .Take(5)
            .ToListAsync();

        return recommendations;
    }

    public async Task<LearningPathStatistics> GetPathStatisticsAsync(string learningPathId)
    {
        var progresses = await GetStudentProgressByPathIdAsync(learningPathId);
        var completedProgresses = progresses.Where(p => p.Status == LearningProgressStatus.Completed).ToList();

        var moduleStats = await _context.LearningModules
            .Where(m => m.LearningPathId == learningPathId)
            .Select(m => new ModuleStatistics
            {
                ModuleId = m.Id,
                ModuleName = m.Nome,
                CompletionCount = m.Completoes.Count,
                AverageGrade = m.Completoes.Where(c => c.NotaAvaliacao.HasValue).Select(c => c.NotaAvaliacao!.Value).DefaultIfEmpty(0.0).Average(),
                AverageTimeMinutes = m.Completoes.Select(c => (double)c.TempoGastoMinutos).DefaultIfEmpty(0.0).Average(),
                DropoffRate = 0.0 // Calculate based on module order and completions
            })
            .ToListAsync();

        return new LearningPathStatistics
        {
            LearningPathId = learningPathId,
            TotalStudentsEnrolled = progresses.Count,
            TotalStudentsCompleted = completedProgresses.Count,
            CompletionRate = progresses.Count > 0 ? (double)completedProgresses.Count / progresses.Count * 100.0 : 0.0,
            AverageCompletionTimeHours = completedProgresses.Count > 0 ? completedProgresses.Average(p => p.TempoGastoMinutos) / 60.0 : 0.0,
            AverageGrade = completedProgresses.Count > 0 ? completedProgresses.Average(p => p.ProgressoPercentual) : 0.0,
            ModuleStats = moduleStats
        };
    }

    public async Task<List<StudentPerformance>> GetClassPerformanceAsync(string classId)
    {
        var studentsInClass = await _context.Usuarios
            .Where(u => u.TurmaId == classId)
            .ToListAsync();

        var performances = new List<StudentPerformance>();

        foreach (var student in studentsInClass)
        {
            var progresses = await GetStudentProgressByUserIdAsync(student.Id);
            var completedPaths = progresses.Where(p => p.Status == LearningProgressStatus.Completed).ToList();

            var averageGrade = await _context.ModuleCompletions
                .Include(mc => mc.StudentProgress)
                .Where(mc => mc.StudentProgress.UserId == student.Id && mc.NotaAvaliacao.HasValue)
                .Select(mc => mc.NotaAvaliacao!.Value)
                .DefaultIfEmpty(0.0)
                .AverageAsync();

            performances.Add(new StudentPerformance
            {
                UserId = student.Id,
                StudentName = student.Nome,
                PathsStarted = progresses.Count,
                PathsCompleted = completedPaths.Count,
                CompletionRate = progresses.Count > 0 ? (double)completedPaths.Count / progresses.Count * 100.0 : 0.0,
                AverageGrade = averageGrade,
                TotalTimeSpentMinutes = progresses.Sum(p => p.TempoGastoMinutos),
                LastActivity = progresses.Any() ? progresses.Max(p => p.UltimaAtividadeEm) : student.UltimoAcesso,
                StrugglingTopics = new List<string>() // Implement based on low grades or incomplete modules
            });
        }

        return performances.OrderByDescending(p => p.CompletionRate).ToList();
    }

    #endregion
}