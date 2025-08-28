import React, { useState, useEffect } from 'react';
import {
  listMcpTools,
  analyzeRepository,
  queryRepository,
  analyzeAndQueryRepository,
  searchCode,
  getRepositoryInfo,
  listRepositoryIssues,
  listRepositoryPullRequests,
  listRepositoryCommits,
  listRepositoryBranches,
  getFileContents,
  listWorkflows,
  listSecurityAlerts,
  listDependabotAlerts
} from '../api/github-mcp';

export default function GitHubSection() {
  const [ngrokStatus, setNgrokStatus] = useState(null);
  const [webhookUrl, setWebhookUrl] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [repositories, setRepositories] = useState([]);
  const [selectedRepo, setSelectedRepo] = useState('');
  const [webhookSecret, setWebhookSecret] = useState('');
  const [cloneUrl, setCloneUrl] = useState('');
  const [cloneBranch, setCloneBranch] = useState('main');
  const [isCloning, setIsCloning] = useState(false);
  const [selectedRepository, setSelectedRepository] = useState('');
  const [repoStatus, setRepoStatus] = useState(null);
  const [repoLog, setRepoLog] = useState([]);
  const [branches, setBranches] = useState([]);
  const [currentBranch, setCurrentBranch] = useState('');

  // Estados para GitHub MCP Server
  const [mcpTools, setMcpTools] = useState([]);
  const [mcpAnalysis, setMcpAnalysis] = useState(null);
  const [mcpQuery, setMcpQuery] = useState(null);
  const [mcpSearchResults, setMcpSearchResults] = useState(null);
  const [mcpRepositoryInfo, setMcpRepositoryInfo] = useState(null);
  const [mcpIssues, setMcpIssues] = useState([]);
  const [mcpPullRequests, setMcpPullRequests] = useState([]);
  const [mcpCommits, setMcpCommits] = useState([]);
  const [mcpBranches, setMcpBranches] = useState([]);
  const [mcpWorkflows, setMcpWorkflows] = useState([]);
  const [mcpSecurityAlerts, setMcpSecurityAlerts] = useState([]);
  const [mcpDependabotAlerts, setMcpDependabotAlerts] = useState([]);

  // Estados para formulários MCP
  const [analysisOwner, setAnalysisOwner] = useState('');
  const [analysisRepo, setAnalysisRepo] = useState('');
  const [analysisBranch, setAnalysisBranch] = useState('main');
  const [searchQuery, setSearchQuery] = useState('');
  const [searchRepo, setSearchRepo] = useState('');
  const [searchOwner, setSearchOwner] = useState('');
  const [mcpLoading, setMcpLoading] = useState(false);
  const [mcpError, setMcpError] = useState(null);

  useEffect(() => {
    checkNgrokStatus();
    loadRepositories();
  }, []);

  const checkNgrokStatus = async () => {
    try {
      const response = await fetch('/api/ngrok/status');
      const data = await response.json();
      setNgrokStatus(data);
      if (data.publicUrl) {
        setWebhookUrl(`${data.publicUrl}/api/webhook/github`);
      }
    } catch (error) {
      console.error('Erro ao verificar status do ngrok:', error);
      setNgrokStatus({ isRunning: false, error: 'Não foi possível conectar ao ngrok' });
    }
  };

  const loadRepositories = async () => {
    try {
      const response = await fetch('/api/git/repositories');
      const data = await response.json();

      if (data.success) {
        setRepositories(data.repositories);
      }
    } catch (error) {
      console.error('Erro ao carregar repositórios:', error);
      // Fallback para repositórios mockados
      const mockRepos = [
        { name: 'ASP.NETCore', fullName: 'dronreef2/ASP.NETCore', url: 'https://github.com/dronreef2/ASP.NETCore' },
        { name: 'react-app', fullName: 'dronreef2/react-app', url: 'https://github.com/dronreef2/react-app' },
        { name: 'node-backend', fullName: 'dronreef2/node-backend', url: 'https://github.com/dronreef2/node-backend' }
      ];
      setRepositories(mockRepos);
    }
  };

  const startNgrok = async () => {
    setIsLoading(true);
    try {
      const response = await fetch('/api/ngrok/start', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ port: 5000 })
      });
      const data = await response.json();

      if (response.ok) {
        setNgrokStatus(data);
        setWebhookUrl(data.webhookUrl);
      } else {
        throw new Error(data.error);
      }
    } catch (error) {
      console.error('Erro ao iniciar ngrok:', error);
      alert('Erro ao iniciar ngrok: ' + error.message);
    } finally {
      setIsLoading(false);
    }
  };

  const stopNgrok = async () => {
    setIsLoading(true);
    try {
      await fetch('/api/ngrok/stop', { method: 'POST' });
      setNgrokStatus({ isRunning: false });
      setWebhookUrl('');
    } catch (error) {
      console.error('Erro ao parar ngrok:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const copyToClipboard = (text) => {
    navigator.clipboard.writeText(text);
    alert('Copiado para a área de transferência!');
  };

  const generateWebhookSecret = () => {
    const secret = 'gh_webhook_' + Math.random().toString(36).substring(2, 15);
    setWebhookSecret(secret);
  };

  const setupWebhook = async () => {
    if (!selectedRepo || !webhookSecret) {
      alert('Selecione um repositório e gere um secret');
      return;
    }

    try {
      // Simular configuração de webhook
      alert(`Webhook configurado para ${selectedRepo}!\n\nURL: ${webhookUrl}\nSecret: ${webhookSecret}\n\nCopie estes valores para as configurações do repositório no GitHub.`);
    } catch (error) {
      console.error('Erro ao configurar webhook:', error);
    }
  };

  const cloneRepository = async () => {
    if (!cloneUrl.trim()) {
      alert('Digite a URL do repositório para clonar');
      return;
    }

    setIsCloning(true);
    try {
      const response = await fetch('/api/git/clone', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          repositoryUrl: cloneUrl,
          branch: cloneBranch || null
        })
      });

      const data = await response.json();

      if (data.success) {
        alert(`Repositório clonado com sucesso!\n\nCaminho: ${data.repositoryPath}`);
        setCloneUrl('');
        setCloneBranch('main');
        loadRepositories();
      } else {
        alert(`Erro ao clonar repositório: ${data.message}`);
      }
    } catch (error) {
      console.error('Erro ao clonar repositório:', error);
      alert('Erro interno ao clonar repositório');
    } finally {
      setIsCloning(false);
    }
  };

  const pullRepository = async () => {
    if (!selectedRepository) {
      alert('Selecione um repositório primeiro');
      return;
    }

    setIsLoading(true);
    try {
      const response = await fetch('/api/git/pull', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          repositoryPath: selectedRepository
        })
      });

      const data = await response.json();

      if (data.success) {
        alert(`Pull executado com sucesso!\n\n${data.message}`);
        loadRepositoryStatus();
        loadRepositoryLog();
      } else {
        alert(`Erro ao executar pull: ${data.message}`);
      }
    } catch (error) {
      console.error('Erro ao executar pull:', error);
      alert('Erro interno ao executar pull');
    } finally {
      setIsLoading(false);
    }
  };

  const loadRepositoryStatus = async () => {
    if (!selectedRepository) return;

    try {
      const response = await fetch(`/api/git/status?repositoryPath=${encodeURIComponent(selectedRepository)}`);
      const data = await response.json();

      if (data.success) {
        setRepoStatus(data);
      }
    } catch (error) {
      console.error('Erro ao carregar status:', error);
    }
  };

  const loadRepositoryLog = async () => {
    if (!selectedRepository) return;

    try {
      const response = await fetch(`/api/git/log?repositoryPath=${encodeURIComponent(selectedRepository)}&maxEntries=10`);
      const data = await response.json();

      if (data.success) {
        setRepoLog(data.commits);
      }
    } catch (error) {
      console.error('Erro ao carregar log:', error);
    }
  };

  const loadBranches = async () => {
    if (!selectedRepository) return;

    try {
      const response = await fetch(`/api/git/branches?repositoryPath=${encodeURIComponent(selectedRepository)}`);
      const data = await response.json();

      if (data.success) {
        setBranches(data.branches);
        setCurrentBranch(data.currentBranch);
      }
    } catch (error) {
      console.error('Erro ao carregar branches:', error);
    }
  };

  const checkoutBranch = async (branch) => {
    if (!selectedRepository) return;

    try {
      const response = await fetch('/api/git/checkout', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          repositoryPath: selectedRepository,
          branch: branch
        })
      });

      const data = await response.json();

      if (data.success) {
        alert(`Branch alterado para: ${branch}`);
        setCurrentBranch(branch);
        loadRepositoryStatus();
      } else {
        alert(`Erro ao alterar branch: ${data.message}`);
      }
    } catch (error) {
      console.error('Erro ao fazer checkout:', error);
      alert('Erro interno ao alterar branch');
    }
  };

  const handleRepositorySelect = (repoPath) => {
    setSelectedRepository(repoPath);
    setTimeout(() => {
      loadRepositoryStatus();
      loadRepositoryLog();
      loadBranches();
    }, 100);
  };

  // Funções do GitHub MCP Server
  const loadMcpTools = async () => {
    setMcpLoading(true);
    setMcpError(null);
    try {
      const result = await listMcpTools();
      setMcpTools(result.tools || []);
    } catch (error) {
      setMcpError('Erro ao carregar ferramentas MCP: ' + error.message);
    } finally {
      setMcpLoading(false);
    }
  };

  const handleAnalyzeRepository = async () => {
    if (!analysisOwner || !analysisRepo) {
      setMcpError('Preencha o proprietário e nome do repositório');
      return;
    }

    setMcpLoading(true);
    setMcpError(null);
    try {
      const result = await analyzeRepository(analysisOwner, analysisRepo, analysisBranch);
      setMcpAnalysis(result);
    } catch (error) {
      setMcpError('Erro na análise do repositório: ' + error.message);
    } finally {
      setMcpLoading(false);
    }
  };

  const handleQueryRepository = async () => {
    if (!analysisOwner || !analysisRepo || !searchQuery) {
      setMcpError('Preencha todos os campos obrigatórios');
      return;
    }

    setMcpLoading(true);
    setMcpError(null);
    try {
      const result = await queryRepository(analysisOwner, analysisRepo, searchQuery);
      setMcpQuery(result);
    } catch (error) {
      setMcpError('Erro na pesquisa do repositório: ' + error.message);
    } finally {
      setMcpLoading(false);
    }
  };

  const handleAnalyzeAndQuery = async () => {
    if (!analysisOwner || !analysisRepo) {
      setMcpError('Preencha o proprietário e nome do repositório');
      return;
    }

    setMcpLoading(true);
    setMcpError(null);
    try {
      const result = await analyzeAndQueryRepository(
        analysisOwner,
        analysisRepo,
        analysisBranch,
        searchQuery || undefined
      );
      setMcpAnalysis(result.analysis);
      setMcpQuery(result.queryResult);
    } catch (error) {
      setMcpError('Erro na análise completa: ' + error.message);
    } finally {
      setMcpLoading(false);
    }
  };

  const handleSearchCode = async () => {
    if (!searchQuery) {
      setMcpError('Digite uma consulta de pesquisa');
      return;
    }

    setMcpLoading(true);
    setMcpError(null);
    try {
      const result = await searchCode(searchQuery, searchRepo, searchOwner);
      setMcpSearchResults(result);
    } catch (error) {
      setMcpError('Erro na pesquisa de código: ' + error.message);
    } finally {
      setMcpLoading(false);
    }
  };

  const handleGetRepositoryInfo = async () => {
    if (!analysisOwner || !analysisRepo) {
      setMcpError('Preencha o proprietário e nome do repositório');
      return;
    }

    setMcpLoading(true);
    setMcpError(null);
    try {
      const result = await getRepositoryInfo(analysisOwner, analysisRepo);
      setMcpRepositoryInfo(result);
    } catch (error) {
      setMcpError('Erro ao obter informações do repositório: ' + error.message);
    } finally {
      setMcpLoading(false);
    }
  };

  const handleLoadIssues = async () => {
    if (!analysisOwner || !analysisRepo) {
      setMcpError('Preencha o proprietário e nome do repositório');
      return;
    }

    setMcpLoading(true);
    setMcpError(null);
    try {
      const result = await listRepositoryIssues(analysisOwner, analysisRepo);
      setMcpIssues(result?.Result?.items || []);
    } catch (error) {
      setMcpError('Erro ao carregar issues: ' + error.message);
    } finally {
      setMcpLoading(false);
    }
  };

  const handleLoadPullRequests = async () => {
    if (!analysisOwner || !analysisRepo) {
      setMcpError('Preencha o proprietário e nome do repositório');
      return;
    }

    setMcpLoading(true);
    setMcpError(null);
    try {
      const result = await listRepositoryPullRequests(analysisOwner, analysisRepo);
      setMcpPullRequests(result?.Result?.items || []);
    } catch (error) {
      setMcpError('Erro ao carregar pull requests: ' + error.message);
    } finally {
      setMcpLoading(false);
    }
  };

  const handleLoadCommits = async () => {
    if (!analysisOwner || !analysisRepo) {
      setMcpError('Preencha o proprietário e nome do repositório');
      return;
    }

    setMcpLoading(true);
    setMcpError(null);
    try {
      const result = await listRepositoryCommits(analysisOwner, analysisRepo);
      setMcpCommits(result?.Result?.items || []);
    } catch (error) {
      setMcpError('Erro ao carregar commits: ' + error.message);
    } finally {
      setMcpLoading(false);
    }
  };

  const handleLoadBranches = async () => {
    if (!analysisOwner || !analysisRepo) {
      setMcpError('Preencha o proprietário e nome do repositório');
      return;
    }

    setMcpLoading(true);
    setMcpError(null);
    try {
      const result = await listRepositoryBranches(analysisOwner, analysisRepo);
      setMcpBranches(result?.Result?.items || []);
    } catch (error) {
      setMcpError('Erro ao carregar branches: ' + error.message);
    } finally {
      setMcpLoading(false);
    }
  };

  const handleLoadWorkflows = async () => {
    if (!analysisOwner || !analysisRepo) {
      setMcpError('Preencha o proprietário e nome do repositório');
      return;
    }

    setMcpLoading(true);
    setMcpError(null);
    try {
      const result = await listWorkflows(analysisOwner, analysisRepo);
      setMcpWorkflows(result?.Result?.workflows || []);
    } catch (error) {
      setMcpError('Erro ao carregar workflows: ' + error.message);
    } finally {
      setMcpLoading(false);
    }
  };

  const handleLoadSecurityAlerts = async () => {
    if (!analysisOwner || !analysisRepo) {
      setMcpError('Preencha o proprietário e nome do repositório');
      return;
    }

    setMcpLoading(true);
    setMcpError(null);
    try {
      const result = await listSecurityAlerts(analysisOwner, analysisRepo);
      setMcpSecurityAlerts(result?.Result?.items || []);
    } catch (error) {
      setMcpError('Erro ao carregar alertas de segurança: ' + error.message);
    } finally {
      setMcpLoading(false);
    }
  };

  const handleLoadDependabotAlerts = async () => {
    if (!analysisOwner || !analysisRepo) {
      setMcpError('Preencha o proprietário e nome do repositório');
      return;
    }

    setMcpLoading(true);
    setMcpError(null);
    try {
      const result = await listDependabotAlerts(analysisOwner, analysisRepo);
      setMcpDependabotAlerts(result?.Result?.items || []);
    } catch (error) {
      setMcpError('Erro ao carregar alertas do Dependabot: ' + error.message);
    } finally {
      setMcpLoading(false);
    }
  };

  // Carregar ferramentas MCP ao montar o componente
  useEffect(() => {
    loadMcpTools();
  }, []);

  return (
    <div className="grid">
      {/* Status do ngrok */}
      <div className="card">
        <div className="card-header">
          <h3 className="card-title">🌐 Túnel ngrok</h3>
          <p className="card-description">Gerencie o túnel público para webhooks</p>
        </div>

        <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', marginBottom: '1rem' }}>
          <div style={{
            padding: '0.5rem 1rem',
            borderRadius: '20px',
            background: ngrokStatus?.isRunning ? '#d1fae5' : '#fee2e2',
            color: ngrokStatus?.isRunning ? '#065f46' : '#dc2626',
            fontWeight: '500'
          }}>
            {ngrokStatus?.isRunning ? '🟢 Online' : '🔴 Offline'}
          </div>

          <div style={{ flex: 1 }}>
            {ngrokStatus?.publicUrl && (
              <div style={{ fontSize: '0.9rem', color: '#6b7280' }}>
                URL Pública: <code>{ngrokStatus.publicUrl}</code>
                <button
                  onClick={() => copyToClipboard(ngrokStatus.publicUrl)}
                  style={{ marginLeft: '0.5rem', fontSize: '0.8rem' }}
                  className="btn btn-secondary"
                >
                  📋
                </button>
              </div>
            )}
          </div>
        </div>

        <div style={{ display: 'flex', gap: '1rem' }}>
          {!ngrokStatus?.isRunning ? (
            <button
              onClick={startNgrok}
              disabled={isLoading}
              className="btn btn-success"
            >
              {isLoading ? '⏳ Iniciando...' : '🚀 Iniciar ngrok'}
            </button>
          ) : (
            <button
              onClick={stopNgrok}
              disabled={isLoading}
              className="btn btn-secondary"
            >
              {isLoading ? '⏳ Parando...' : '🛑 Parar ngrok'}
            </button>
          )}

          <button
            onClick={checkNgrokStatus}
            className="btn btn-secondary"
          >
            🔄 Atualizar Status
          </button>
        </div>
      </div>

      {/* Configuração de Webhook */}
      <div className="card">
        <div className="card-header">
          <h3 className="card-title">🔗 Configuração de Webhook</h3>
          <p className="card-description">Configure webhooks para receber notificações de push</p>
        </div>

        <div className="grid grid-2">
          <div>
            <label className="form-label">Repositório</label>
            <select
              value={selectedRepo}
              onChange={(e) => setSelectedRepo(e.target.value)}
              className="form-input"
            >
              <option value="">Selecione um repositório...</option>
              {repositories.map((repo) => (
                <option key={repo.fullName || repo.name} value={repo.fullName || repo.name}>
                  {repo.fullName || repo.name}
                </option>
              ))}
            </select>
          </div>

          <div>
            <label className="form-label">Secret do Webhook</label>
            <div style={{ display: 'flex', gap: '0.5rem' }}>
              <input
                type="text"
                value={webhookSecret}
                onChange={(e) => setWebhookSecret(e.target.value)}
                placeholder="Secret para validar webhook"
                className="form-input"
              />
              <button
                onClick={generateWebhookSecret}
                className="btn btn-secondary"
              >
                🎲 Gerar
              </button>
            </div>
          </div>
        </div>

        {webhookUrl && (
          <div style={{ marginTop: '1rem', padding: '1rem', background: '#f8fafc', borderRadius: '8px' }}>
            <div style={{ fontWeight: '600', marginBottom: '0.5rem', color: '#374151' }}>
              📡 URL do Webhook:
            </div>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
              <code style={{ flex: 1, background: 'white', padding: '0.5rem', borderRadius: '4px' }}>
                {webhookUrl}
              </code>
              <button
                onClick={() => copyToClipboard(webhookUrl)}
                className="btn btn-secondary"
              >
                📋
              </button>
            </div>
          </div>
        )}

        <button
          onClick={setupWebhook}
          disabled={!selectedRepo || !webhookSecret || !ngrokStatus?.isRunning}
          className="btn btn-primary"
          style={{ marginTop: '1rem' }}
        >
          ⚙️ Configurar Webhook
        </button>
      </div>

      {/* Clone de Repositório */}
      <div className="card">
        <div className="card-header">
          <h3 className="card-title">📥 Clonar Repositório</h3>
          <p className="card-description">Clone um repositório Git para análise local</p>
        </div>

        <div style={{ marginBottom: '1rem' }}>
          <label className="form-label">URL do Repositório</label>
          <input
            type="url"
            value={cloneUrl}
            onChange={(e) => setCloneUrl(e.target.value)}
            placeholder="https://github.com/user/repo.git"
            className="form-input"
          />
        </div>

        <div style={{ marginBottom: '1rem' }}>
          <label className="form-label">Branch (opcional)</label>
          <input
            type="text"
            value={cloneBranch}
            onChange={(e) => setCloneBranch(e.target.value)}
            placeholder="main"
            className="form-input"
          />
        </div>

        <button
          onClick={cloneRepository}
          disabled={isCloning || !cloneUrl.trim()}
          className="btn btn-primary"
        >
          {isCloning ? (
            <div className="loading">
              <div className="spinner"></div>
              Clonando...
            </div>
          ) : (
            '📥 Clonar Repositório'
          )}
        </button>
      </div>

      {/* Gerenciamento de Repositórios */}
      <div className="card">
        <div className="card-header">
          <h3 className="card-title">📂 Repositórios Locais</h3>
          <p className="card-description">Gerencie repositórios clonados localmente</p>
        </div>

        <div style={{ marginBottom: '1rem' }}>
          <label className="form-label">Repositório</label>
          <select
            value={selectedRepository}
            onChange={(e) => handleRepositorySelect(e.target.value)}
            className="form-input"
          >
            <option value="">Selecione um repositório...</option>
            {repositories.map((repo) => (
              <option key={repo.path || repo.name} value={repo.path || repo.name}>
                {repo.name}
              </option>
            ))}
          </select>
        </div>

        {selectedRepository && (
          <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap' }}>
            <button
              onClick={pullRepository}
              disabled={isLoading}
              className="btn btn-primary"
            >
              {isLoading ? '🔄...' : '⬇️ Pull'}
            </button>

            <button
              onClick={loadRepositoryStatus}
              className="btn btn-secondary"
            >
              📊 Status
            </button>

            <button
              onClick={loadRepositoryLog}
              className="btn btn-secondary"
            >
              📋 Log
            </button>

            <button
              onClick={loadBranches}
              className="btn btn-secondary"
            >
              🌿 Branches
            </button>
          </div>
        )}
      </div>

      {/* Status do Repositório */}
      {repoStatus && (
        <div className="card">
          <div className="card-header">
            <h3 className="card-title">📊 Status do Repositório</h3>
            <p className="card-description">Status atual dos arquivos no repositório</p>
          </div>

          <div style={{ marginBottom: '1rem' }}>
            <div style={{
              padding: '0.75rem',
              borderRadius: '8px',
              background: repoStatus.isClean ? '#f0fdf4' : '#fef3c7',
              color: repoStatus.isClean ? '#166534' : '#92400e',
              fontWeight: '500'
            }}>
              {repoStatus.message}
            </div>
          </div>

          {repoStatus.modifiedFiles && repoStatus.modifiedFiles.length > 0 && (
            <div style={{ marginBottom: '1rem' }}>
              <h4 style={{ margin: '0 0 0.5rem 0', color: '#374151' }}>📝 Arquivos Modificados</h4>
              <ul style={{ color: '#6b7280', margin: 0 }}>
                {repoStatus.modifiedFiles.map((file, index) => (
                  <li key={index}>• {file}</li>
                ))}
              </ul>
            </div>
          )}

          {repoStatus.untrackedFiles && repoStatus.untrackedFiles.length > 0 && (
            <div style={{ marginBottom: '1rem' }}>
              <h4 style={{ margin: '0 0 0.5rem 0', color: '#374151' }}>📄 Arquivos Não Rastreados</h4>
              <ul style={{ color: '#6b7280', margin: 0 }}>
                {repoStatus.untrackedFiles.map((file, index) => (
                  <li key={index}>• {file}</li>
                ))}
              </ul>
            </div>
          )}
        </div>
      )}

      {/* Log de Commits */}
      {repoLog && repoLog.length > 0 && (
        <div className="card">
          <div className="card-header">
            <h3 className="card-title">📋 Histórico de Commits</h3>
            <p className="card-description">Últimos commits do repositório</p>
          </div>

          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
            {repoLog.map((commit, index) => (
              <div key={index} style={{
                padding: '1rem',
                border: '1px solid #e5e7eb',
                borderRadius: '8px',
                background: '#f9fafb'
              }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '0.5rem' }}>
                  <div>
                    <div style={{ fontWeight: '600', marginBottom: '0.25rem', color: '#374151' }}>
                      {commit.hash.substring(0, 8)}
                    </div>
                    <div style={{ fontSize: '0.9rem', color: '#6b7280', marginBottom: '0.5rem' }}>
                      {commit.message}
                    </div>
                    <div style={{ fontSize: '0.8rem', color: '#9ca3af' }}>
                      👤 {commit.authorName} • 📅 {commit.date}
                    </div>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Branches */}
      {branches && branches.length > 0 && (
        <div className="card">
          <div className="card-header">
            <h3 className="card-title">🌿 Branches</h3>
            <p className="card-description">Branches disponíveis no repositório</p>
          </div>

          <div style={{ marginBottom: '1rem' }}>
            <div style={{ fontSize: '0.9rem', color: '#6b7280', marginBottom: '0.5rem' }}>
              Branch atual: <strong>{currentBranch}</strong>
            </div>
          </div>

          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
            {branches.map((branch, index) => (
              <div key={index} style={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                padding: '0.75rem',
                border: '1px solid #e5e7eb',
                borderRadius: '8px',
                background: branch === currentBranch ? '#f0fdf4' : '#f9fafb'
              }}>
                <span style={{ fontWeight: branch === currentBranch ? '600' : '400' }}>
                  {branch}
                  {branch === currentBranch && ' (atual)'}
                </span>

                {branch !== currentBranch && (
                  <button
                    onClick={() => checkoutBranch(branch)}
                    className="btn btn-secondary"
                    style={{ fontSize: '0.8rem', padding: '0.25rem 0.75rem' }}
                  >
                    🔄 Checkout
                  </button>
                )}
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Instruções de Configuração */}
      <div className="card">
        <div className="card-header">
          <h3 className="card-title">🤖 GitHub MCP Server - Análise Avançada</h3>
          <p className="card-description">Utilize o poder do GitHub MCP Server para análise profunda de repositórios</p>
        </div>

        {/* Ferramentas Disponíveis */}
        <div style={{ marginBottom: '1rem' }}>
          <h4 style={{ margin: '0 0 0.5rem 0', color: '#374151' }}>🛠️ Ferramentas MCP Disponíveis</h4>
          <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap' }}>
            {mcpTools.slice(0, 10).map((tool, index) => (
              <span key={index} style={{
                padding: '0.25rem 0.75rem',
                background: '#e0f2fe',
                color: '#0369a1',
                borderRadius: '12px',
                fontSize: '0.8rem',
                fontWeight: '500'
              }}>
                {tool.name}
              </span>
            ))}
            {mcpTools.length > 10 && (
              <span style={{
                padding: '0.25rem 0.75rem',
                background: '#f3f4f6',
                color: '#6b7280',
                borderRadius: '12px',
                fontSize: '0.8rem',
                fontWeight: '500'
              }}>
                +{mcpTools.length - 10} mais
              </span>
            )}
          </div>
        </div>

        {/* Formulário de Análise */}
        <div className="grid grid-2" style={{ marginBottom: '1rem' }}>
          <div>
            <label className="form-label">Proprietário/Organização</label>
            <input
              type="text"
              value={analysisOwner}
              onChange={(e) => setAnalysisOwner(e.target.value)}
              placeholder="ex: microsoft"
              className="form-input"
            />
          </div>
          <div>
            <label className="form-label">Repositório</label>
            <input
              type="text"
              value={analysisRepo}
              onChange={(e) => setAnalysisRepo(e.target.value)}
              placeholder="ex: vscode"
              className="form-input"
            />
          </div>
        </div>

        <div className="grid grid-2" style={{ marginBottom: '1rem' }}>
          <div>
            <label className="form-label">Branch</label>
            <input
              type="text"
              value={analysisBranch}
              onChange={(e) => setAnalysisBranch(e.target.value)}
              placeholder="main"
              className="form-input"
            />
          </div>
          <div>
            <label className="form-label">Consulta de Pesquisa (opcional)</label>
            <input
              type="text"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              placeholder="ex: function login"
              className="form-input"
            />
          </div>
        </div>

        {/* Erro MCP */}
        {mcpError && (
          <div style={{
            padding: '1rem',
            background: '#fef2f2',
            border: '1px solid #fecaca',
            borderRadius: '8px',
            color: '#dc2626',
            marginBottom: '1rem'
          }}>
            ⚠️ {mcpError}
          </div>
        )}

        {/* Botões de Ação MCP */}
        <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap', marginBottom: '1rem' }}>
          <button
            onClick={handleAnalyzeRepository}
            disabled={mcpLoading}
            className="btn btn-primary"
          >
            {mcpLoading ? '⏳...' : '📊 Analisar Repo'}
          </button>

          <button
            onClick={handleQueryRepository}
            disabled={mcpLoading || !searchQuery}
            className="btn btn-secondary"
          >
            {mcpLoading ? '⏳...' : '🔍 Pesquisar'}
          </button>

          <button
            onClick={handleAnalyzeAndQuery}
            disabled={mcpLoading}
            className="btn btn-success"
          >
            {mcpLoading ? '⏳...' : '🚀 Análise Completa'}
          </button>

          <button
            onClick={handleGetRepositoryInfo}
            disabled={mcpLoading}
            className="btn btn-secondary"
          >
            {mcpLoading ? '⏳...' : 'ℹ️ Info Repo'}
          </button>
        </div>

        {/* Botões de Carregamento de Dados */}
        <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap', marginBottom: '1rem' }}>
          <button
            onClick={handleLoadIssues}
            disabled={mcpLoading}
            className="btn btn-secondary"
          >
            {mcpLoading ? '⏳...' : '📋 Issues'}
          </button>

          <button
            onClick={handleLoadPullRequests}
            disabled={mcpLoading}
            className="btn btn-secondary"
          >
            {mcpLoading ? '⏳...' : '🔄 PRs'}
          </button>

          <button
            onClick={handleLoadCommits}
            disabled={mcpLoading}
            className="btn btn-secondary"
          >
            {mcpLoading ? '⏳...' : '📝 Commits'}
          </button>

          <button
            onClick={handleLoadBranches}
            disabled={mcpLoading}
            className="btn btn-secondary"
          >
            {mcpLoading ? '⏳...' : '🌿 Branches'}
          </button>

          <button
            onClick={handleLoadWorkflows}
            disabled={mcpLoading}
            className="btn btn-secondary"
          >
            {mcpLoading ? '⏳...' : '⚙️ Workflows'}
          </button>

          <button
            onClick={handleLoadSecurityAlerts}
            disabled={mcpLoading}
            className="btn btn-secondary"
          >
            {mcpLoading ? '⏳...' : '🔒 Segurança'}
          </button>

          <button
            onClick={handleLoadDependabotAlerts}
            disabled={mcpLoading}
            className="btn btn-secondary"
          >
            {mcpLoading ? '⏳...' : '🤖 Dependabot'}
          </button>
        </div>
      </div>

      {/* Resultados da Análise MCP */}
      {mcpAnalysis && (
        <div className="card">
          <div className="card-header">
            <h3 className="card-title">📊 Análise do Repositório - {mcpAnalysis.repository}</h3>
            <p className="card-description">Análise completa realizada pelo GitHub MCP Server</p>
          </div>

          <div className="grid grid-2">
            <div>
              <h4 style={{ margin: '0 0 0.5rem 0', color: '#374151' }}>📈 Estatísticas Gerais</h4>
              <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                <div style={{ padding: '0.5rem', background: '#f8fafc', borderRadius: '4px' }}>
                  <strong>Branch:</strong> {mcpAnalysis.branch}
                </div>
                <div style={{ padding: '0.5rem', background: '#f8fafc', borderRadius: '4px' }}>
                  <strong>Issues Abertas:</strong> {mcpAnalysis.openIssues?.Result?.total_count || 0}
                </div>
                <div style={{ padding: '0.5rem', background: '#f8fafc', borderRadius: '4px' }}>
                  <strong>PRs Abertas:</strong> {mcpAnalysis.openPullRequests?.Result?.total_count || 0}
                </div>
                <div style={{ padding: '0.5rem', background: '#f8fafc', borderRadius: '4px' }}>
                  <strong>Última Análise:</strong> {new Date(mcpAnalysis.analysisTimestamp).toLocaleString()}
                </div>
              </div>
            </div>

            <div>
              <h4 style={{ margin: '0 0 0.5rem 0', color: '#374151' }}>🔗 Links Rápidos</h4>
              <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                <a
                  href={`https://github.com/${mcpAnalysis.repository}`}
                  target="_blank"
                  rel="noopener noreferrer"
                  style={{ color: '#2563eb', textDecoration: 'none' }}
                >
                  🐙 Ver no GitHub
                </a>
                <a
                  href={`https://github.com/${mcpAnalysis.repository}/issues`}
                  target="_blank"
                  rel="noopener noreferrer"
                  style={{ color: '#2563eb', textDecoration: 'none' }}
                >
                  📋 Ver Issues
                </a>
                <a
                  href={`https://github.com/${mcpAnalysis.repository}/pulls`}
                  target="_blank"
                  rel="noopener noreferrer"
                  style={{ color: '#2563eb', textDecoration: 'none' }}
                >
                  🔄 Ver Pull Requests
                </a>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Resultados da Pesquisa MCP */}
      {mcpQuery && (
        <div className="card">
          <div className="card-header">
            <h3 className="card-title">🔍 Resultados da Pesquisa</h3>
            <p className="card-description">Resultados da pesquisa realizada no repositório</p>
          </div>

          <div style={{ marginBottom: '1rem' }}>
            <div style={{ padding: '0.75rem', background: '#f0f9ff', borderRadius: '8px', border: '1px solid #bae6fd' }}>
              <strong>Consulta:</strong> {mcpQuery.query}
            </div>
          </div>

          {mcpQuery.fileSearchResults?.Result?.total_count > 0 && (
            <div>
              <h4 style={{ margin: '0 0 0.5rem 0', color: '#374151' }}>
                📁 Arquivos Encontrados ({mcpQuery.fileSearchResults.Result.total_count})
              </h4>
              <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                {mcpQuery.fileSearchResults.Result.items?.slice(0, 5).map((item, index) => (
                  <div key={index} style={{
                    padding: '0.75rem',
                    border: '1px solid #e5e7eb',
                    borderRadius: '8px',
                    background: '#f9fafb'
                  }}>
                    <div style={{ fontWeight: '600', marginBottom: '0.25rem', color: '#374151' }}>
                      {item.name}
                    </div>
                    <div style={{ fontSize: '0.9rem', color: '#6b7280', marginBottom: '0.5rem' }}>
                      📂 {item.path}
                    </div>
                    {item.text_matches && (
                      <div style={{ fontSize: '0.8rem', color: '#059669', fontFamily: 'monospace' }}>
                        {item.text_matches[0]?.fragment}
                      </div>
                    )}
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      )}

      {/* Informações do Repositório MCP */}
      {mcpRepositoryInfo && (
        <div className="card">
          <div className="card-header">
            <h3 className="card-title">ℹ️ Informações do Repositório</h3>
            <p className="card-description">Detalhes técnicos do repositório</p>
          </div>

          <div style={{ background: '#f8fafc', padding: '1rem', borderRadius: '8px', fontFamily: 'monospace' }}>
            <pre style={{ margin: 0, whiteSpace: 'pre-wrap', color: '#374151', fontSize: '0.9rem' }}>
              {JSON.stringify(mcpRepositoryInfo.Result, null, 2)}
            </pre>
          </div>
        </div>
      )}

      {/* Issues MCP */}
      {mcpIssues.length > 0 && (
        <div className="card">
          <div className="card-header">
            <h3 className="card-title">📋 Issues Abertas ({mcpIssues.length})</h3>
            <p className="card-description">Issues ativas no repositório</p>
          </div>

          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
            {mcpIssues.slice(0, 5).map((issue, index) => (
              <div key={index} style={{
                padding: '1rem',
                border: '1px solid #e5e7eb',
                borderRadius: '8px',
                background: '#f9fafb'
              }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '0.5rem' }}>
                  <div style={{ flex: 1 }}>
                    <div style={{ fontWeight: '600', marginBottom: '0.25rem', color: '#374151' }}>
                      #{issue.number} - {issue.title}
                    </div>
                    <div style={{ fontSize: '0.9rem', color: '#6b7280' }}>
                      👤 {issue.user?.login} • 📅 {new Date(issue.created_at).toLocaleDateString()}
                    </div>
                  </div>
                  <div style={{
                    padding: '0.25rem 0.75rem',
                    borderRadius: '12px',
                    background: issue.state === 'open' ? '#dcfce7' : '#f3f4f6',
                    color: issue.state === 'open' ? '#166534' : '#374151',
                    fontSize: '0.8rem',
                    fontWeight: '500'
                  }}>
                    {issue.state}
                  </div>
                </div>
                {issue.labels && issue.labels.length > 0 && (
                  <div style={{ display: 'flex', gap: '0.25rem', flexWrap: 'wrap', marginTop: '0.5rem' }}>
                    {issue.labels.map((label, labelIndex) => (
                      <span key={labelIndex} style={{
                        padding: '0.125rem 0.5rem',
                        background: `#${label.color}`,
                        color: 'white',
                        borderRadius: '8px',
                        fontSize: '0.7rem',
                        fontWeight: '500'
                      }}>
                        {label.name}
                      </span>
                    ))}
                  </div>
                )}
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Pull Requests MCP */}
      {mcpPullRequests.length > 0 && (
        <div className="card">
          <div className="card-header">
            <h3 className="card-title">🔄 Pull Requests Abertas ({mcpPullRequests.length})</h3>
            <p className="card-description">Pull requests ativas no repositório</p>
          </div>

          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
            {mcpPullRequests.slice(0, 5).map((pr, index) => (
              <div key={index} style={{
                padding: '1rem',
                border: '1px solid #e5e7eb',
                borderRadius: '8px',
                background: '#f9fafb'
              }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '0.5rem' }}>
                  <div style={{ flex: 1 }}>
                    <div style={{ fontWeight: '600', marginBottom: '0.25rem', color: '#374151' }}>
                      #{pr.number} - {pr.title}
                    </div>
                    <div style={{ fontSize: '0.9rem', color: '#6b7280' }}>
                      👤 {pr.user?.login} • 📅 {new Date(pr.created_at).toLocaleDateString()}
                    </div>
                  </div>
                  <div style={{
                    padding: '0.25rem 0.75rem',
                    borderRadius: '12px',
                    background: pr.state === 'open' ? '#dcfce7' : '#f3f4f6',
                    color: pr.state === 'open' ? '#166534' : '#374151',
                    fontSize: '0.8rem',
                    fontWeight: '500'
                  }}>
                    {pr.state}
                  </div>
                </div>
                <div style={{ fontSize: '0.9rem', color: '#6b7280', marginTop: '0.5rem' }}>
                  {pr.body?.substring(0, 150)}...
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Commits MCP */}
      {mcpCommits.length > 0 && (
        <div className="card">
          <div className="card-header">
            <h3 className="card-title">📝 Commits Recentes ({mcpCommits.length})</h3>
            <p className="card-description">Últimos commits do repositório</p>
          </div>

          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
            {mcpCommits.slice(0, 5).map((commit, index) => (
              <div key={index} style={{
                padding: '1rem',
                border: '1px solid #e5e7eb',
                borderRadius: '8px',
                background: '#f9fafb'
              }}>
                <div style={{ fontWeight: '600', marginBottom: '0.25rem', color: '#374151' }}>
                  {commit.sha?.substring(0, 8)}
                </div>
                <div style={{ fontSize: '0.9rem', color: '#6b7280', marginBottom: '0.5rem' }}>
                  {commit.commit?.message}
                </div>
                <div style={{ fontSize: '0.8rem', color: '#9ca3af' }}>
                  👤 {commit.commit?.author?.name} • 📅 {new Date(commit.commit?.author?.date).toLocaleDateString()}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Branches MCP */}
      {mcpBranches.length > 0 && (
        <div className="card">
          <div className="card-header">
            <h3 className="card-title">🌿 Branches ({mcpBranches.length})</h3>
            <p className="card-description">Branches disponíveis no repositório</p>
          </div>

          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
            {mcpBranches.map((branch, index) => (
              <div key={index} style={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                padding: '0.75rem',
                border: '1px solid #e5e7eb',
                borderRadius: '8px',
                background: '#f9fafb'
              }}>
                <span style={{ fontWeight: '500' }}>
                  {branch.name}
                  {branch.protected && ' 🛡️'}
                </span>
                <div style={{ fontSize: '0.8rem', color: '#6b7280' }}>
                  {branch.commit?.sha?.substring(0, 8)}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Workflows MCP */}
      {mcpWorkflows.length > 0 && (
        <div className="card">
          <div className="card-header">
            <h3 className="card-title">⚙️ GitHub Actions Workflows ({mcpWorkflows.length})</h3>
            <p className="card-description">Workflows de CI/CD configurados</p>
          </div>

          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
            {mcpWorkflows.map((workflow, index) => (
              <div key={index} style={{
                padding: '1rem',
                border: '1px solid #e5e7eb',
                borderRadius: '8px',
                background: '#f9fafb'
              }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.5rem' }}>
                  <div style={{ fontWeight: '600', color: '#374151' }}>
                    {workflow.name}
                  </div>
                  <div style={{
                    padding: '0.25rem 0.75rem',
                    borderRadius: '12px',
                    background: workflow.state === 'active' ? '#dcfce7' : '#f3f4f6',
                    color: workflow.state === 'active' ? '#166534' : '#6b7280',
                    fontSize: '0.8rem',
                    fontWeight: '500'
                  }}>
                    {workflow.state}
                  </div>
                </div>
                <div style={{ fontSize: '0.9rem', color: '#6b7280' }}>
                  📁 {workflow.path}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Alertas de Segurança MCP */}
      {mcpSecurityAlerts.length > 0 && (
        <div className="card">
          <div className="card-header">
            <h3 className="card-title">🔒 Alertas de Segurança ({mcpSecurityAlerts.length})</h3>
            <p className="card-description">Vulnerabilidades detectadas no código</p>
          </div>

          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
            {mcpSecurityAlerts.map((alert, index) => (
              <div key={index} style={{
                padding: '1rem',
                border: '1px solid #e5e7eb',
                borderRadius: '8px',
                background: '#fef2f2',
                borderColor: '#fecaca'
              }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '0.5rem' }}>
                  <div style={{ flex: 1 }}>
                    <div style={{ fontWeight: '600', marginBottom: '0.25rem', color: '#dc2626' }}>
                      {alert.rule?.description || alert.title}
                    </div>
                    <div style={{ fontSize: '0.9rem', color: '#7f1d1d' }}>
                      📁 {alert.location?.path}:{alert.location?.start_line}-{alert.location?.end_line}
                    </div>
                  </div>
                  <div style={{
                    padding: '0.25rem 0.75rem',
                    borderRadius: '12px',
                    background: '#fee2e2',
                    color: '#dc2626',
                    fontSize: '0.8rem',
                    fontWeight: '500'
                  }}>
                    {alert.severity || 'high'}
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Alertas do Dependabot MCP */}
      {mcpDependabotAlerts.length > 0 && (
        <div className="card">
          <div className="card-header">
            <h3 className="card-title">🤖 Alertas do Dependabot ({mcpDependabotAlerts.length})</h3>
            <p className="card-description">Dependências desatualizadas ou vulneráveis</p>
          </div>

          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
            {mcpDependabotAlerts.map((alert, index) => (
              <div key={index} style={{
                padding: '1rem',
                border: '1px solid #e5e7eb',
                borderRadius: '8px',
                background: '#fef3c7',
                borderColor: '#f59e0b'
              }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '0.5rem' }}>
                  <div style={{ flex: 1 }}>
                    <div style={{ fontWeight: '600', marginBottom: '0.25rem', color: '#92400e' }}>
                      {alert.security_advisory?.summary || alert.title}
                    </div>
                    <div style={{ fontSize: '0.9rem', color: '#78350f' }}>
                      📦 {alert.dependency?.package?.name} {alert.dependency?.manifest_path}
                    </div>
                  </div>
                  <div style={{
                    padding: '0.25rem 0.75rem',
                    borderRadius: '12px',
                    background: '#fed7aa',
                    color: '#9a3412',
                    fontSize: '0.8rem',
                    fontWeight: '500'
                  }}>
                    {alert.severity}
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Instruções de Configuração */}
      <div className="card">
        <div className="card-header">
          <h3 className="card-title">📋 Como Configurar no GitHub</h3>
          <p className="card-description">Passos para configurar o webhook no seu repositório</p>
        </div>

        <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
          <div style={{ display: 'flex', alignItems: 'flex-start', gap: '1rem' }}>
            <div style={{
              width: '30px',
              height: '30px',
              borderRadius: '50%',
              background: '#667eea',
              color: 'white',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              fontWeight: '600',
              fontSize: '0.9rem',
              flexShrink: 0
            }}>
              1
            </div>
            <div>
              <div style={{ fontWeight: '600', marginBottom: '0.25rem' }}>Acesse seu repositório no GitHub</div>
              <div style={{ color: '#6b7280', fontSize: '0.9rem' }}>
                Vá para Settings → Webhooks → Add webhook
              </div>
            </div>
          </div>

          <div style={{ display: 'flex', alignItems: 'flex-start', gap: '1rem' }}>
            <div style={{
              width: '30px',
              height: '30px',
              borderRadius: '50%',
              background: '#667eea',
              color: 'white',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              fontWeight: '600',
              fontSize: '0.9rem',
              flexShrink: 0
            }}>
              2
            </div>
            <div>
              <div style={{ fontWeight: '600', marginBottom: '0.25rem' }}>Configure o Payload URL</div>
              <div style={{ color: '#6b7280', fontSize: '0.9rem' }}>
                Cole a URL do webhook gerada acima
              </div>
            </div>
          </div>

          <div style={{ display: 'flex', alignItems: 'flex-start', gap: '1rem' }}>
            <div style={{
              width: '30px',
              height: '30px',
              borderRadius: '50%',
              background: '#667eea',
              color: 'white',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              fontWeight: '600',
              fontSize: '0.9rem',
              flexShrink: 0
            }}>
              3
            </div>
            <div>
              <div style={{ fontWeight: '600', marginBottom: '0.25rem' }}>Defina o Secret</div>
              <div style={{ color: '#6b7280', fontSize: '0.9rem' }}>
                Use o secret gerado para validar as requisições
              </div>
            </div>
          </div>

          <div style={{ display: 'flex', alignItems: 'flex-start', gap: '1rem' }}>
            <div style={{
              width: '30px',
              height: '30px',
              borderRadius: '50%',
              background: '#667eea',
              color: 'white',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              fontWeight: '600',
              fontSize: '0.9rem',
              flexShrink: 0
            }}>
              4
            </div>
            <div>
              <div style={{ fontWeight: '600', marginBottom: '0.25rem' }}>Selecione os Eventos</div>
              <div style={{ color: '#6b7280', fontSize: '0.9rem' }}>
                Marque "Pushes" e "Pull requests" para deployments automáticos
              </div>
            </div>
          </div>

          <div style={{ display: 'flex', alignItems: 'flex-start', gap: '1rem' }}>
            <div style={{
              width: '30px',
              height: '30px',
              borderRadius: '50%',
              background: '#667eea',
              color: 'white',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              fontWeight: '600',
              fontSize: '0.9rem',
              flexShrink: 0
            }}>
              5
            </div>
            <div>
              <div style={{ fontWeight: '600', marginBottom: '0.25rem' }}>Salve as Configurações</div>
              <div style={{ color: '#6b7280', fontSize: '0.9rem' }}>
                Clique em "Add webhook" para finalizar
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Status dos Webhooks */}
      <div className="card">
        <div className="card-header">
          <h3 className="card-title">📊 Status dos Webhooks</h3>
          <p className="card-description">Monitoramento dos webhooks configurados</p>
        </div>

        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
          <div style={{
            padding: '1rem',
            border: '1px solid #e5e7eb',
            borderRadius: '8px',
            background: '#f8fafc'
          }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <div>
                <div style={{ fontWeight: '600', marginBottom: '0.25rem' }}>
                  dronreef2/ASP.NETCore
                </div>
                <div style={{ fontSize: '0.9rem', color: '#6b7280' }}>
                  Última entrega: 2 minutos atrás • Status: ✅ Sucesso
                </div>
              </div>
              <div style={{
                padding: '0.25rem 0.75rem',
                borderRadius: '12px',
                background: '#d1fae5',
                color: '#065f46',
                fontSize: '0.8rem',
                fontWeight: '500'
              }}>
                Ativo
              </div>
            </div>
          </div>

          <div style={{
            padding: '1rem',
            border: '1px solid #e5e7eb',
            borderRadius: '8px',
            background: '#f8fafc'
          }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <div>
                <div style={{ fontWeight: '600', marginBottom: '0.25rem' }}>
                  dronreef2/react-app
                </div>
                <div style={{ fontSize: '0.9rem', color: '#6b7280' }}>
                  Última tentativa: 1 hora atrás • Status: ❌ Falha
                </div>
              </div>
              <div style={{
                padding: '0.25rem 0.75rem',
                borderRadius: '12px',
                background: '#fee2e2',
                color: '#dc2626',
                fontSize: '0.8rem',
                fontWeight: '500'
              }}>
                Inativo
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
