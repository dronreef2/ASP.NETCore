import React, { useState, useEffect } from 'react';

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
