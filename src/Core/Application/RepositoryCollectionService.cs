using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using TutorCopiloto.Domain.Entities;
using TutorCopiloto.Services.Dto;

namespace TutorCopiloto.Services
{
    /// <summary>
    /// Configurações para o serviço de coleta de repositórios
    /// </summary>
    public class RepositoryCollectionOptions
    {
        public string GitHubApiKey { get; set; } = string.Empty;
        public string GitHubApiBaseUrl { get; set; } = "https://api.github.com";
        public int MaxRepositoriesPerRequest { get; set; } = 100;
        public int RequestDelayMs { get; set; } = 1000;
        public List<string> TargetLanguages { get; set; } = new() { "csharp", "python", "javascript", "go", "rust" };
        public int MinStars { get; set; } = 10;
        public bool EnableAutoCollection { get; set; } = true;
    }

    /// <summary>
    /// Serviço para coleta de repositórios do GitHub
    /// </summary>
    public class RepositoryCollectionService
    {
        private readonly HttpClient _httpClient;
        private readonly RepositoryCollectionOptions _options;
        private readonly ILogger<RepositoryCollectionService> _logger;

        public RepositoryCollectionService(
            HttpClient httpClient,
            IOptions<RepositoryCollectionOptions> options,
            ILogger<RepositoryCollectionService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;

            // Configurar headers da GitHub API
            _httpClient.BaseAddress = new Uri(_options.GitHubApiBaseUrl);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "TutorCopiloto-RepositoryAnalyzer/1.0");

