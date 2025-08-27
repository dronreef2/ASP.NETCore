import React, { useState, useEffect } from 'react';

export default function DatabaseSection() {
  const [dbStats, setDbStats] = useState(null);
  const [redisStats, setRedisStats] = useState(null);
  const [selectedDatabase, setSelectedDatabase] = useState('postgresql');
  const [queryResults, setQueryResults] = useState(null);
  const [customQuery, setCustomQuery] = useState('');
  const [isExecuting, setIsExecuting] = useState(false);
  const [backups, setBackups] = useState([]);

  useEffect(() => {
    loadDatabaseStats();
    loadBackups();
  }, []);

  const loadDatabaseStats = () => {
    // Simular estatísticas do PostgreSQL
    const mockPgStats = {
      connections: {
        active: 12,
        idle: 8,
        total: 20,
        max: 100
      },
      size: {
        database: '2.4 GB',
        tables: '1.8 GB',
        indexes: '456 MB',
        unused: '180 MB'
      },
      performance: {
        cacheHitRatio: 98.5,
        avgQueryTime: 45,
        slowQueries: 3,
        deadlocks: 0
      },
      tables: [
        { name: 'users', rows: 15420, size: '234 MB' },
        { name: 'deployments', rows: 892, size: '45 MB' },
        { name: 'logs', rows: 45632, size: '1.2 GB' },
        { name: 'reports', rows: 2341, size: '89 MB' }
      ]
    };

    // Simular estatísticas do Redis
    const mockRedisStats = {
      memory: {
        used: '456 MB',
        peak: '512 MB',
        fragmentation: 1.12
      },
      connections: {
        active: 45,
        total: 1234
      },
      keys: {
        total: 56789,
        expires: 12345,
        avgTtl: 86400
      },
      performance: {
        opsPerSec: 1250,
        hitRate: 94.2,
        missRate: 5.8
      },
      slowlog: [
        { id: 1, timestamp: new Date(Date.now() - 300000), duration: 1250, command: 'LRANGE users:recent 0 10' },
        { id: 2, timestamp: new Date(Date.now() - 600000), duration: 890, command: 'HGETALL user:1234' }
      ]
    };

    setDbStats(mockPgStats);
    setRedisStats(mockRedisStats);
  };

  const loadBackups = () => {
    const mockBackups = [
      {
        id: 'bk-001',
        type: 'postgresql',
        name: 'Backup Diário - 2024-01-15',
        size: '2.1 GB',
        createdAt: new Date(Date.now() - 86400000),
        status: 'completed',
        location: 's3://backups/postgresql/daily-2024-01-15.sql.gz'
      },
      {
        id: 'bk-002',
        type: 'redis',
        name: 'Snapshot Redis - 2024-01-15',
        size: '456 MB',
        createdAt: new Date(Date.now() - 86400000),
        status: 'completed',
        location: 's3://backups/redis/snapshot-2024-01-15.rdb'
      },
      {
        id: 'bk-003',
        type: 'postgresql',
        name: 'Backup Semanal - 2024-01-08',
        size: '8.9 GB',
        createdAt: new Date(Date.now() - 604800000),
        status: 'completed',
        location: 's3://backups/postgresql/weekly-2024-01-08.sql.gz'
      }
    ];
    setBackups(mockBackups);
  };

  const executeQuery = async () => {
    if (!customQuery.trim()) {
      alert('Digite uma consulta para executar');
      return;
    }

    setIsExecuting(true);
    try {
      // Simular execução de query
      await new Promise(resolve => setTimeout(resolve, 2000));

      const mockResults = {
        query: customQuery,
        executionTime: Math.random() * 100 + 10,
        rowsAffected: selectedDatabase === 'postgresql' ? Math.floor(Math.random() * 1000) : null,
        results: selectedDatabase === 'postgresql' ? [
          { id: 1, name: 'João Silva', email: 'joao@example.com', created_at: '2024-01-15' },
          { id: 2, name: 'Maria Santos', email: 'maria@example.com', created_at: '2024-01-14' },
          { id: 3, name: 'Pedro Costa', email: 'pedro@example.com', created_at: '2024-01-13' }
        ] : [
          { key: 'user:1', type: 'hash', fields: 5 },
          { key: 'user:2', type: 'hash', fields: 3 },
          { key: 'session:abc123', type: 'string', value: 'active' }
        ]
      };

      setQueryResults(mockResults);
    } catch (error) {
      console.error('Erro na execução da query:', error);
    } finally {
      setIsExecuting(false);
    }
  };

  const createBackup = async (type) => {
    try {
      // Simular criação de backup
      await new Promise(resolve => setTimeout(resolve, 3000));
      alert(`Backup ${type.toUpperCase()} criado com sucesso!`);
      loadBackups();
    } catch (error) {
      console.error('Erro ao criar backup:', error);
    }
  };

  const getStatusColor = (status) => {
    switch (status) {
      case 'completed': return '#10b981';
      case 'running': return '#d97706';
      case 'failed': return '#dc2626';
      default: return '#6b7280';
    }
  };

  const getStatusText = (status) => {
    switch (status) {
      case 'completed': return 'Concluído';
      case 'running': return 'Executando';
      case 'failed': return 'Falhou';
      default: return 'Desconhecido';
    }
  };

  return (
    <div className="grid">
      {/* Seleção de Banco */}
      <div className="card">
        <div className="card-header">
          <h3 className="card-title">🗄️ Gerenciamento de Banco de Dados</h3>
          <p className="card-description">PostgreSQL e Redis - Monitoramento e administração</p>
        </div>

        <div style={{ display: 'flex', gap: '1rem', marginBottom: '1rem' }}>
          <button
            onClick={() => setSelectedDatabase('postgresql')}
            className={`btn ${selectedDatabase === 'postgresql' ? 'btn-primary' : 'btn-secondary'}`}
          >
            🐘 PostgreSQL
          </button>
          <button
            onClick={() => setSelectedDatabase('redis')}
            className={`btn ${selectedDatabase === 'redis' ? 'btn-primary' : 'btn-secondary'}`}
          >
            🔴 Redis
          </button>
        </div>
      </div>

      {/* Estatísticas PostgreSQL */}
      {selectedDatabase === 'postgresql' && dbStats && (
        <div className="card">
          <div className="card-header">
            <h3 className="card-title">📊 PostgreSQL - Estatísticas</h3>
          </div>

          {/* Conexões */}
          <div style={{ marginBottom: '2rem' }}>
            <h4 style={{ margin: '0 0 1rem 0', color: '#374151' }}>🔗 Conexões</h4>
            <div className="grid grid-4">
              <div style={{ textAlign: 'center', padding: '1rem', background: '#f8fafc', borderRadius: '8px' }}>
                <div style={{ fontSize: '1.5rem', fontWeight: '700', color: '#10b981' }}>
                  {dbStats.connections.active}
                </div>
                <div style={{ fontSize: '0.9rem', color: '#6b7280' }}>Ativas</div>
              </div>

              <div style={{ textAlign: 'center', padding: '1rem', background: '#f8fafc', borderRadius: '8px' }}>
                <div style={{ fontSize: '1.5rem', fontWeight: '700', color: '#6b7280' }}>
                  {dbStats.connections.idle}
                </div>
                <div style={{ fontSize: '0.9rem', color: '#6b7280' }}>Ociosas</div>
              </div>

              <div style={{ textAlign: 'center', padding: '1rem', background: '#f8fafc', borderRadius: '8px' }}>
                <div style={{ fontSize: '1.5rem', fontWeight: '700', color: '#374151' }}>
                  {dbStats.connections.total}
                </div>
                <div style={{ fontSize: '0.9rem', color: '#6b7280' }}>Total</div>
              </div>

              <div style={{ textAlign: 'center', padding: '1rem', background: '#f8fafc', borderRadius: '8px' }}>
                <div style={{ fontSize: '1.5rem', fontWeight: '700', color: '#374151' }}>
                  {dbStats.connections.max}
                </div>
                <div style={{ fontSize: '0.9rem', color: '#6b7280' }}>Máximo</div>
              </div>
            </div>
          </div>

          {/* Tamanho do Banco */}
          <div style={{ marginBottom: '2rem' }}>
            <h4 style={{ margin: '0 0 1rem 0', color: '#374151' }}>💾 Tamanho do Banco</h4>
            <div className="grid grid-4">
              <div style={{ textAlign: 'center', padding: '1rem', background: '#f8fafc', borderRadius: '8px' }}>
                <div style={{ fontSize: '1.2rem', fontWeight: '700', color: '#374151' }}>
                  {dbStats.size.database}
                </div>
                <div style={{ fontSize: '0.9rem', color: '#6b7280' }}>Database</div>
              </div>

              <div style={{ textAlign: 'center', padding: '1rem', background: '#f8fafc', borderRadius: '8px' }}>
                <div style={{ fontSize: '1.2rem', fontWeight: '700', color: '#374151' }}>
                  {dbStats.size.tables}
                </div>
                <div style={{ fontSize: '0.9rem', color: '#6b7280' }}>Tabelas</div>
              </div>

              <div style={{ textAlign: 'center', padding: '1rem', background: '#f8fafc', borderRadius: '8px' }}>
                <div style={{ fontSize: '1.2rem', fontWeight: '700', color: '#374151' }}>
                  {dbStats.size.indexes}
                </div>
                <div style={{ fontSize: '0.9rem', color: '#6b7280' }}>Índices</div>
              </div>

              <div style={{ textAlign: 'center', padding: '1rem', background: '#f8fafc', borderRadius: '8px' }}>
                <div style={{ fontSize: '1.2rem', fontWeight: '700', color: '#374151' }}>
                  {dbStats.size.unused}
                </div>
                <div style={{ fontSize: '0.9rem', color: '#6b7280' }}>Não usado</div>
              </div>
            </div>
          </div>

          {/* Performance */}
          <div style={{ marginBottom: '2rem' }}>
            <h4 style={{ margin: '0 0 1rem 0', color: '#374151' }}>⚡ Performance</h4>
            <div className="grid grid-4">
              <div style={{ textAlign: 'center', padding: '1rem', background: '#f8fafc', borderRadius: '8px' }}>
                <div style={{ fontSize: '1.5rem', fontWeight: '700', color: '#10b981' }}>
                  {dbStats.performance.cacheHitRatio}%
                </div>
                <div style={{ fontSize: '0.9rem', color: '#6b7280' }}>Cache Hit</div>
              </div>

              <div style={{ textAlign: 'center', padding: '1rem', background: '#f8fafc', borderRadius: '8px' }}>
                <div style={{ fontSize: '1.5rem', fontWeight: '700', color: '#374151' }}>
                  {dbStats.performance.avgQueryTime}ms
                </div>
                <div style={{ fontSize: '0.9rem', color: '#6b7280' }}>Query Média</div>
              </div>

              <div style={{ textAlign: 'center', padding: '1rem', background: '#f8fafc', borderRadius: '8px' }}>
                <div style={{ fontSize: '1.5rem', fontWeight: '700', color: '#d97706' }}>
                  {dbStats.performance.slowQueries}
                </div>
                <div style={{ fontSize: '0.9rem', color: '#6b7280' }}>Queries Lentas</div>
              </div>

              <div style={{ textAlign: 'center', padding: '1rem', background: '#f8fafc', borderRadius: '8px' }}>
                <div style={{ fontSize: '1.5rem', fontWeight: '700', color: '#dc2626' }}>
                  {dbStats.performance.deadlocks}
                </div>
                <div style={{ fontSize: '0.9rem', color: '#6b7280' }}>Deadlocks</div>
              </div>
            </div>
          </div>

          {/* Principais Tabelas */}
          <div>
            <h4 style={{ margin: '0 0 1rem 0', color: '#374151' }}>📋 Principais Tabelas</h4>
            <div style={{ overflowX: 'auto' }}>
              <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                <thead>
                  <tr style={{ background: '#f9fafb' }}>
                    <th style={{ padding: '0.75rem', textAlign: 'left', borderBottom: '1px solid #e5e7eb' }}>Tabela</th>
                    <th style={{ padding: '0.75rem', textAlign: 'right', borderBottom: '1px solid #e5e7eb' }}>Linhas</th>
                    <th style={{ padding: '0.75rem', textAlign: 'right', borderBottom: '1px solid #e5e7eb' }}>Tamanho</th>
                  </tr>
                </thead>
                <tbody>
                  {dbStats.tables.map((table, index) => (
                    <tr key={index} style={{ borderBottom: '1px solid #f3f4f6' }}>
                      <td style={{ padding: '0.75rem', fontWeight: '500' }}>{table.name}</td>
                      <td style={{ padding: '0.75rem', textAlign: 'right' }}>{table.rows.toLocaleString()}</td>
                      <td style={{ padding: '0.75rem', textAlign: 'right' }}>{table.size}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {/* Estatísticas Redis */}
      {selectedDatabase === 'redis' && redisStats && (
        <div className="card">
          <div className="card-header">
            <h3 className="card-title">📊 Redis - Estatísticas</h3>
          </div>

          {/* Memória */}
          <div style={{ marginBottom: '2rem' }}>
            <h4 style={{ margin: '0 0 1rem 0', color: '#374151' }}>🧠 Memória</h4>
            <div className="grid grid-3">
              <div style={{ textAlign: 'center', padding: '1rem', background: '#f8fafc', borderRadius: '8px' }}>
                <div style={{ fontSize: '1.5rem', fontWeight: '700', color: '#374151' }}>
                  {redisStats.memory.used}
                </div>
                <div style={{ fontSize: '0.9rem', color: '#6b7280' }}>Usada</div>
              </div>

              <div style={{ textAlign: 'center', padding: '1rem', background: '#f8fafc', borderRadius: '8px' }}>
                <div style={{ fontSize: '1.5rem', fontWeight: '700', color: '#374151' }}>
                  {redisStats.memory.peak}
                </div>
                <div style={{ fontSize: '0.9rem', color: '#6b7280' }}>Pico</div>
              </div>

              <div style={{ textAlign: 'center', padding: '1rem', background: '#f8fafc', borderRadius: '8px' }}>
                <div style={{ fontSize: '1.5rem', fontWeight: '700', color: '#374151' }}>
                  {redisStats.memory.fragmentation.toFixed(2)}
                </div>
                <div style={{ fontSize: '0.9rem', color: '#6b7280' }}>Fragmentação</div>
              </div>
            </div>
          </div>

          {/* Chaves */}
          <div style={{ marginBottom: '2rem' }}>
            <h4 style={{ margin: '0 0 1rem 0', color: '#374151' }}>🔑 Chaves</h4>
            <div className="grid grid-3">
              <div style={{ textAlign: 'center', padding: '1rem', background: '#f8fafc', borderRadius: '8px' }}>
                <div style={{ fontSize: '1.5rem', fontWeight: '700', color: '#374151' }}>
                  {redisStats.keys.total.toLocaleString()}
                </div>
                <div style={{ fontSize: '0.9rem', color: '#6b7280' }}>Total</div>
              </div>

              <div style={{ textAlign: 'center', padding: '1rem', background: '#f8fafc', borderRadius: '8px' }}>
                <div style={{ fontSize: '1.5rem', fontWeight: '700', color: '#374151' }}>
                  {redisStats.keys.expires.toLocaleString()}
                </div>
                <div style={{ fontSize: '0.9rem', color: '#6b7280' }}>Com Expiração</div>
              </div>

              <div style={{ textAlign: 'center', padding: '1rem', background: '#f8fafc', borderRadius: '8px' }}>
                <div style={{ fontSize: '1.5rem', fontWeight: '700', color: '#374151' }}>
                  {Math.floor(redisStats.keys.avgTtl / 3600)}h
                </div>
                <div style={{ fontSize: '0.9rem', color: '#6b7280' }}>TTL Médio</div>
              </div>
            </div>
          </div>

          {/* Performance */}
          <div style={{ marginBottom: '2rem' }}>
            <h4 style={{ margin: '0 0 1rem 0', color: '#374151' }}>⚡ Performance</h4>
            <div className="grid grid-3">
              <div style={{ textAlign: 'center', padding: '1rem', background: '#f8fafc', borderRadius: '8px' }}>
                <div style={{ fontSize: '1.5rem', fontWeight: '700', color: '#10b981' }}>
                  {redisStats.performance.opsPerSec}
                </div>
                <div style={{ fontSize: '0.9rem', color: '#6b7280' }}>Ops/seg</div>
              </div>

              <div style={{ textAlign: 'center', padding: '1rem', background: '#f8fafc', borderRadius: '8px' }}>
                <div style={{ fontSize: '1.5rem', fontWeight: '700', color: '#10b981' }}>
                  {redisStats.performance.hitRate}%
                </div>
                <div style={{ fontSize: '0.9rem', color: '#6b7280' }}>Hit Rate</div>
              </div>

              <div style={{ textAlign: 'center', padding: '1rem', background: '#f8fafc', borderRadius: '8px' }}>
                <div style={{ fontSize: '1.5rem', fontWeight: '700', color: '#dc2626' }}>
                  {redisStats.performance.missRate}%
                </div>
                <div style={{ fontSize: '0.9rem', color: '#6b7280' }}>Miss Rate</div>
              </div>
            </div>
          </div>

          {/* Slow Log */}
          <div>
            <h4 style={{ margin: '0 0 1rem 0', color: '#374151' }}>🐌 Slow Log</h4>
            <div style={{ overflowX: 'auto' }}>
              <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                <thead>
                  <tr style={{ background: '#f9fafb' }}>
                    <th style={{ padding: '0.75rem', textAlign: 'left', borderBottom: '1px solid #e5e7eb' }}>ID</th>
                    <th style={{ padding: '0.75rem', textAlign: 'left', borderBottom: '1px solid #e5e7eb' }}>Timestamp</th>
                    <th style={{ padding: '0.75rem', textAlign: 'right', borderBottom: '1px solid #e5e7eb' }}>Duração (μs)</th>
                    <th style={{ padding: '0.75rem', textAlign: 'left', borderBottom: '1px solid #e5e7eb' }}>Comando</th>
                  </tr>
                </thead>
                <tbody>
                  {redisStats.slowlog.map((entry) => (
                    <tr key={entry.id} style={{ borderBottom: '1px solid #f3f4f6' }}>
                      <td style={{ padding: '0.75rem', fontWeight: '500' }}>{entry.id}</td>
                      <td style={{ padding: '0.75rem' }}>{entry.timestamp.toLocaleString('pt-BR')}</td>
                      <td style={{ padding: '0.75rem', textAlign: 'right' }}>{entry.duration.toLocaleString()}</td>
                      <td style={{ padding: '0.75rem', fontFamily: 'monospace', fontSize: '0.9rem' }}>{entry.command}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {/* Query Executor */}
      <div className="card">
        <div className="card-header">
          <h3 className="card-title">🔍 Executor de Queries</h3>
          <p className="card-description">Execute queries customizadas no {selectedDatabase === 'postgresql' ? 'PostgreSQL' : 'Redis'}</p>
        </div>

        <div style={{ marginBottom: '1rem' }}>
          <label className="form-label">Query SQL/Redis</label>
          <textarea
            value={customQuery}
            onChange={(e) => setCustomQuery(e.target.value)}
            placeholder={selectedDatabase === 'postgresql'
              ? 'SELECT * FROM users LIMIT 10;'
              : 'GET user:1234'
            }
            className="form-input"
            rows={4}
            style={{ fontFamily: 'monospace' }}
          />
        </div>

        <button
          onClick={executeQuery}
          disabled={isExecuting || !customQuery.trim()}
          className="btn btn-primary"
        >
          {isExecuting ? (
            <div className="loading">
              <div className="spinner"></div>
              Executando...
            </div>
          ) : (
            '▶️ Executar Query'
          )}
        </button>

        {/* Resultados da Query */}
        {queryResults && (
          <div style={{ marginTop: '2rem' }}>
            <h4 style={{ margin: '0 0 1rem 0', color: '#374151' }}>📊 Resultados</h4>

            <div style={{ marginBottom: '1rem', fontSize: '0.9rem', color: '#6b7280' }}>
              ⏱️ Tempo de execução: {queryResults.executionTime.toFixed(2)}ms
              {queryResults.rowsAffected !== null && (
                <> • 📊 Linhas afetadas: {queryResults.rowsAffected}</>
              )}
            </div>

            <div style={{ overflowX: 'auto' }}>
              <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                <thead>
                  <tr style={{ background: '#f9fafb' }}>
                    {queryResults.results.length > 0 && Object.keys(queryResults.results[0]).map((key) => (
                      <th key={key} style={{ padding: '0.75rem', textAlign: 'left', borderBottom: '1px solid #e5e7eb' }}>
                        {key}
                      </th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {queryResults.results.map((row, index) => (
                    <tr key={index} style={{ borderBottom: '1px solid #f3f4f6' }}>
                      {Object.values(row).map((value, cellIndex) => (
                        <td key={cellIndex} style={{ padding: '0.75rem', fontFamily: 'monospace', fontSize: '0.9rem' }}>
                          {String(value)}
                        </td>
                      ))}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </div>

      {/* Backups */}
      <div className="card">
        <div className="card-header">
          <h3 className="card-title">💾 Backups</h3>
          <p className="card-description">Gerenciamento de backups automáticos</p>
        </div>

        {/* Ações de Backup */}
        <div style={{ marginBottom: '2rem' }}>
          <div className="grid grid-2">
            <button
              onClick={() => createBackup('postgresql')}
              className="btn btn-primary"
            >
              🐘 Criar Backup PostgreSQL
            </button>

            <button
              onClick={() => createBackup('redis')}
              className="btn btn-primary"
            >
              🔴 Criar Backup Redis
            </button>
          </div>
        </div>

        {/* Lista de Backups */}
        <div>
          <h4 style={{ margin: '0 0 1rem 0', color: '#374151' }}>📋 Backups Recentes</h4>

          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
            {backups.map((backup) => (
              <div key={backup.id} style={{
                padding: '1rem',
                border: '1px solid #e5e7eb',
                borderRadius: '8px',
                background: '#f9fafb'
              }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '0.5rem' }}>
                  <div>
                    <div style={{ fontWeight: '600', marginBottom: '0.25rem', color: '#374151' }}>
                      {backup.name}
                    </div>
                    <div style={{ fontSize: '0.9rem', color: '#6b7280', marginBottom: '0.5rem' }}>
                      📁 {backup.location}
                    </div>
                    <div style={{ fontSize: '0.8rem', color: '#9ca3af' }}>
                      📅 Criado em {backup.createdAt.toLocaleDateString('pt-BR')} às {backup.createdAt.toLocaleTimeString('pt-BR')}
                    </div>
                  </div>

                  <div style={{ textAlign: 'right' }}>
                    <div style={{
                      padding: '0.25rem 0.75rem',
                      borderRadius: '12px',
                      background: getStatusColor(backup.status),
                      color: 'white',
                      fontSize: '0.8rem',
                      fontWeight: '500',
                      marginBottom: '0.5rem'
                    }}>
                      {getStatusText(backup.status)}
                    </div>
                    <div style={{ fontSize: '0.9rem', color: '#6b7280' }}>
                      📦 {backup.size}
                    </div>
                  </div>
                </div>

                <div style={{ display: 'flex', gap: '0.5rem' }}>
                  <button className="btn btn-secondary" style={{ fontSize: '0.8rem', padding: '0.25rem 0.75rem' }}>
                    ⬇️ Download
                  </button>
                  <button className="btn btn-secondary" style={{ fontSize: '0.8rem', padding: '0.25rem 0.75rem' }}>
                    🔄 Restaurar
                  </button>
                  <button className="btn btn-secondary" style={{ fontSize: '0.8rem', padding: '0.25rem 0.75rem' }}>
                    🗑️ Excluir
                  </button>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
