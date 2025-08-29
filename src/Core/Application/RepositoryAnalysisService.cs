using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text;

namespace TutorCopiloto.Services
{
    public interface IRepositoryAnalysisService
    {
        Task<RepositoryAnalysis> AnalyzeRepositoryAsync(string repositoryUrl, string branch = "main");
        Task<RepositoryAnalysis?> GetRepositoryAnalysisAsync(string repositoryUrl);
    }

    public class RepositoryAnalysisService : IRepositoryAnalysisService
    {
        private readonly ILogger<RepositoryAnalysisService> _logger;
        private readonly IConfiguration _configuration;
        private readonly Dictionary<string, RepositoryAnalysis> _analyses = new();

        public RepositoryAnalysisService(ILogger<RepositoryAnalysisService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<RepositoryAnalysis> AnalyzeRepositoryAsync(string repositoryUrl, string branch = "main")
        {
            var cacheKey = $"{repositoryUrl}#{branch}";
            
            if (_analyses.TryGetValue(cacheKey, out var cachedAnalysis))
            {
                _logger.LogInformation("Retornando análise em cache para {RepositoryUrl}", repositoryUrl);
                return cachedAnalysis;
            }

            var analysis = new RepositoryAnalysis
            {
                RepositoryUrl = repositoryUrl,
                Branch = branch,
                AnalyzedAt = DateTime.UtcNow,
                AnalysisSteps = new List<AnalysisStep>()
            };

            try
            {
                // 1. Clonar repositório
                var clonePath = await CloneRepositoryAsync(repositoryUrl, branch, analysis);
                
                if (string.IsNullOrEmpty(clonePath))
                {
                    analysis.Status = "Failed";
                    analysis.ErrorMessage = "Falha ao clonar repositório";
                    _analyses[cacheKey] = analysis;
                    return analysis;
                }

                // 2. Analisar estrutura de arquivos
                AnalyzeFileStructure(clonePath, analysis);
                
                // 3. Identificar linguagens de programação
                IdentifyProgrammingLanguages(clonePath, analysis);
                
                // 4. Analisar arquivos de configuração
                await AnalyzeConfigurationFilesAsync(clonePath, analysis);
                
                // 5. Ler arquivos importantes
                await ReadImportantFilesAsync(clonePath, analysis);
                
                // 6. Gerar relatório final
                GenerateFinalReport(analysis);
                
                analysis.Status = "Completed";
                
                // Limpar arquivos temporários
                CleanupRepository(clonePath);
                
                _logger.LogInformation("Análise concluída para {RepositoryUrl}", repositoryUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante análise do repositório {RepositoryUrl}", repositoryUrl);
                analysis.Status = "Failed";
                analysis.ErrorMessage = ex.Message;
            }

            _analyses[cacheKey] = analysis;
            return analysis;
        }

        public async Task<RepositoryAnalysis?> GetRepositoryAnalysisAsync(string repositoryUrl)
        {
            await Task.CompletedTask;
            var cacheKey = $"{repositoryUrl}#main";
            return _analyses.TryGetValue(cacheKey, out var analysis) ? analysis : null;
        }

        private async Task<string?> CloneRepositoryAsync(string repositoryUrl, string branch, RepositoryAnalysis analysis)
        {
            var step = new AnalysisStep
            {
                StepName = "Clonando Repositório",
                StartedAt = DateTime.UtcNow,
                Status = "Running"
            };
            
            analysis.AnalysisSteps.Add(step);

            try
            {
                // Criar diretório temporário
                var tempDir = Path.Combine(Path.GetTempPath(), "repo_analysis", Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempDir);

                step.Logs.Add($"Criando diretório temporário: {tempDir}");

                // Comando git clone
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "git",
                        Arguments = $"clone --branch {branch} --depth 1 {repositoryUrl} .",
                        WorkingDirectory = tempDir,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                step.Logs.Add($"Executando: git clone --branch {branch} --depth 1 {repositoryUrl}");
                
                process.Start();
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (process.ExitCode == 0)
                {
                    step.Status = "Completed";
                    step.Logs.Add("Repositório clonado com sucesso");
                    step.Logs.Add($"Output: {output}");
                    
                    analysis.RepositoryName = ExtractRepositoryName(repositoryUrl);
                    analysis.TotalFiles = Directory.GetFiles(tempDir, "*", SearchOption.AllDirectories).Length;
                    
                    return tempDir;
                }
                else
                {
                    step.Status = "Failed";
                    step.Logs.Add($"Erro ao clonar: {error}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                step.Status = "Failed";
                step.Logs.Add($"Exceção: {ex.Message}");
                return null;
            }
            finally
            {
                step.CompletedAt = DateTime.UtcNow;
            }
        }

        private void AnalyzeFileStructure(string clonePath, RepositoryAnalysis analysis)
        {
            var step = new AnalysisStep
            {
                StepName = "Analisando Estrutura de Arquivos",
                StartedAt = DateTime.UtcNow,
                Status = "Running"
            };
            
            analysis.AnalysisSteps.Add(step);

            try
            {
                var allFiles = Directory.GetFiles(clonePath, "*", SearchOption.AllDirectories);
                var fileExtensions = new Dictionary<string, int>();
                var directoryStructure = new List<string>();

                foreach (var file in allFiles)
                {
                    var extension = Path.GetExtension(file).ToLower();
                    if (!string.IsNullOrEmpty(extension))
                    {
                        fileExtensions[extension] = fileExtensions.GetValueOrDefault(extension, 0) + 1;
                    }
                }

                // Analisar estrutura de diretórios
                var directories = Directory.GetDirectories(clonePath, "*", SearchOption.AllDirectories);
                foreach (var dir in directories)
                {
                    var relativePath = Path.GetRelativePath(clonePath, dir);
                    directoryStructure.Add(relativePath);
                }

                analysis.FileExtensions = fileExtensions;
                analysis.DirectoryStructure = directoryStructure;
                analysis.TotalSize = CalculateDirectorySize(clonePath);

                step.Status = "Completed";
                step.Logs.Add($"Encontrados {allFiles.Length} arquivos");
                step.Logs.Add($"Extensões encontradas: {string.Join(", ", fileExtensions.Keys)}");
                step.Logs.Add($"Tamanho total: {analysis.TotalSize} bytes");
            }
            catch (Exception ex)
            {
                step.Status = "Failed";
                step.Logs.Add($"Erro: {ex.Message}");
            }
            finally
            {
                step.CompletedAt = DateTime.UtcNow;
            }
        }

        private void IdentifyProgrammingLanguages(string clonePath, RepositoryAnalysis analysis)
        {
            var step = new AnalysisStep
            {
                StepName = "Identificando Linguagens de Programação",
                StartedAt = DateTime.UtcNow,
                Status = "Running"
            };
            
            analysis.AnalysisSteps.Add(step);

            try
            {
                var languages = new Dictionary<string, LanguageInfo>();
                var allFiles = Directory.GetFiles(clonePath, "*", SearchOption.AllDirectories);

                foreach (var file in allFiles)
                {
                    var extension = Path.GetExtension(file).ToLower();
                    var language = IdentifyLanguageByExtension(extension);
                    
                    if (!string.IsNullOrEmpty(language))
                    {
                        if (!languages.ContainsKey(language))
                        {
                            languages[language] = new LanguageInfo
                            {
                                Name = language,
                                FileCount = 0,
                                FileExtensions = new List<string>()
                            };
                        }
                        
                        languages[language].FileCount++;
                        if (!languages[language].FileExtensions.Contains(extension))
                        {
                            languages[language].FileExtensions.Add(extension);
                        }
                    }
                }

                analysis.ProgrammingLanguages = languages.Values.ToList();
                
                step.Status = "Completed";
                step.Logs.Add($"Linguagens identificadas: {string.Join(", ", languages.Keys)}");
            }
            catch (Exception ex)
            {
                step.Status = "Failed";
                step.Logs.Add($"Erro: {ex.Message}");
            }
            finally
            {
                step.CompletedAt = DateTime.UtcNow;
            }
        }

        private async Task AnalyzeConfigurationFilesAsync(string clonePath, RepositoryAnalysis analysis)
        {
            var step = new AnalysisStep
            {
                StepName = "Analisando Arquivos de Configuração",
                StartedAt = DateTime.UtcNow,
                Status = "Running"
            };
            
            analysis.AnalysisSteps.Add(step);

            try
            {
                var configFiles = new List<ConfigurationFile>();
                var configFilePatterns = new[]
                {
                    "package.json", "requirements.txt", "Cargo.toml", "composer.json",
                    "Gemfile", "go.mod", "build.gradle", "pom.xml", "Dockerfile",
                    "docker-compose.yml", ".env", "appsettings.json", "web.config",
                    "tsconfig.json", "vite.config.js", "webpack.config.js"
                };

                foreach (var pattern in configFilePatterns)
                {
                    var files = Directory.GetFiles(clonePath, pattern, SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        try
                        {
                            var content = await File.ReadAllTextAsync(file);
                            var relativePath = Path.GetRelativePath(clonePath, file);
                            
                            configFiles.Add(new ConfigurationFile
                            {
                                FileName = Path.GetFileName(file),
                                FilePath = relativePath,
                                Content = content.Length > 10000 ? content.Substring(0, 10000) + "..." : content,
                                FileType = IdentifyConfigFileType(pattern)
                            });
                            
                            step.Logs.Add($"Analisado: {relativePath}");
                        }
                        catch (Exception ex)
                        {
                            step.Logs.Add($"Erro ao ler {file}: {ex.Message}");
                        }
                    }
                }

                analysis.ConfigurationFiles = configFiles;
                step.Status = "Completed";
            }
            catch (Exception ex)
            {
                step.Status = "Failed";
                step.Logs.Add($"Erro: {ex.Message}");
            }
            finally
            {
                step.CompletedAt = DateTime.UtcNow;
            }
        }

        private async Task ReadImportantFilesAsync(string clonePath, RepositoryAnalysis analysis)
        {
            var step = new AnalysisStep
            {
                StepName = "Lendo Arquivos Importantes",
                StartedAt = DateTime.UtcNow,
                Status = "Running"
            };
            
            analysis.AnalysisSteps.Add(step);

            try
            {
                var importantFiles = new List<ImportantFile>();
                var importantFileNames = new[]
                {
                    "README.md", "README.txt", "readme.md", "readme.txt",
                    "CONTRIBUTING.md", "CHANGELOG.md", "LICENSE", "LICENSE.txt",
                    ".gitignore", "Dockerfile", "docker-compose.yml"
                };

                foreach (var fileName in importantFileNames)
                {
                    var files = Directory.GetFiles(clonePath, fileName, SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        try
                        {
                            var content = await File.ReadAllTextAsync(file);
                            var relativePath = Path.GetRelativePath(clonePath, file);
                            
                            importantFiles.Add(new ImportantFile
                            {
                                FileName = Path.GetFileName(file),
                                FilePath = relativePath,
                                Content = content.Length > 50000 ? content.Substring(0, 50000) + "..." : content,
                                FileType = IdentifyImportantFileType(fileName)
                            });
                            
                            step.Logs.Add($"Lido: {relativePath} ({content.Length} caracteres)");
                        }
                        catch (Exception ex)
                        {
                            step.Logs.Add($"Erro ao ler {file}: {ex.Message}");
                        }
                    }
                }

                analysis.ImportantFiles = importantFiles;
                step.Status = "Completed";
            }
            catch (Exception ex)
            {
                step.Status = "Failed";
                step.Logs.Add($"Erro: {ex.Message}");
            }
            finally
            {
                step.CompletedAt = DateTime.UtcNow;
            }
        }

        private void GenerateFinalReport(RepositoryAnalysis analysis)
        {
            var report = new StringBuilder();
            report.AppendLine("# 📊 Relatório de Análise do Repositório");
            report.AppendLine();
            report.AppendLine($"**Repositório:** {analysis.RepositoryUrl}");
            report.AppendLine($"**Branch:** {analysis.Branch}");
            report.AppendLine($"**Analisado em:** {analysis.AnalyzedAt:yyyy-MM-dd HH:mm:ss UTC}");
            report.AppendLine();

            // Estatísticas gerais
            report.AppendLine("## 📈 Estatísticas Gerais");
            report.AppendLine($"- **Arquivos Totais:** {analysis.TotalFiles}");
            report.AppendLine($"- **Tamanho Total:** {FormatFileSize(analysis.TotalSize)}");
            report.AppendLine($"- **Linguagens Identificadas:** {analysis.ProgrammingLanguages.Count}");
            report.AppendLine();

            // Linguagens de programação
            if (analysis.ProgrammingLanguages.Any())
            {
                report.AppendLine("## 💻 Linguagens de Programação");
                foreach (var lang in analysis.ProgrammingLanguages.OrderByDescending(l => l.FileCount))
                {
                    report.AppendLine($"- **{lang.Name}:** {lang.FileCount} arquivos ({string.Join(", ", lang.FileExtensions)})");
                }
                report.AppendLine();
            }

            // Estrutura de arquivos
            if (analysis.FileExtensions.Any())
            {
                report.AppendLine("## 📁 Extensões de Arquivo");
                foreach (var ext in analysis.FileExtensions.OrderByDescending(kv => kv.Value))
                {
                    report.AppendLine($"- **{ext.Key}:** {ext.Value} arquivos");
                }
                report.AppendLine();
            }

            // Arquivos de configuração
            if (analysis.ConfigurationFiles.Any())
            {
                report.AppendLine("## ⚙️ Arquivos de Configuração");
                foreach (var config in analysis.ConfigurationFiles)
                {
                    report.AppendLine($"- **{config.FileName}** ({config.FileType})");
                    report.AppendLine($"  Caminho: `{config.FilePath}`");
                }
                report.AppendLine();
            }

            analysis.FinalReport = report.ToString();
        }

        private void CleanupRepository(string clonePath)
        {
            try
            {
                if (Directory.Exists(clonePath))
                {
                    Directory.Delete(clonePath, true);
                    _logger.LogInformation("Diretório temporário limpo: {Path}", clonePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao limpar diretório temporário {Path}", clonePath);
            }
        }

        // Métodos auxiliares
        private string ExtractRepositoryName(string repositoryUrl)
        {
            var match = Regex.Match(repositoryUrl, @"github\.com/([^/]+/[^/]+?)(\.git|/|$)");
            return match.Success ? match.Groups[1].Value : "Unknown";
        }

        private string IdentifyLanguageByExtension(string extension)
        {
            var languageMap = new Dictionary<string, string>
            {
                [".js"] = "JavaScript",
                [".ts"] = "TypeScript", 
                [".jsx"] = "React/JavaScript",
                [".tsx"] = "React/TypeScript",
                [".py"] = "Python",
                [".java"] = "Java",
                [".cs"] = "C#",
                [".cpp"] = "C++",
                [".c"] = "C",
                [".php"] = "PHP",
                [".rb"] = "Ruby",
                [".go"] = "Go",
                [".rs"] = "Rust",
                [".swift"] = "Swift",
                [".kt"] = "Kotlin",
                [".scala"] = "Scala",
                [".html"] = "HTML",
                [".css"] = "CSS",
                [".scss"] = "SCSS",
                [".sql"] = "SQL",
                [".sh"] = "Shell",
                [".yml"] = "YAML",
                [".yaml"] = "YAML",
                [".json"] = "JSON",
                [".xml"] = "XML",
                [".md"] = "Markdown"
            };

            return languageMap.GetValueOrDefault(extension, "");
        }

        private string IdentifyConfigFileType(string fileName)
        {
            var typeMap = new Dictionary<string, string>
            {
                ["package.json"] = "Node.js Dependencies",
                ["requirements.txt"] = "Python Dependencies",
                ["Cargo.toml"] = "Rust Dependencies",
                ["composer.json"] = "PHP Dependencies",
                ["Gemfile"] = "Ruby Dependencies",
                ["go.mod"] = "Go Dependencies",
                ["build.gradle"] = "Gradle Build",
                ["pom.xml"] = "Maven Build",
                ["Dockerfile"] = "Docker Configuration",
                ["docker-compose.yml"] = "Docker Compose",
                [".env"] = "Environment Variables",
                ["appsettings.json"] = ".NET Configuration",
                ["web.config"] = "ASP.NET Configuration",
                ["tsconfig.json"] = "TypeScript Configuration",
                ["vite.config.js"] = "Vite Configuration",
                ["webpack.config.js"] = "Webpack Configuration"
            };

            return typeMap.GetValueOrDefault(fileName, "Configuration File");
        }

        private string IdentifyImportantFileType(string fileName)
        {
            var typeMap = new Dictionary<string, string>
            {
                ["README.md"] = "Documentation",
                ["README.txt"] = "Documentation",
                ["readme.md"] = "Documentation",
                ["readme.txt"] = "Documentation",
                ["CONTRIBUTING.md"] = "Contribution Guide",
                ["CHANGELOG.md"] = "Changelog",
                ["LICENSE"] = "License",
                ["LICENSE.txt"] = "License",
                [".gitignore"] = "Git Ignore",
                ["Dockerfile"] = "Docker Configuration",
                ["docker-compose.yml"] = "Docker Compose"
            };

            return typeMap.GetValueOrDefault(fileName, "Important File");
        }

        private long CalculateDirectorySize(string path)
        {
            try
            {
                var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                return files.Sum(file => new FileInfo(file).Length);
            }
            catch
            {
                return 0;
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double size = bytes;
            
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            
            return $"{size:0.##} {sizes[order]}";
        }
    }

    // Models para análise de repositório
    public class RepositoryAnalysis
    {
        public string RepositoryUrl { get; set; } = string.Empty;
        public string RepositoryName { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public string? ErrorMessage { get; set; }
        public DateTime AnalyzedAt { get; set; }
        public int TotalFiles { get; set; }
        public long TotalSize { get; set; }
        public Dictionary<string, int> FileExtensions { get; set; } = new();
        public List<string> DirectoryStructure { get; set; } = new();
        public List<LanguageInfo> ProgrammingLanguages { get; set; } = new();
        public List<ConfigurationFile> ConfigurationFiles { get; set; } = new();
        public List<ImportantFile> ImportantFiles { get; set; } = new();
        public List<AnalysisStep> AnalysisSteps { get; set; } = new();
        public string? FinalReport { get; set; }
    }

    public class LanguageInfo
    {
        public string Name { get; set; } = string.Empty;
        public int FileCount { get; set; }
        public List<string> FileExtensions { get; set; } = new();
    }

    public class ConfigurationFile
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
    }

    public class ImportantFile
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
    }

    public class AnalysisStep
    {
        public string StepName { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<string> Logs { get; set; } = new();
    }
}
