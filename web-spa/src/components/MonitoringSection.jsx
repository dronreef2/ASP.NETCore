import React, { useState, useEffect } from 'react';

export default function MonitoringSection() {
  const [metrics, setMetrics] = useState(null);
  const [logs, setLogs] = useState([]);
  const [healthStatus, setHealthStatus] = useState(null);
  const [selectedTimeRange, setSelectedTimeRange] = useState('1h');
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    loadMetrics();
    loadHealthStatus();
    loadLogs();
  }, [selectedTimeRange]);

  const loadMetrics = async () => {
    setIsLoading(true);
    try {
      // Simular carregamento de métricas
      const mockMetrics = {
        responseTime: {
          p50: 245,
          p95: 890,
          p99: 1200
        },
        requestsPerSecond: 12.5,
        errorRate: 0.02,
        cpuUsage: 67,
        memoryUsage: 2.4,
        diskUsage: 23,
        activeConnections: 45,
        deploymentsToday: 8,
        successfulDeployments: 7,
        failedDeployments: 1
      };
      setMetrics(mockMetrics);
    } catch (error) {
      console.error('Erro ao carregar métricas:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const loadHealthStatus = async () => {
    try {
      // Simular verificação de saúde
      const mockHealth = {
        overall: 'healthy',
        services: {
          dotnet: { status: 'healthy', responseTime: 120 },
          nodejs: { status: 'healthy', responseTime: 95 },
          database: { status: 'healthy', responseTime: 45 },
          redis: { status: 'healthy', responseTime: 12 },
          ngrok: { status: 'warning', responseTime: 500 }
        },
        uptime: '7d 4h 23m',
        lastIncident: null
      };
      setHealthStatus(mockHealth);
    } catch (error) {
      console.error('Erro ao carregar status de saúde:', error);
    }
  };

  const loadLogs = () => {
    // Simular logs recentes
    const mockLogs = [
      { timestamp: new Date(Date.now() - 300000), level: 'info', service: 'dotnet', message: 'Deployment completed successfully' },
      { timestamp: new Date(Date.now() - 600000), level: 'warn', service: 'nodejs', message: 'High memory usage detected' },
      { timestamp: new Date(Date.now() - 900000), level: 'info', service: 'database', message: 'Database backup completed' },
      { timestamp: new Date(Date.now() - 1200000), level: 'error', service: 'ngrok', message: 'Connection timeout' },
      { timestamp: new Date(Date.now() - 1500000), level: 'info', service: 'dotnet', message: 'New deployment started' },
      { timestamp: new Date(Date.now() - 1800000), level: 'info', service: 'nodejs', message: 'Cache cleared successfully' }
    ];
    setLogs(mockLogs);
  };

  const getStatusColor = (status) => {
    switch (status) {
      case 'healthy': return '#10b981';
      case 'warning': return '#f59e0b';
      case 'error': return '#ef4444';
      default: return '#6b7280';
    }
  };

  const getLogLevelColor = (level) => {
    switch (level) {
      case 'error': return '#ef4444';
      case 'warn': return '#f59e0b';
      case 'info': return '#3b82f6';
      case 'debug': return '#6b7280';
      default: return '#374151';
    }
  };

  const formatBytes = (bytes) => {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  return (
    <div className="grid">
      {/* Controles de Tempo */}
      <div className="card">
        <div className="card-header">
          <h3 className="card-title">⏰ Controle de Tempo</h3>
          <p className="card-description">Selecione o período para análise das métricas</p>
        </div>

        <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap' }}>
          {['5m', '1h', '6h', '24h', '7d', '30d'].map((range) => (
            <button
              key={range}
              onClick={() => setSelectedTimeRange(range)}
              className={`btn ${selectedTimeRange === range ? 'btn-primary' : 'btn-secondary'}`}
            >
              {range}
            </button>
          ))}
        </div>
      </div>

      {/* Status de Saúde */}
      <div className="card">
        <div className="card-header">
          <h3 className="card-title">💓 Status de Saúde do Sistema</h3>
          <p className="card-description">Monitoramento de saúde de todos os serviços</p>
        </div>

        {healthStatus && (
          <div>
            <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', marginBottom: '1rem' }}>
              <div style={{
                padding: '0.5rem 1rem',
                borderRadius: '20px',
                background: getStatusColor(healthStatus.overall),
                color: 'white',
                fontWeight: '500'
              }}>
                {healthStatus.overall === 'healthy' ? '🟢 Sistema Saudável' : '🔴 Problemas Detectados'}
              </div>
              <div style={{ color: '#6b7280' }}>
                Uptime: {healthStatus.uptime}
              </div>
            </div>

            <div className="grid grid-2">
              {Object.entries(healthStatus.services).map(([service, info]) => (
                <div key={service} style={{
                  padding: '1rem',
                  border: `2px solid ${getStatusColor(info.status)}`,
                  borderRadius: '8px',
                  background: info.status === 'healthy' ? '#f0fdf4' : '#fef3c7'
                }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <div>
                      <div style={{ fontWeight: '600', textTransform: 'capitalize' }}>
                        {service}
                      </div>
                      <div style={{ fontSize: '0.9rem', color: '#6b7280' }}>
                        {info.responseTime}ms de resposta
                      </div>
                    </div>
                    <div style={{
                      width: '12px',
                      height: '12px',
                      borderRadius: '50%',
                      background: getStatusColor(info.status)
                    }}></div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>

      {/* Métricas de Performance */}
      <div className="card">
        <div className="card-header">
          <h3 className="card-title">📊 Métricas de Performance</h3>
          <p className="card-description">Indicadores chave de performance do sistema</p>
        </div>

        {metrics && (
          <div className="grid grid-3">
            <div className="stats-card">
              <div className="stats-icon">⚡</div>
              <div className="stats-number">{metrics.requestsPerSecond}</div>
              <div className="stats-label">Req/s</div>
            </div>

            <div className="stats-card">
              <div className="stats-icon">🚨</div>
              <div className="stats-number">{(metrics.errorRate * 100).toFixed(2)}%</div>
              <div className="stats-label">Taxa de Erro</div>
            </div>

            <div className="stats-card">
              <div className="stats-icon">🔗</div>
              <div className="stats-number">{metrics.activeConnections}</div>
              <div className="stats-label">Conexões Ativas</div>
            </div>

            <div className="stats-card">
              <div className="stats-icon">🖥️</div>
              <div className="stats-number">{metrics.cpuUsage}%</div>
              <div className="stats-label">CPU</div>
            </div>

            <div className="stats-card">
              <div className="stats-icon">💾</div>
              <div className="stats-number">{metrics.memoryUsage}GB</div>
              <div className="stats-label">Memória</div>
            </div>

            <div className="stats-card">
              <div className="stats-icon">💽</div>
              <div className="stats-number">{metrics.diskUsage}%</div>
              <div className="stats-label">Disco</div>
            </div>
          </div>
        )}

        {metrics && (
          <div style={{ marginTop: '1.5rem' }}>
            <h4 style={{ margin: '0 0 1rem 0', color: '#374151' }}>⏱️ Latência de Resposta</h4>
            <div className="grid grid-3">
              <div style={{ textAlign: 'center', padding: '1rem', background: '#f8fafc', borderRadius: '8px' }}>
                <div style={{ fontSize: '1.5rem', fontWeight: '600', color: '#10b981' }}>
                  {metrics.responseTime.p50}ms
                </div>
                <div style={{ fontSize: '0.9rem', color: '#6b7280' }}>P50</div>
              </div>

              <div style={{ textAlign: 'center', padding: '1rem', background: '#f8fafc', borderRadius: '8px' }}>
                <div style={{ fontSize: '1.5rem', fontWeight: '600', color: '#f59e0b' }}>
                  {metrics.responseTime.p95}ms
                </div>
                <div style={{ fontSize: '0.9rem', color: '#6b7280' }}>P95</div>
              </div>

              <div style={{ textAlign: 'center', padding: '1rem', background: '#f8fafc', borderRadius: '8px' }}>
                <div style={{ fontSize: '1.5rem', fontWeight: '600', color: '#ef4444' }}>
                  {metrics.responseTime.p99}ms
                </div>
                <div style={{ fontSize: '0.9rem', color: '#6b7280' }}>P99</div>
              </div>
            </div>
          </div>
        )}
      </div>

      {/* Métricas de Deployment */}
      <div className="card">
        <div className="card-header">
          <h3 className="card-title">🚀 Métricas de Deployment</h3>
          <p className="card-description">Performance dos deployments no período selecionado</p>
        </div>

        {metrics && (
          <div className="grid grid-3">
            <div className="stats-card">
              <div className="stats-icon">📦</div>
              <div className="stats-number">{metrics.deploymentsToday}</div>
              <div className="stats-label">Deployments Hoje</div>
            </div>

            <div className="stats-card">
              <div className="stats-icon">✅</div>
              <div className="stats-number">{metrics.successfulDeployments}</div>
              <div className="stats-label">Sucessos</div>
            </div>

            <div className="stats-card">
              <div className="stats-icon">❌</div>
              <div className="stats-number">{metrics.failedDeployments}</div>
              <div className="stats-label">Falhas</div>
            </div>
          </div>
        )}

        {metrics && (
          <div style={{ marginTop: '1.5rem' }}>
            <h4 style={{ margin: '0 0 1rem 0', color: '#374151' }}>📈 Taxa de Sucesso</h4>
            <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
              <div style={{ flex: 1, height: '20px', background: '#e5e7eb', borderRadius: '10px', overflow: 'hidden' }}>
                <div style={{
                  height: '100%',
                  width: `${(metrics.successfulDeployments / metrics.deploymentsToday) * 100}%`,
                  background: '#10b981',
                  borderRadius: '10px'
                }}></div>
              </div>
              <div style={{ fontWeight: '600', color: '#374151' }}>
                {((metrics.successfulDeployments / metrics.deploymentsToday) * 100).toFixed(1)}%
              </div>
            </div>
          </div>
        )}
      </div>

      {/* Logs Recentes */}
      <div className="card">
        <div className="card-header">
          <h3 className="card-title">📋 Logs Recentes</h3>
          <p className="card-description">Últimas entradas de log do sistema</p>
        </div>

        <div style={{ maxHeight: '400px', overflowY: 'auto' }}>
          {logs.map((log, index) => (
            <div key={index} style={{
              padding: '0.75rem',
              borderBottom: index < logs.length - 1 ? '1px solid #e5e7eb' : 'none',
              display: 'flex',
              alignItems: 'center',
              gap: '0.75rem'
            }}>
              <div style={{
                width: '8px',
                height: '8px',
                borderRadius: '50%',
                background: getLogLevelColor(log.level),
                flexShrink: 0
              }}></div>

              <div style={{ flex: 1 }}>
                <div style={{ fontSize: '0.9rem', color: '#374151' }}>
                  <span style={{ fontWeight: '600', textTransform: 'uppercase', marginRight: '0.5rem' }}>
                    {log.service}
                  </span>
                  {log.message}
                </div>
                <div style={{ fontSize: '0.8rem', color: '#6b7280' }}>
                  {log.timestamp.toLocaleString('pt-BR')}
                </div>
              </div>

              <div style={{
                padding: '0.25rem 0.5rem',
                borderRadius: '4px',
                background: getLogLevelColor(log.level) + '20',
                color: getLogLevelColor(log.level),
                fontSize: '0.7rem',
                fontWeight: '600',
                textTransform: 'uppercase'
              }}>
                {log.level}
              </div>
            </div>
          ))}
        </div>

        <div style={{ marginTop: '1rem', display: 'flex', gap: '0.5rem' }}>
          <button className="btn btn-secondary" style={{ fontSize: '0.9rem' }}>
            📄 Ver Todos os Logs
          </button>
          <button className="btn btn-secondary" style={{ fontSize: '0.9rem' }}>
            🔄 Atualizar
          </button>
        </div>
      </div>
    </div>
  );
}
