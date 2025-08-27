using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TutorCopiloto.Services;

namespace TutorCopiloto.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GitController : ControllerBase
    {
        private readonly IGitService _gitService;
        private readonly ILogger<GitController> _logger;

        public GitController(IGitService gitService, ILogger<GitController> logger)
        {
            _gitService = gitService;
            _logger = logger;
        }

        [HttpPost("clone")]
        public async Task<IActionResult> CloneRepository([FromBody] CloneRepositoryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Solicitação de clone recebida: {RepositoryUrl}", request.RepositoryUrl);

                // Gera diretório único baseado no nome do repositório
                var repoName = GetRepositoryNameFromUrl(request.RepositoryUrl);
                var targetDirectory = Path.Combine("/tmp/repositories", repoName);

                // Cria diretório se não existir
                Directory.CreateDirectory(Path.GetDirectoryName(targetDirectory)!);

                var result = await _gitService.CloneRepositoryAsync(
                    request.RepositoryUrl,
                    targetDirectory,
                    request.Branch
                );

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        repositoryPath = result.RepositoryPath,
                        message = result.Message,
                        repositoryName = repoName
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        error = result.Error,
                        message = result.Message
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar solicitação de clone");
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        [HttpPost("pull")]
        public async Task<IActionResult> PullRepository([FromBody] RepositoryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Solicitação de pull recebida: {RepositoryPath}", request.RepositoryPath);

                var result = await _gitService.PullRepositoryAsync(request.RepositoryPath);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = result.Message,
                        output = result.Output
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        error = result.Error,
                        message = result.Message,
                        output = result.Output
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar solicitação de pull");
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetRepositoryStatus([FromQuery] string repositoryPath)
        {
            try
            {
                if (string.IsNullOrEmpty(repositoryPath))
                {
                    return BadRequest("RepositoryPath é obrigatório");
                }

                var result = await _gitService.GetRepositoryStatusAsync(repositoryPath);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        isClean = result.IsClean,
                        modifiedFiles = result.ModifiedFiles,
                        untrackedFiles = result.UntrackedFiles,
                        stagedFiles = result.StagedFiles,
                        message = result.Message
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        error = result.Error,
                        message = result.Message
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter status do repositório");
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        [HttpGet("log")]
        public async Task<IActionResult> GetRepositoryLog([FromQuery] string repositoryPath, [FromQuery] int maxEntries = 10)
        {
            try
            {
                if (string.IsNullOrEmpty(repositoryPath))
                {
                    return BadRequest("RepositoryPath é obrigatório");
                }

                var result = await _gitService.GetRepositoryLogAsync(repositoryPath, maxEntries);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        commits = result.Commits,
                        message = result.Message
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        error = result.Error,
                        message = result.Message
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter log do repositório");
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        [HttpGet("branches")]
        public async Task<IActionResult> GetBranches([FromQuery] string repositoryPath)
        {
            try
            {
                if (string.IsNullOrEmpty(repositoryPath))
                {
                    return BadRequest("RepositoryPath é obrigatório");
                }

                var branches = await _gitService.GetBranchesAsync(repositoryPath);
                var currentBranch = await _gitService.GetCurrentBranchAsync(repositoryPath);

                return Ok(new
                {
                    success = true,
                    branches = branches,
                    currentBranch = currentBranch
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter branches do repositório");
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> CheckoutBranch([FromBody] CheckoutRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Solicitação de checkout recebida: {RepositoryPath}, Branch: {Branch}",
                    request.RepositoryPath, request.Branch);

                var result = await _gitService.CheckoutBranchAsync(request.RepositoryPath, request.Branch);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        branch = result.Branch,
                        message = result.Message
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        error = result.Error,
                        message = result.Message
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar solicitação de checkout");
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        [HttpGet("repositories")]
        public IActionResult GetRepositories()
        {
            try
            {
                var repositoriesPath = "/tmp/repositories";
                if (!Directory.Exists(repositoriesPath))
                {
                    return Ok(new { success = true, repositories = new List<object>() });
                }

                var repositories = new List<object>();
                var directories = Directory.GetDirectories(repositoriesPath);

                foreach (var dir in directories)
                {
                    var repoName = Path.GetFileName(dir);
                    var isValid = _gitService.IsValidRepositoryAsync(dir).GetAwaiter().GetResult();

                    if (isValid)
                    {
                        repositories.Add(new
                        {
                            name = repoName,
                            path = dir,
                            isValid = true
                        });
                    }
                }

                return Ok(new
                {
                    success = true,
                    repositories = repositories
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar repositórios");
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        private string GetRepositoryNameFromUrl(string repositoryUrl)
        {
            // Remove .git do final se existir
            if (repositoryUrl.EndsWith(".git"))
            {
                repositoryUrl = repositoryUrl.Substring(0, repositoryUrl.Length - 4);
            }

            // Extrai o nome do repositório da URL
            var parts = repositoryUrl.Split('/');
            var repoName = parts.Last();

            // Adiciona timestamp para evitar conflitos
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            return $"{repoName}_{timestamp}";
        }
    }

    // DTOs
    public class CloneRepositoryRequest
    {
        [Required]
        [Url]
        public string RepositoryUrl { get; set; } = string.Empty;

        public string? Branch { get; set; }
    }

    public class RepositoryRequest
    {
        [Required]
        public string RepositoryPath { get; set; } = string.Empty;
    }

    public class CheckoutRequest
    {
        [Required]
        public string RepositoryPath { get; set; } = string.Empty;

        [Required]
        public string Branch { get; set; } = string.Empty;
    }
}