            if (!string.IsNullOrEmpty(_options.GitHubApiKey))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _options.GitHubApiKey);
            }
        }

        /// <summary>
        /// Busca repositórios no GitHub com query genérica
        /// </summary>
        public async Task<GitHubSearchResponse> SearchRepositoriesAsync(
            string query, string language = null, string sort = "stars", int page = 1, int perPage = 20)
        {
            try
            {
                var searchQuery = query;
                if (!string.IsNullOrEmpty(language))
                {
                    searchQuery += $" language:{language}";
                }

                var url = $"/search/repositories?q={Uri.EscapeDataString(searchQuery)}&sort={sort}&order=desc&page={page}&per_page={perPage}";

                _logger.LogInformation("Buscando repositórios: {Query}", searchQuery);

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var searchResult = JsonSerializer.Deserialize<GitHubSearchResponse>(jsonResponse);

                    _logger.LogInformation("Encontrados {Count} repositórios", searchResult?.TotalCount ?? 0);

                    return searchResult ?? new GitHubSearchResponse();
                }
                else
                {
                    _logger.LogWarning("Erro na busca de repositórios: {StatusCode}", response.StatusCode);
                    return new GitHubSearchResponse();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar repositórios: {Query}", query);
                return new GitHubSearchResponse();
            }
        }

        /// <summary>
        /// Busca repositórios populares por linguagem
        /// </summary>
        public async Task<List<GitHubRepositoryDto>> SearchRepositoriesAsync(
            string language,
            int minStars = 10,
            int maxResults = 50,
            string sort = "stars",
            string order = "desc")
        {
            var repositories = new List<GitHubRepositoryDto>();

            try
            {
                var query = $"language:{language} stars:>={minStars}";
                var url = $"/search/repositories?q={Uri.EscapeDataString(query)}&sort={sort}&order={order}&per_page={maxResults}";

                _logger.LogInformation("Buscando repositórios: {Query}", query);

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var searchResult = JsonSerializer.Deserialize<GitHubSearchResponse>(jsonResponse);

                    if (searchResult?.Items != null)
                    {
                        repositories.AddRange(searchResult.Items);
                        _logger.LogInformation("Encontrados {Count} repositórios para {Language}",
                            searchResult.Items.Count, language);
                    }
                }
                else
                {
                    _logger.LogWarning("Erro na busca de repositórios: {StatusCode}",
                        response.StatusCode);
                }

                // Respeitar rate limiting
                await Task.Delay(_options.RequestDelayMs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar repositórios para linguagem {Language}", language);
            }

            return repositories;
        }

        /// <summary>
        /// Busca repositórios por tópicos específicos
        /// </summary>
        public async Task<List<GitHubRepositoryDto>> SearchByTopicsAsync(
            List<string> topics,
            int minStars = 10,
            int maxResults = 50)
        {
            var repositories = new List<GitHubRepositoryDto>();

            try
            {
                var topicQuery = string.Join(" ", topics.Select(t => $"topic:{t}"));
                var query = $"{topicQuery} stars:>={minStars}";
                var url = $"/search/repositories?q={Uri.EscapeDataString(query)}&sort=stars&order=desc&per_page={maxResults}";

                _logger.LogInformation("Buscando repositórios por tópicos: {Topics}", string.Join(", ", topics));

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var searchResult = JsonSerializer.Deserialize<GitHubSearchResponse>(jsonResponse);

                    if (searchResult?.Items != null)
                    {
                        repositories.AddRange(searchResult.Items);
                        _logger.LogInformation("Encontrados {Count} repositórios por tópicos",
                            searchResult.Items.Count);
                    }
                }
                else
                {
                    _logger.LogWarning("Erro na busca por tópicos: {StatusCode}", response.StatusCode);
                }

                await Task.Delay(_options.RequestDelayMs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar repositórios por tópicos {Topics}",
                    string.Join(", ", topics));
            }

            return repositories;
        }

        /// <summary>
        /// Obtém detalhes completos de um repositório específico
        /// </summary>
        public async Task<GitHubRepositoryDto?> GetRepositoryDetailsAsync(string owner, string name)
        {
            try
            {
                var url = $"/repos/{owner}/{name}";
                _logger.LogInformation("Obtendo detalhes do repositório: {Owner}/{Name}", owner, name);

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var repository = JsonSerializer.Deserialize<GitHubRepositoryDto>(jsonResponse);

                    await Task.Delay(_options.RequestDelayMs);
                    return repository;
                }
                else
                {
                    _logger.LogWarning("Erro ao obter detalhes do repositório {Owner}/{Name}: {StatusCode}",
                        owner, name, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter detalhes do repositório {Owner}/{Name}", owner, name);
            }

            return null;
        }

        /// <summary>
        /// Busca repositórios em lote para múltiplas linguagens
        /// </summary>
        public async Task<List<GitHubRepositoryDto>> CollectRepositoriesBatchAsync(
            int maxPerLanguage = 20)
        {
            var allRepositories = new List<GitHubRepositoryDto>();

            foreach (var language in _options.TargetLanguages)
            {
                try
                {
                    var repositories = await SearchRepositoriesAsync(
                        language,
                        _options.MinStars,
                        maxPerLanguage);

                    allRepositories.AddRange(repositories);

                    _logger.LogInformation("Coletados {Count} repositórios para {Language}",
                        repositories.Count, language);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao coletar repositórios para {Language}", language);
                }
            }

            // Remover duplicatas
            var uniqueRepositories = allRepositories
                .GroupBy(r => r.Id)
                .Select(g => g.First())
                .ToList();

            _logger.LogInformation("Coleta concluída: {Total} repositórios únicos coletados",
                uniqueRepositories.Count);

            return uniqueRepositories;
        }
    }

    // DTOs para resposta da GitHub API
    public class GitHubSearchResponse
    {
        public int TotalCount { get; set; }
        public bool IncompleteResults { get; set; }
        public List<GitHubRepositoryDto> Items { get; set; } = new();
    }

    public class GitHubRepositoryDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string HtmlUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public int StargazersCount { get; set; }
        public int ForksCount { get; set; }
        public int OpenIssuesCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime PushedAt { get; set; }
        public int Size { get; set; }
        public GitHubOwnerDto Owner { get; set; } = new();
        public bool Archived { get; set; }
        public bool Disabled { get; set; }
        public List<string> Topics { get; set; } = new();
    }

    public class GitHubOwnerDto
    {
        public long Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public string HtmlUrl { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }
}
