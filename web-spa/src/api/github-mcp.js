// GitHub MCP Server API client
const API_BASE = '/api/github-mcp';

/**
 * Lista todas as ferramentas disponíveis no MCP Server
 */
export const listMcpTools = async () => {
  try {
    const response = await fetch(`${API_BASE}/tools`);
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    return await response.json();
  } catch (error) {
    console.error('Erro ao listar ferramentas MCP:', error);
    throw error;
  }
};

/**
 * Executa uma ferramenta específica do MCP Server
 */
export const executeMcpTool = async (toolName, parameters = {}) => {
  try {
    const response = await fetch(`${API_BASE}/execute-tool`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        toolName,
        parameters
      })
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    return await response.json();
  } catch (error) {
    console.error(`Erro ao executar ferramenta MCP ${toolName}:`, error);
    throw error;
  }
};

/**
 * Analisa um repositório GitHub
 */
export const analyzeRepository = async (owner, repo, branch = 'main') => {
  try {
    const response = await fetch(`${API_BASE}/analyze-repository`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        owner,
        repo,
        branch
      })
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    return await response.json();
  } catch (error) {
    console.error(`Erro ao analisar repositório ${owner}/${repo}:`, error);
    throw error;
  }
};

/**
 * Pesquisa em um repositório GitHub
 */
export const queryRepository = async (owner, repo, query, context = '') => {
  try {
    const response = await fetch(`${API_BASE}/query-repository`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        owner,
        repo,
        query,
        context
      })
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    return await response.json();
  } catch (error) {
    console.error(`Erro ao pesquisar repositório ${owner}/${repo}:`, error);
    throw error;
  }
};

/**
 * Análise completa e pesquisa combinada
 */
export const analyzeAndQueryRepository = async (owner, repo, branch = 'main', query = '', context = '') => {
  try {
    const response = await fetch(`${API_BASE}/analyze-and-query`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        owner,
        repo,
        branch,
        query,
        context
      })
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    return await response.json();
  } catch (error) {
    console.error(`Erro na análise completa do repositório ${owner}/${repo}:`, error);
    throw error;
  }
};

/**
 * Busca código em um repositório
 */
export const searchCode = async (query, repo = '', owner = '') => {
  const parameters = { q: query };
  if (repo && owner) {
    parameters.q = `${query} repo:${owner}/${repo}`;
  }

  return await executeMcpTool('search_code', parameters);
};

/**
 * Obtém informações de um repositório
 */
export const getRepositoryInfo = async (owner, repo) => {
  return await executeMcpTool('get_repository', { owner, repo });
};

/**
 * Lista issues de um repositório
 */
export const listRepositoryIssues = async (owner, repo, state = 'open', perPage = 20) => {
  return await executeMcpTool('list_issues', {
    owner,
    repo,
    state,
    per_page: perPage
  });
};

/**
 * Lista pull requests de um repositório
 */
export const listRepositoryPullRequests = async (owner, repo, state = 'open', perPage = 10) => {
  return await executeMcpTool('list_pull_requests', {
    owner,
    repo,
    state,
    per_page: perPage
  });
};

/**
 * Lista commits de um repositório
 */
export const listRepositoryCommits = async (owner, repo, perPage = 10) => {
  return await executeMcpTool('list_commits', {
    owner,
    repo,
    per_page: perPage
  });
};

/**
 * Lista branches de um repositório
 */
export const listRepositoryBranches = async (owner, repo) => {
  return await executeMcpTool('list_branches', { owner, repo });
};

/**
 * Obtém conteúdo de arquivo ou diretório
 */
export const getFileContents = async (owner, repo, path = '', ref = '') => {
  const parameters = { owner, repo };
  if (path) parameters.path = path;
  if (ref) parameters.ref = ref;

  return await executeMcpTool('get_file_contents', parameters);
};

/**
 * Lista workflows do GitHub Actions
 */
export const listWorkflows = async (owner, repo) => {
  return await executeMcpTool('list_workflows', { owner, repo });
};

/**
 * Lista alertas de segurança
 */
export const listSecurityAlerts = async (owner, repo) => {
  return await executeMcpTool('list_code_scanning_alerts', { owner, repo });
};

/**
 * Lista alertas do Dependabot
 */
export const listDependabotAlerts = async (owner, repo) => {
  return await executeMcpTool('list_dependabot_alerts', { owner, repo });
};
