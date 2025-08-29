using System.Diagnostics;
using System.Text;

namespace TutorCopiloto.Services
{
    public interface IGitService
    {
        Task<GitCloneResult> CloneRepositoryAsync(string repositoryUrl, string targetDirectory, string? branch = null);
        Task<GitPullResult> PullRepositoryAsync(string repositoryPath);
        Task<GitStatusResult> GetRepositoryStatusAsync(string repositoryPath);
        Task<GitLogResult> GetRepositoryLogAsync(string repositoryPath, int maxEntries = 10);
        Task<bool> IsValidRepositoryAsync(string repositoryPath);
        Task<string> GetCurrentBranchAsync(string repositoryPath);
        Task<List<string>> GetBranchesAsync(string repositoryPath);
        Task<GitCheckoutResult> CheckoutBranchAsync(string repositoryPath, string branch);
    }

    public class GitService : IGitService
    {
        private readonly ILogger<GitService> _logger;

        public GitService(ILogger<GitService> logger)
        {
            _logger = logger;
        }

        public async Task<GitCloneResult> CloneRepositoryAsync(string repositoryUrl, string targetDirectory, string? branch = null)
        {
            try
            {
                _logger.LogInformation("Clonando repositório: {RepositoryUrl} para {TargetDirectory}", repositoryUrl, targetDirectory);

                var arguments = $"clone \"{repositoryUrl}\" \"{targetDirectory}\"";
                if (!string.IsNullOrEmpty(branch))
                {
                    arguments += $" --branch {branch}";
                }

                var result = await ExecuteGitCommandAsync(arguments, null);

                if (result.Success)
                {
                    _logger.LogInformation("Repositório clonado com sucesso: {RepositoryUrl}", repositoryUrl);
                    return new GitCloneResult
                    {
                        Success = true,
                        RepositoryPath = targetDirectory,
                        Message = "Repositório clonado com sucesso"
                    };
                }
                else
                {
                    _logger.LogError("Falha ao clonar repositório: {Error}", result.Error);
                    return new GitCloneResult
                    {
                        Success = false,
                        Error = result.Error,
                        Message = "Falha ao clonar repositório"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao clonar repositório: {RepositoryUrl}", repositoryUrl);
                return new GitCloneResult
                {
                    Success = false,
                    Error = ex.Message,
                    Message = "Erro interno ao clonar repositório"
                };
            }
        }

        public async Task<GitPullResult> PullRepositoryAsync(string repositoryPath)
        {
            try
            {
                if (!await IsValidRepositoryAsync(repositoryPath))
                {
                    return new GitPullResult
                    {
                        Success = false,
                        Error = "Diretório não é um repositório git válido",
                        Message = "Diretório inválido"
                    };
                }

                _logger.LogInformation("Executando git pull em: {RepositoryPath}", repositoryPath);

                var result = await ExecuteGitCommandAsync("pull --no-edit", repositoryPath);

                if (result.Success)
                {
                    _logger.LogInformation("Git pull executado com sucesso em: {RepositoryPath}", repositoryPath);
                    return new GitPullResult
                    {
                        Success = true,
                        Message = "Repositório atualizado com sucesso",
                        Output = result.Output
                    };
                }
                else
                {
                    _logger.LogWarning("Git pull com conflitos ou erros em: {RepositoryPath}. Error: {Error}", repositoryPath, result.Error);
                    return new GitPullResult
                    {
                        Success = false,
                        Error = result.Error,
                        Message = "Falha ao atualizar repositório",
                        Output = result.Output
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar git pull em: {RepositoryPath}", repositoryPath);
                return new GitPullResult
                {
                    Success = false,
                    Error = ex.Message,
                    Message = "Erro interno ao atualizar repositório"
                };
            }
        }

        public async Task<GitStatusResult> GetRepositoryStatusAsync(string repositoryPath)
        {
            try
            {
                if (!await IsValidRepositoryAsync(repositoryPath))
                {
                    return new GitStatusResult
                    {
                        Success = false,
                        Error = "Diretório não é um repositório git válido",
                        Message = "Diretório inválido"
                    };
                }

                var result = await ExecuteGitCommandAsync("status --porcelain", repositoryPath);

                if (result.Success)
                {
                    var statusLines = result.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    var modifiedFiles = new List<string>();
                    var untrackedFiles = new List<string>();
                    var stagedFiles = new List<string>();

                    foreach (var line in statusLines)
                    {
                        if (line.Length < 3) continue;

                        var status = line.Substring(0, 2);
                        var filePath = line.Substring(3);

                        if (status.Contains('M') || status.Contains('A') || status.Contains('D'))
                        {
                            stagedFiles.Add(filePath);
                        }
                        else if (status.Contains('?'))
                        {
                            untrackedFiles.Add(filePath);
                        }
                        else if (status.Contains('M') || status.Contains('D'))
                        {
                            modifiedFiles.Add(filePath);
                        }
                    }

                    return new GitStatusResult
                    {
                        Success = true,
                        IsClean = statusLines.Length == 0,
                        ModifiedFiles = modifiedFiles,
                        UntrackedFiles = untrackedFiles,
                        StagedFiles = stagedFiles,
                        Message = statusLines.Length == 0 ? "Repositório limpo" : $"{statusLines.Length} arquivo(s) modificado(s)"
                    };
                }
                else
                {
                    return new GitStatusResult
                    {
                        Success = false,
                        Error = result.Error,
                        Message = "Erro ao obter status do repositório"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter status do repositório: {RepositoryPath}", repositoryPath);
                return new GitStatusResult
                {
                    Success = false,
                    Error = ex.Message,
                    Message = "Erro interno ao obter status"
                };
            }
        }

        public async Task<GitLogResult> GetRepositoryLogAsync(string repositoryPath, int maxEntries = 10)
        {
            try
            {
                if (!await IsValidRepositoryAsync(repositoryPath))
                {
                    return new GitLogResult
                    {
                        Success = false,
                        Error = "Diretório não é um repositório git válido",
                        Message = "Diretório inválido"
                    };
                }

                var format = "--pretty=format:%H|%an|%ae|%ad|%s";
                var arguments = $"log {format} --date=short -n {maxEntries}";

                var result = await ExecuteGitCommandAsync(arguments, repositoryPath);

                if (result.Success)
                {
                    var commits = new List<GitCommit>();
                    var lines = result.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

                    foreach (var line in lines)
                    {
                        var parts = line.Split('|');
                        if (parts.Length >= 5)
                        {
                            commits.Add(new GitCommit
                            {
                                Hash = parts[0],
                                AuthorName = parts[1],
                                AuthorEmail = parts[2],
                                Date = parts[3],
                                Message = parts[4]
                            });
                        }
                    }

                    return new GitLogResult
                    {
                        Success = true,
                        Commits = commits,
                        Message = $"{commits.Count} commit(s) encontrado(s)"
                    };
                }
                else
                {
                    return new GitLogResult
                    {
                        Success = false,
                        Error = result.Error,
                        Message = "Erro ao obter histórico de commits"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter log do repositório: {RepositoryPath}", repositoryPath);
                return new GitLogResult
                {
                    Success = false,
                    Error = ex.Message,
                    Message = "Erro interno ao obter histórico"
                };
            }
        }

        public async Task<bool> IsValidRepositoryAsync(string repositoryPath)
        {
            try
            {
                var result = await ExecuteGitCommandAsync("rev-parse --git-dir", repositoryPath);
                return result.Success;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GetCurrentBranchAsync(string repositoryPath)
        {
            try
            {
                if (!await IsValidRepositoryAsync(repositoryPath))
                {
                    return string.Empty;
                }

                var result = await ExecuteGitCommandAsync("rev-parse --abbrev-ref HEAD", repositoryPath);
                return result.Success ? result.Output.Trim() : string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter branch atual: {RepositoryPath}", repositoryPath);
                return string.Empty;
            }
        }

        public async Task<List<string>> GetBranchesAsync(string repositoryPath)
        {
            try
            {
                if (!await IsValidRepositoryAsync(repositoryPath))
                {
                    return new List<string>();
                }

                var result = await ExecuteGitCommandAsync("branch -a", repositoryPath);
                if (!result.Success)
                {
                    return new List<string>();
                }

                var branches = new List<string>();
                var lines = result.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    var branch = line.Trim();
                    if (branch.StartsWith('*'))
                    {
                        branch = branch.Substring(1).Trim();
                    }
                    branches.Add(branch);
                }

                return branches;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter branches: {RepositoryPath}", repositoryPath);
                return new List<string>();
            }
        }

        public async Task<GitCheckoutResult> CheckoutBranchAsync(string repositoryPath, string branch)
        {
            try
            {
                if (!await IsValidRepositoryAsync(repositoryPath))
                {
                    return new GitCheckoutResult
                    {
                        Success = false,
                        Error = "Diretório não é um repositório git válido",
                        Message = "Diretório inválido"
                    };
                }

                var result = await ExecuteGitCommandAsync($"checkout {branch}", repositoryPath);

                if (result.Success)
                {
                    return new GitCheckoutResult
                    {
                        Success = true,
                        Branch = branch,
                        Message = $"Branch alterado para: {branch}"
                    };
                }
                else
                {
                    return new GitCheckoutResult
                    {
                        Success = false,
                        Error = result.Error,
                        Message = $"Falha ao alterar para branch: {branch}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao fazer checkout: {RepositoryPath}, Branch: {Branch}", repositoryPath, branch);
                return new GitCheckoutResult
                {
                    Success = false,
                    Error = ex.Message,
                    Message = "Erro interno ao alterar branch"
                };
            }
        }

        private async Task<GitCommandResult> ExecuteGitCommandAsync(string arguments, string? workingDirectory)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                if (!string.IsNullOrEmpty(workingDirectory))
                {
                    startInfo.WorkingDirectory = workingDirectory;
                }

                using var process = Process.Start(startInfo);
                if (process == null)
                {
                    throw new Exception("Falha ao iniciar processo git");
                }

                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                return new GitCommandResult
                {
                    Success = process.ExitCode == 0,
                    Output = output,
                    Error = error,
                    ExitCode = process.ExitCode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar comando git: {Arguments}", arguments);
                return new GitCommandResult
                {
                    Success = false,
                    Error = ex.Message,
                    ExitCode = -1
                };
            }
        }
    }

    // Result classes
    public class GitCommandResult
    {
        public bool Success { get; set; }
        public string Output { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
        public int ExitCode { get; set; }
    }

    public class GitCloneResult
    {
        public bool Success { get; set; }
        public string RepositoryPath { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }

    public class GitPullResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
        public string Output { get; set; } = string.Empty;
    }

    public class GitStatusResult
    {
        public bool Success { get; set; }
        public bool IsClean { get; set; }
        public List<string> ModifiedFiles { get; set; } = new();
        public List<string> UntrackedFiles { get; set; } = new();
        public List<string> StagedFiles { get; set; } = new();
        public string Message { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }

    public class GitLogResult
    {
        public bool Success { get; set; }
        public List<GitCommit> Commits { get; set; } = new();
        public string Message { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }

    public class GitCommit
    {
        public string Hash { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorEmail { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class GitCheckoutResult
    {
        public bool Success { get; set; }
        public string Branch { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }
}
