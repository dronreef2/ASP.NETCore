import React, { useState, useEffect } from 'react';

export default function DeploymentSection() {
  const [deployments, setDeployments] = useState([]);
  const [selectedDeployment, setSelectedDeployment] = useState(null);
  const [isLoading, setIsLoading] = useState(false);
  const [logs, setLogs] = useState([]);
  const [analysis, setAnalysis] = useState(null);
  const [newDeployment, setNewDeployment] = useState({
    repositoryUrl: '',
    branch: 'main',
    author: ''
  });

  useEffect(() => {
    loadDeployments();
  }, []);

  const loadDeployments = async () => {
    setIsLoading(true);
    try {
      // Simular carregamento de deployments
      const mockDeployments = [
        {
          id: 'dep-001',
          repositoryUrl: 'https://github.com/dronreef2/ASP.NETCore',
          branch: 'main',
          status: 'success',
          createdAt: new Date(Date.now() - 3600000),
          duration: 125,
          author: 'dronreef2',
          commitSha: 'abc123def456'
        },
        {
          id: 'dep-002',
          repositoryUrl: 'https://github.com/dronreef2/ASP.NETCore',
          branch: 'feature/new-ui',
          status: 'failed',
          createdAt: new Date(Date.now() - 7200000),
          duration: 89,
          author: 'dronreef2',
          commitSha: 'def789ghi012'
        },
        {
          id: 'dep-003',
          repositoryUrl: 'https://github.com/dronreef2/ASP.NETCore',
          branch: 'main',
          status: 'running',
          createdAt: new Date(),
          duration: null,
          author: 'dronreef2',
          commitSha: 'jkl345mno678'
        }
      ];
      setDeployments(mockDeployments);
    } catch (error) {
      console.error('Erro ao carregar deployments:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const createDeployment = async () => {
    if (!newDeployment.repositoryUrl.trim()) {
      alert('URL do repositório é obrigatória');
      return;
    }

    setIsLoading(true);
    try {
      // Simular criação de deployment
      const deployment = {
        id: `dep-${Date.now()}`,
        ...newDeployment,
        status: 'running',
        createdAt: new Date(),
        duration: null,
        commitSha: 'pending'
      };

      setDeployments(prev => [deployment, ...prev]);
      setNewDeployment({ repositoryUrl: '', branch: 'main', author: '' });
    } catch (error) {
      console.error('Erro ao criar deployment:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const loadDeploymentLogs = async (deploymentId) => {
    try {
      // Simular carregamento de logs
      const mockLogs = [
        { timestamp: new Date(Date.now() - 300000), message: 'Starting deployment process...', level: 'info' },
        { timestamp: new Date(Date.now() - 250000), message: 'Cloning repository...', level: 'info' },
        { timestamp: new Date(Date.now() - 200000), message: 'Installing dependencies...', level: 'info' },
        { timestamp: new Date(Date.now() - 150000), message: 'Running tests...', level: 'warn' },
        { timestamp: new Date(Date.now() - 100000), message: 'Build completed successfully', level: 'info' },
        { timestamp: new Date(Date.now() - 50000), message: 'Deploying to production...', level: 'info' },
        { timestamp: new Date(), message: 'Deployment completed!', level: 'info' }
      ];
      setLogs(mockLogs);
    } catch (error) {
      console.error('Erro ao carregar logs:', error);
    }
  };

  const analyzeDeployment = async (deploymentId) => {
    try {
      // Simular análise de deployment
      const mockAnalysis = {
        summary: 'Deployment executado com sucesso. Todas as verificações passaram.',
        recommendations: [
          'Considerar otimização do tempo de build',
          'Implementar cache para dependências',
          'Adicionar mais testes automatizados'
        ],
        securityScore: 85,
        performanceScore: 92,
        reliabilityScore: 88
      };
      setAnalysis(mockAnalysis);
    } catch (error) {
      console.error('Erro ao analisar deployment:', error);
    }
  };

  const getStatusColor = (status) => {
    switch (status) {
      case 'success': return '#10b981';
      case 'failed': return '#ef4444';
      case 'running': return '#f59e0b';
      default: return '#6b7280';
    }
  };

  const getStatusText = (status) => {
    switch (status) {
      case 'success': return 'Sucesso';
      case 'failed': return 'Falha';
      case 'running': return 'Executando';
      default: return 'Desconhecido';
    }
  };

  const formatDuration = (seconds) => {
    if (!seconds) return 'N/A';
    const minutes = Math.floor(seconds / 60);
    const remainingSeconds = seconds % 60;
    return `${minutes}m ${remainingSeconds}s`;
  };

  return (
    <div className="grid">
      <div className="card">
        <div className="card-header">
          <h3 className="card-title">🚀 Sistema de Deployments</h3>
          <p className="card-description">Gerencie e monitore deployments automáticos do GitHub</p>
        </div>

        {/* Criar Novo Deployment */}
        <div className="form-group">
          <h4 style={{ margin: '0 0 1rem 0', color: '#374151' }}>🔧 Criar Novo Deployment</h4>

          <div className="grid grid-3">
            <div>
              <label className="form-label">URL do Repositório</label>
              <input
                type="url"
                value={newDeployment.repositoryUrl}
                onChange={(e) => setNewDeployment(prev => ({ ...prev, repositoryUrl: e.target.value }))}
                placeholder="https://github.com/user/repo"
                className="form-input"
              />
            </div>

            <div>
              <label className="form-label">Branch</label>
              <input
                type="text"
                value={newDeployment.branch}
                onChange={(e) => setNewDeployment(prev => ({ ...prev, branch: e.target.value }))}
                placeholder="main"
                className="form-input"
              />
            </div>

            <div>
              <label className="form-label">Autor</label>
              <input
                type="text"
                value={newDeployment.author}
                onChange={(e) => setNewDeployment(prev => ({ ...prev, author: e.target.value }))}
                placeholder="Seu nome"
                className="form-input"
              />
            </div>
          </div>

          <button
            onClick={createDeployment}
            disabled={isLoading || !newDeployment.repositoryUrl.trim()}
            className="btn btn-primary"
            style={{ marginTop: '1rem' }}
          >
            {isLoading ? '⏳ Criando...' : '🚀 Iniciar Deployment'}
          </button>
        </div>
      </div>

      {/* Lista de Deployments */}
      <div className="card">
        <div className="card-header">
          <h3 className="card-title">📋 Histórico de Deployments</h3>
          <p className="card-description">Últimos deployments realizados</p>
        </div>

        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
          {deployments.map((deployment) => (
            <div
              key={deployment.id}
              style={{
                padding: '1rem',
                border: `2px solid ${selectedDeployment && selectedDeployment.id === deployment.id ? '#667eea' : '#e5e7eb'}`,
                borderRadius: '8px',
                background: selectedDeployment && selectedDeployment.id === deployment.id ? '#f0f4ff' : 'white',
                cursor: 'pointer',
                transition: 'all 0.3s ease'
              }}
              onClick={() => {
                setSelectedDeployment(deployment);
                loadDeploymentLogs(deployment.id);
                analyzeDeployment(deployment.id);
              }}
            >
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                <div style={{ flex: 1 }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.5rem' }}>
                    <span style={{
                      color: getStatusColor(deployment.status),
                      fontWeight: '600'
                    }}>
                      ● {getStatusText(deployment.status)}
                    </span>
                    <span style={{ fontSize: '0.9rem', color: '#6b7280' }}>
                      {deployment.repositoryUrl.split('/').pop()}:{deployment.branch}
                    </span>
                  </div>

                  <div style={{ fontSize: '0.9rem', color: '#6b7280', marginBottom: '0.25rem' }}>
                    👤 {deployment.author} • 📅 {deployment.createdAt.toLocaleString('pt-BR')}
                  </div>

                  <div style={{ fontSize: '0.8rem', color: '#9ca3af' }}>
                    ⏱️ {formatDuration(deployment.duration)} • 🔗 {deployment.commitSha}
                  </div>
                </div>

                <div style={{ display: 'flex', gap: '0.5rem' }}>
                  <button
                    onClick={(e) => {
                      e.stopPropagation();
                      loadDeploymentLogs(deployment.id);
                    }}
                    className="btn btn-secondary"
                    style={{ fontSize: '0.8rem', padding: '0.25rem 0.75rem' }}
                  >
                    📄 Logs
                  </button>

                  <button
                    onClick={(e) => {
                      e.stopPropagation();
                      analyzeDeployment(deployment.id);
                    }}
                    className="btn btn-secondary"
                    style={{ fontSize: '0.8rem', padding: '0.25rem 0.75rem' }}
                  >
                    🧠 Analisar
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Detalhes do Deployment Selecionado */}
      {selectedDeployment && (
        <div className="grid">
          {/* Logs */}
          <div className="card">
            <div className="card-header">
              <h3 className="card-title">📄 Logs do Deployment</h3>
              <p className="card-description">{selectedDeployment.id} - {selectedDeployment.repositoryUrl}</p>
            </div>

            <div style={{
              background: '#f8fafc',
              padding: '1rem',
              borderRadius: '8px',
              fontFamily: 'monospace',
              fontSize: '0.8rem',
              maxHeight: '400px',
              overflowY: 'auto'
            }}>
              {logs.map((log, index) => (
                <div key={index} style={{
                  marginBottom: '0.5rem',
                  color: log.level === 'error' ? '#dc2626' :
                         log.level === 'warn' ? '#d97706' : '#374151'
                }}>
                  <span style={{ color: '#6b7280' }}>
                    [{log.timestamp.toLocaleTimeString('pt-BR')}]
                  </span> {log.message}
                </div>
              ))}
            </div>
          </div>

          {/* Análise */}
          {analysis && (
            <div className="card">
              <div className="card-header">
                <h3 className="card-title">🧠 Análise Inteligente</h3>
                <p className="card-description">Análise automática do deployment</p>
              </div>

              <div style={{ marginBottom: '1rem' }}>
                <h4 style={{ margin: '0 0 0.5rem 0', color: '#374151' }}>📋 Resumo</h4>
                <p style={{ color: '#6b7280', lineHeight: '1.6' }}>{analysis.summary}</p>
              </div>

              <div className="grid grid-3" style={{ marginBottom: '1rem' }}>
                <div className="stats-card">
                  <div className="stats-icon">🔒</div>
                  <div className="stats-number">{analysis.securityScore}%</div>
                  <div className="stats-label">Segurança</div>
                </div>

                <div className="stats-card">
                  <div className="stats-icon">⚡</div>
                  <div className="stats-number">{analysis.performanceScore}%</div>
                  <div className="stats-label">Performance</div>
                </div>

                <div className="stats-card">
                  <div className="stats-icon">🔄</div>
                  <div className="stats-number">{analysis.reliabilityScore}%</div>
                  <div className="stats-label">Confiabilidade</div>
                </div>
              </div>

              <div>
                <h4 style={{ margin: '0 0 0.5rem 0', color: '#374151' }}>💡 Recomendações</h4>
                <ul style={{ color: '#6b7280', lineHeight: '1.6' }}>
                  {analysis.recommendations.map((rec, index) => (
                    <li key={index}>• {rec}</li>
                  ))}
                </ul>
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
