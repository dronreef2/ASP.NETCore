using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TutorCopiloto.Models;
using TutorCopiloto.Services;

namespace TutorCopiloto.Controllers;

/// <summary>
/// API Controller para gerenciamento de trilhas de aprendizado
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class LearningPathController : ControllerBase
{
    private readonly ILearningPathService _learningPathService;
    private readonly ILogger<LearningPathController> _logger;

    public LearningPathController(
        ILearningPathService learningPathService,
        ILogger<LearningPathController> logger)
    {
        _learningPathService = learningPathService;
        _logger = logger;
    }

    #region Learning Path Operations

    /// <summary>
    /// Lista todas as trilhas de aprendizado disponíveis
    /// </summary>
    /// <param name="includeInactive">Incluir trilhas inativas</param>
    /// <returns>Lista de trilhas de aprendizado</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<LearningPath>), 200)]
    public async Task<ActionResult<List<LearningPath>>> GetAllLearningPaths([FromQuery] bool includeInactive = false)
    {
        try
        {
            var paths = await _learningPathService.GetAllLearningPathsAsync(includeInactive);
            return Ok(paths);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving learning paths");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém trilhas de aprendizado por categoria
    /// </summary>
    /// <param name="category">Categoria da trilha</param>
    /// <returns>Lista de trilhas da categoria especificada</returns>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(List<LearningPath>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<List<LearningPath>>> GetLearningPathsByCategory(LearningPathCategory category)
    {
        try
        {
            var paths = await _learningPathService.GetLearningPathsByCategoryAsync(category);
            return Ok(paths);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving learning paths by category {Category}", category);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém trilhas de aprendizado por nível de dificuldade
    /// </summary>
    /// <param name="difficulty">Nível de dificuldade</param>
    /// <returns>Lista de trilhas do nível especificado</returns>
    [HttpGet("difficulty/{difficulty}")]
    [ProducesResponseType(typeof(List<LearningPath>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<List<LearningPath>>> GetLearningPathsByDifficulty(LearningPathDifficulty difficulty)
    {
        try
        {
            var paths = await _learningPathService.GetLearningPathsByDifficultyAsync(difficulty);
            return Ok(paths);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving learning paths by difficulty {Difficulty}", difficulty);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém uma trilha de aprendizado específica
    /// </summary>
    /// <param name="id">ID da trilha</param>
    /// <param name="includeModules">Incluir módulos da trilha</param>
    /// <returns>Trilha de aprendizado</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LearningPath), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<LearningPath>> GetLearningPathById(string id, [FromQuery] bool includeModules = true)
    {
        try
        {
            var path = await _learningPathService.GetLearningPathByIdAsync(id, includeModules);
            if (path == null)
            {
                return NotFound($"Trilha de aprendizado {id} não encontrada");
            }

            return Ok(path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving learning path {PathId}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Cria uma nova trilha de aprendizado
    /// </summary>
    /// <param name="learningPath">Dados da trilha</param>
    /// <returns>Trilha criada</returns>
    [HttpPost]
    [ProducesResponseType(typeof(LearningPath), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<LearningPath>> CreateLearningPath([FromBody] LearningPath learningPath)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdPath = await _learningPathService.CreateLearningPathAsync(learningPath);
            return CreatedAtAction(nameof(GetLearningPathById), new { id = createdPath.Id }, createdPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating learning path");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Atualiza uma trilha de aprendizado existente
    /// </summary>
    /// <param name="id">ID da trilha</param>
    /// <param name="learningPath">Dados atualizados da trilha</param>
    /// <returns>Trilha atualizada</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(LearningPath), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<LearningPath>> UpdateLearningPath(string id, [FromBody] LearningPath learningPath)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedPath = await _learningPathService.UpdateLearningPathAsync(id, learningPath);
            if (updatedPath == null)
            {
                return NotFound($"Trilha de aprendizado {id} não encontrada");
            }

            return Ok(updatedPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating learning path {PathId}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Remove uma trilha de aprendizado
    /// </summary>
    /// <param name="id">ID da trilha</param>
    /// <returns>Resultado da operação</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteLearningPath(string id)
    {
        try
        {
            var deleted = await _learningPathService.DeleteLearningPathAsync(id);
            if (!deleted)
            {
                return NotFound($"Trilha de aprendizado {id} não encontrada");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting learning path {PathId}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Alterna o status ativo/inativo de uma trilha
    /// </summary>
    /// <param name="id">ID da trilha</param>
    /// <returns>Resultado da operação</returns>
    [HttpPatch("{id}/toggle-status")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ToggleLearningPathStatus(string id)
    {
        try
        {
            var success = await _learningPathService.ToggleLearningPathStatusAsync(id);
            if (!success)
            {
                return NotFound($"Trilha de aprendizado {id} não encontrada");
            }

            return Ok(new { message = "Status da trilha alterado com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling learning path status {PathId}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    #endregion

    #region Module Operations

    /// <summary>
    /// Obtém todos os módulos de uma trilha
    /// </summary>
    /// <param name="pathId">ID da trilha</param>
    /// <returns>Lista de módulos</returns>
    [HttpGet("{pathId}/modules")]
    [ProducesResponseType(typeof(List<LearningModule>), 200)]
    public async Task<ActionResult<List<LearningModule>>> GetModulesByPathId(string pathId)
    {
        try
        {
            var modules = await _learningPathService.GetModulesByPathIdAsync(pathId);
            return Ok(modules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving modules for path {PathId}", pathId);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém um módulo específico
    /// </summary>
    /// <param name="moduleId">ID do módulo</param>
    /// <returns>Módulo de aprendizado</returns>
    [HttpGet("modules/{moduleId}")]
    [ProducesResponseType(typeof(LearningModule), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<LearningModule>> GetModuleById(string moduleId)
    {
        try
        {
            var module = await _learningPathService.GetModuleByIdAsync(moduleId);
            if (module == null)
            {
                return NotFound($"Módulo {moduleId} não encontrado");
            }

            return Ok(module);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving module {ModuleId}", moduleId);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Cria um novo módulo em uma trilha
    /// </summary>
    /// <param name="module">Dados do módulo</param>
    /// <returns>Módulo criado</returns>
    [HttpPost("modules")]
    [ProducesResponseType(typeof(LearningModule), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<LearningModule>> CreateModule([FromBody] LearningModule module)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdModule = await _learningPathService.CreateModuleAsync(module);
            return CreatedAtAction(nameof(GetModuleById), new { moduleId = createdModule.Id }, createdModule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating module");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Atualiza um módulo existente
    /// </summary>
    /// <param name="moduleId">ID do módulo</param>
    /// <param name="module">Dados atualizados do módulo</param>
    /// <returns>Módulo atualizado</returns>
    [HttpPut("modules/{moduleId}")]
    [ProducesResponseType(typeof(LearningModule), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<LearningModule>> UpdateModule(string moduleId, [FromBody] LearningModule module)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedModule = await _learningPathService.UpdateModuleAsync(moduleId, module);
            if (updatedModule == null)
            {
                return NotFound($"Módulo {moduleId} não encontrado");
            }

            return Ok(updatedModule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating module {ModuleId}", moduleId);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Remove um módulo
    /// </summary>
    /// <param name="moduleId">ID do módulo</param>
    /// <returns>Resultado da operação</returns>
    [HttpDelete("modules/{moduleId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteModule(string moduleId)
    {
        try
        {
            var deleted = await _learningPathService.DeleteModuleAsync(moduleId);
            if (!deleted)
            {
                return NotFound($"Módulo {moduleId} não encontrado");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting module {ModuleId}", moduleId);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Reordena os módulos de uma trilha
    /// </summary>
    /// <param name="pathId">ID da trilha</param>
    /// <param name="moduleIds">IDs dos módulos na nova ordem</param>
    /// <returns>Resultado da operação</returns>
    [HttpPut("{pathId}/modules/reorder")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ReorderModules(string pathId, [FromBody] List<string> moduleIds)
    {
        try
        {
            var success = await _learningPathService.ReorderModulesAsync(pathId, moduleIds);
            if (!success)
            {
                return BadRequest("Erro ao reordenar módulos");
            }

            return Ok(new { message = "Módulos reordenados com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering modules for path {PathId}", pathId);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    #endregion

    #region Student Progress Operations

    /// <summary>
    /// Obtém o progresso do usuário em uma trilha específica
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <param name="pathId">ID da trilha</param>
    /// <returns>Progresso do estudante</returns>
    [HttpGet("progress/{userId}/{pathId}")]
    [ProducesResponseType(typeof(StudentProgress), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<StudentProgress>> GetStudentProgress(string userId, string pathId)
    {
        try
        {
            var progress = await _learningPathService.GetStudentProgressAsync(userId, pathId);
            if (progress == null)
            {
                return NotFound($"Progresso não encontrado para usuário {userId} na trilha {pathId}");
            }

            return Ok(progress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving progress for user {UserId} in path {PathId}", userId, pathId);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém todo o progresso de um usuário
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <returns>Lista de progressos do usuário</returns>
    [HttpGet("progress/user/{userId}")]
    [ProducesResponseType(typeof(List<StudentProgress>), 200)]
    public async Task<ActionResult<List<StudentProgress>>> GetStudentProgressByUserId(string userId)
    {
        try
        {
            var progresses = await _learningPathService.GetStudentProgressByUserIdAsync(userId);
            return Ok(progresses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving progress for user {UserId}", userId);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Inicia uma trilha de aprendizado para um usuário
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <param name="pathId">ID da trilha</param>
    /// <returns>Progresso criado</returns>
    [HttpPost("progress/{userId}/{pathId}/start")]
    [ProducesResponseType(typeof(StudentProgress), 201)]
    public async Task<ActionResult<StudentProgress>> StartLearningPath(string userId, string pathId)
    {
        try
        {
            var progress = await _learningPathService.StartLearningPathAsync(userId, pathId);
            return CreatedAtAction(nameof(GetStudentProgress), new { userId, pathId }, progress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting path {PathId} for user {UserId}", pathId, userId);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Completa um módulo para um usuário
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <param name="pathId">ID da trilha</param>
    /// <param name="moduleId">ID do módulo</param>
    /// <param name="request">Dados da conclusão</param>
    /// <returns>Conclusão do módulo</returns>
    [HttpPost("progress/{userId}/{pathId}/modules/{moduleId}/complete")]
    [ProducesResponseType(typeof(ModuleCompletion), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<ModuleCompletion>> CompleteModule(
        string userId, 
        string pathId, 
        string moduleId, 
        [FromBody] CompleteModuleRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var completion = await _learningPathService.CompleteModuleAsync(
                userId, pathId, moduleId, request.TimeSpentMinutes, request.Grade, request.Feedback);

            if (completion == null)
            {
                return BadRequest("Erro ao completar módulo");
            }

            return CreatedAtAction(nameof(GetStudentProgress), new { userId, pathId }, completion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing module {ModuleId} for user {UserId}", moduleId, userId);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém analytics de aprendizado para um usuário
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <returns>Analytics de aprendizado</returns>
    [HttpGet("analytics/{userId}")]
    [ProducesResponseType(typeof(LearningAnalytics), 200)]
    public async Task<ActionResult<LearningAnalytics>> GetLearningAnalytics(string userId)
    {
        try
        {
            var analytics = await _learningPathService.GetLearningAnalyticsAsync(userId);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analytics for user {UserId}", userId);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém trilhas recomendadas para um usuário
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <returns>Lista de trilhas recomendadas</returns>
    [HttpGet("recommendations/{userId}")]
    [ProducesResponseType(typeof(List<LearningPath>), 200)]
    public async Task<ActionResult<List<LearningPath>>> GetRecommendedPaths(string userId)
    {
        try
        {
            var recommendations = await _learningPathService.GetRecommendedPathsAsync(userId);
            return Ok(recommendations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recommendations for user {UserId}", userId);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém estatísticas de uma trilha
    /// </summary>
    /// <param name="pathId">ID da trilha</param>
    /// <returns>Estatísticas da trilha</returns>
    [HttpGet("{pathId}/statistics")]
    [ProducesResponseType(typeof(LearningPathStatistics), 200)]
    public async Task<ActionResult<LearningPathStatistics>> GetPathStatistics(string pathId)
    {
        try
        {
            var statistics = await _learningPathService.GetPathStatisticsAsync(pathId);
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving statistics for path {PathId}", pathId);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    #endregion
}

/// <summary>
/// Request model para completar um módulo
/// </summary>
public class CompleteModuleRequest
{
    public int TimeSpentMinutes { get; set; }
    public double? Grade { get; set; }
    public string? Feedback { get; set; }
}