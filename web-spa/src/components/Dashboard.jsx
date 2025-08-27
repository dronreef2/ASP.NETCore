import React, { useState, useEffect } from 'react';

export default function Dashboard() {
  const [systemStatus, setSystemStatus] = useState({
    dotnetBackend: 'checking',
    nodeBackend: 'checking',
    database: 'checking',
    aiServices: 'checking'
  });

  const [stats, setStats] = useState({
    totalChats: 0,
    totalAnalyses: 0,
    totalSearches: 0,
    activeUsers: 0
  });

  useEffect(() => {
    // Simular verificação de status dos serviços
    const checkServices = async () => {
      try {
        // Verificar backend .NET
        const dotnetResponse = await fetch('/api/health/dotnet').catch(() => null);
        setSystemStatus(prev => ({
          ...prev,
          dotnetBackend: dotnetResponse?.ok ? 'online' : 'offline'
        }));

        // Verificar backend Node.js
        const nodeResponse = await fetch('/api/health/node').catch(() => null);
        setSystemStatus(prev => ({
          ...prev,
          nodeBackend: nodeResponse?.ok ? 'online' : 'offline'
        }));

        // Simular outros status
        setSystemStatus(prev => ({
          ...prev,
          database: 'online',
          aiServices: 'online'
        }));

        // Carregar estatísticas (simuladas)
        setStats({
          totalChats: 1247,
          totalAnalyses: 89,
          totalSearches: 456,
          activeUsers: 23
        });
      } catch (error) {
        console.error('Erro ao verificar status:', error);
      }
    };

    checkServices();
  }, []);

  const getStatusColor = (status) => {
    switch (status) {
      case 'online': return '#10b981';
      case 'offline': return '#ef4444';
      default: return '#f59e0b';
    }
  };

  const getStatusText = (status) => {
    switch (status) {
      case 'online': return 'Online';
      case 'offline': return 'Offline';
      default: return 'Verificando...';
    }
  };

  return (
    <div className="grid">
      {/* Status do Sistema */}
      <div className="card">
        <div className="card-header">
          <h3 className="card-title">📊 Status do Sistema</h3>
          <p className="card-description">Monitoramento em tempo real dos serviços</p>
        </div>

        <div className="grid grid-2">
          <div className="stats-card">
            <div className="stats-icon">🔧</div>
            <div className="stats-number" style={{ color: getStatusColor(systemStatus.dotnetBackend) }}>
              ●
            </div>
            <div className="stats-label">Backend .NET</div>
            <div style={{ fontSize: '0.8rem', marginTop: '0.5rem', color: getStatusColor(systemStatus.dotnetBackend) }}>
              {getStatusText(systemStatus.dotnetBackend)}
            </div>
          </div>

          <div className="stats-card">
            <div className="stats-icon">🟢</div>
            <div className="stats-number" style={{ color: getStatusColor(systemStatus.nodeBackend) }}>
              ●
            </div>
            <div className="stats-label">Backend Node.js</div>
            <div style={{ fontSize: '0.8rem', marginTop: '0.5rem', color: getStatusColor(systemStatus.nodeBackend) }}>
              {getStatusText(systemStatus.nodeBackend)}
            </div>
          </div>

          <div className="stats-card">
            <div className="stats-icon">🗄️</div>
            <div className="stats-number" style={{ color: getStatusColor(systemStatus.database) }}>
              ●
            </div>
            <div className="stats-label">Banco de Dados</div>
            <div style={{ fontSize: '0.8rem', marginTop: '0.5rem', color: getStatusColor(systemStatus.database) }}>
              {getStatusText(systemStatus.database)}
            </div>
          </div>

          <div className="stats-card">
            <div className="stats-icon">🤖</div>
            <div className="stats-number" style={{ color: getStatusColor(systemStatus.aiServices) }}>
              ●
            </div>
            <div className="stats-label">Serviços IA</div>
            <div style={{ fontSize: '0.8rem', marginTop: '0.5rem', color: getStatusColor(systemStatus.aiServices) }}>
              {getStatusText(systemStatus.aiServices)}
            </div>
          </div>
        </div>
      </div>

      {/* Estatísticas Gerais */}
      <div className="card">
        <div className="card-header">
          <h3 className="card-title">📈 Estatísticas Gerais</h3>
          <p className="card-description">Resumo das atividades do sistema</p>
        </div>

        <div className="grid grid-4">
          <div className="stats-card">
            <div className="stats-icon">💬</div>
            <div className="stats-number">{stats.totalChats.toLocaleString()}</div>
            <div className="stats-label">Total de Chats</div>
          </div>

          <div className="stats-card">
            <div className="stats-icon">🧠</div>
            <div className="stats-number">{stats.totalAnalyses.toLocaleString()}</div>
            <div className="stats-label">Análises Realizadas</div>
          </div>

          <div className="stats-card">
            <div className="stats-icon">🔍</div>
            <div className="stats-number">{stats.totalSearches.toLocaleString()}</div>
            <div className="stats-label">Buscas Semânticas</div>
          </div>

          <div className="stats-card">
            <div className="stats-icon">👥</div>
            <div className="stats-number">{stats.activeUsers}</div>
            <div className="stats-label">Usuários Ativos</div>
          </div>
        </div>
      </div>

      {/* Ações Rápidas */}
      <div className="card">
        <div className="card-header">
          <h3 className="card-title">⚡ Ações Rápidas</h3>
          <p className="card-description">Acesse rapidamente as funcionalidades principais</p>
        </div>

        <div className="grid grid-3">
          <button className="btn btn-primary">
            💬 Iniciar Chat
          </button>

          <button className="btn btn-primary">
            🧠 Nova Análise
          </button>

          <button className="btn btn-primary">
            🔍 Busca Rápida
          </button>
        </div>
      </div>

      {/* Logs Recentes */}
      <div className="card">
        <div className="card-header">
          <h3 className="card-title">📋 Atividades Recentes</h3>
          <p className="card-description">Últimas ações realizadas no sistema</p>
        </div>

        <div style={{ maxHeight: '300px', overflowY: 'auto' }}>
          <div style={{ padding: '0.5rem 0', borderBottom: '1px solid #e5e7eb' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <span>💬 Chat iniciado com usuário #1234</span>
              <span style={{ fontSize: '0.8rem', color: '#6b7280' }}>2 min atrás</span>
            </div>
          </div>

          <div style={{ padding: '0.5rem 0', borderBottom: '1px solid #e5e7eb' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <span>🧠 Análise de documento realizada</span>
              <span style={{ fontSize: '0.8rem', color: '#6b7280' }}>5 min atrás</span>
            </div>
          </div>

          <div style={{ padding: '0.5rem 0', borderBottom: '1px solid #e5e7eb' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <span>🔍 Busca semântica executada</span>
              <span style={{ fontSize: '0.8rem', color: '#6b7280' }}>8 min atrás</span>
            </div>
          </div>

          <div style={{ padding: '0.5rem 0', borderBottom: '1px solid #e5e7eb' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <span>📋 Relatório gerado automaticamente</span>
              <span style={{ fontSize: '0.8rem', color: '#6b7280' }}>12 min atrás</span>
            </div>
          </div>

          <div style={{ padding: '0.5rem 0' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <span>⚙️ Configurações atualizadas</span>
              <span style={{ fontSize: '0.8rem', color: '#6b7280' }}>15 min atrás</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
