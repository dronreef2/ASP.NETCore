import React, { useState, useEffect } from 'react';

export default function ReportsSection() {
  const [reports, setReports] = useState([]);
  const [selectedReport, setSelectedReport] = useState(null);
  const [isGenerating, setIsGenerating] = useState(false);
  const [reportType, setReportType] = useState('usage');

  const reportTypes = [
    { value: 'usage', label: '📊 Relatório de Uso', description: 'Estatísticas de utilização do sistema' },
    { value: 'performance', label: '⚡ Relatório de Performance', description: 'Métricas de desempenho e resposta' },
    { value: 'errors', label: '🚨 Relatório de Erros', description: 'Logs de erros e problemas encontrados' },
    { value: 'ai-activity', label: '🤖 Atividade de IA', description: 'Uso dos modelos de IA e resultados' },
    { value: 'user-activity', label: '👥 Atividade de Usuários', description: 'Comportamento e padrões de uso' }
  ];

  useEffect(() => {
    loadReports();
  }, []);

  const loadReports = () => {
    // Simular carregamento de relatórios existentes
    const mockReports = [
      {
        id: 1,
        title: 'Relatório de Uso - Janeiro 2025',
        type: 'usage',
        date: '2025-01-31',
        size: '2.4 MB',
        status: 'completed'
      },
      {
        id: 2,
        title: 'Performance Q4 2024',
        type: 'performance',
        date: '2024-12-31',
        size: '1.8 MB',
        status: 'completed'
      },
      {
        id: 3,
        title: 'Erros do Sistema - Dezembro',
        type: 'errors',
        date: '2024-12-31',
        size: '956 KB',
        status: 'completed'
      }
    ];
    setReports(mockReports);
  };

  const generateReport = async () => {
    setIsGenerating(true);

    try {
      // Simular geração de relatório
      await new Promise(resolve => setTimeout(resolve, 3000));

      const newReport = {
        id: Date.now(),
        title: `${reportTypes.find(t => t.value === reportType).label} - ${new Date().toLocaleDateString('pt-BR')}`,
        type: reportType,
        date: new Date().toISOString().split('T')[0],
        size: '1.2 MB',
        status: 'completed'
      };

      setReports(prev => [newReport, ...prev]);
      setSelectedReport(newReport);
    } catch (error) {
      console.error('Erro ao gerar relatório:', error);
    } finally {
      setIsGenerating(false);
    }
  };

  const downloadReport = (report) => {
    // Simular download
    const element = document.createElement('a');
    const file = new Blob(['Conteúdo do relatório: ' + report.title], { type: 'text/plain' });
    element.href = URL.createObjectURL(file);
    element.download = `${report.title}.txt`;
    element.click();
  };

  const deleteReport = (reportId) => {
    setReports(prev => prev.filter(r => r.id !== reportId));
    if (selectedReport && selectedReport.id === reportId) {
      setSelectedReport(null);
    }
  };

  return (
    <div className="grid">
      <div className="card">
        <div className="card-header">
          <h3 className="card-title">📋 Sistema de Relatórios</h3>
          <p className="card-description">Gere e gerencie relatórios analíticos do sistema</p>
        </div>

        {/* Geração de Relatórios */}
        <div className="form-group">
          <label className="form-label">Tipo de Relatório:</label>
          <select
            value={reportType}
            onChange={(e) => setReportType(e.target.value)}
            className="form-input"
          >
            {reportTypes.map((type) => (
              <option key={type.value} value={type.value}>
                {type.label}
              </option>
            ))}
          </select>
        </div>

        <div style={{ marginBottom: '1rem' }}>
          <div style={{ fontSize: '0.9rem', color: '#6b7280', marginBottom: '0.5rem' }}>
            {reportTypes.find(t => t.value === reportType).description}
          </div>
        </div>

        <button
          onClick={generateReport}
          disabled={isGenerating}
          className="btn btn-primary"
          style={{ width: '100%' }}
        >
          {isGenerating ? (
            <div className="loading">
              <div className="spinner"></div>
              Gerando Relatório...
            </div>
          ) : (
            '📊 Gerar Relatório'
          )}
        </button>
      </div>

      {/* Lista de Relatórios */}
      <div className="card">
        <div className="card-header">
          <h3 className="card-title">📁 Relatórios Disponíveis</h3>
          <p className="card-description">Relatórios gerados anteriormente</p>
        </div>

        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
          {reports.length === 0 ? (
            <div style={{ textAlign: 'center', color: '#6b7280', padding: '2rem' }}>
              Nenhum relatório encontrado. Gere o primeiro relatório para começar.
            </div>
          ) : (
            reports.map((report) => (
              <div
                key={report.id}
                style={{
                  padding: '1rem',
                  border: `2px solid ${selectedReport && selectedReport.id === report.id ? '#667eea' : '#e5e7eb'}`,
                  borderRadius: '8px',
                  background: selectedReport && selectedReport.id === report.id ? '#f0f4ff' : 'white',
                  cursor: 'pointer',
                  transition: 'all 0.3s ease'
                }}
                onClick={() => setSelectedReport(report)}
              >
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                  <div style={{ flex: 1 }}>
                    <div style={{ fontWeight: '600', marginBottom: '0.25rem', color: '#374151' }}>
                      {report.title}
                    </div>
                    <div style={{ fontSize: '0.9rem', color: '#6b7280', marginBottom: '0.5rem' }}>
                      📅 {new Date(report.date).toLocaleDateString('pt-BR')} •
                      📎 {report.size} •
                      {report.status === 'completed' ? '✅ Concluído' : '⏳ Processando'}
                    </div>
                  </div>

                  <div style={{ display: 'flex', gap: '0.5rem' }}>
                    <button
                      onClick={(e) => {
                        e.stopPropagation();
                        downloadReport(report);
                      }}
                      className="btn btn-success"
                      style={{ fontSize: '0.8rem', padding: '0.25rem 0.75rem' }}
                    >
                      ⬇️
                    </button>

                    <button
                      onClick={(e) => {
                        e.stopPropagation();
                        deleteReport(report.id);
                      }}
                      className="btn btn-secondary"
                      style={{ fontSize: '0.8rem', padding: '0.25rem 0.75rem', background: '#ef4444', color: 'white' }}
                    >
                      🗑️
                    </button>
                  </div>
                </div>
              </div>
            ))
          )}
        </div>
      </div>

      {/* Detalhes do Relatório Selecionado */}
      {selectedReport && (
        <div className="card">
          <div className="card-header">
            <h3 className="card-title">📄 Detalhes do Relatório</h3>
            <p className="card-description">{selectedReport.title}</p>
          </div>

          <div className="grid grid-2">
            <div>
              <h4 style={{ margin: '0 0 1rem 0', color: '#374151' }}>📊 Informações Gerais</h4>
              <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                <div><strong>Tipo:</strong> {reportTypes.find(t => t.value === selectedReport.type).label}</div>
                <div><strong>Data de Geração:</strong> {new Date(selectedReport.date).toLocaleDateString('pt-BR')}</div>
                <div><strong>Tamanho:</strong> {selectedReport.size}</div>
                <div><strong>Status:</strong> {selectedReport.status === 'completed' ? '✅ Concluído' : '⏳ Processando'}</div>
              </div>
            </div>

            <div>
              <h4 style={{ margin: '0 0 1rem 0', color: '#374151' }}>📋 Ações Disponíveis</h4>
              <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                <button
                  onClick={() => downloadReport(selectedReport)}
                  className="btn btn-primary"
                >
                  ⬇️ Baixar Relatório
                </button>

                <button
                  onClick={() => setSelectedReport(null)}
                  className="btn btn-secondary"
                >
                  ❌ Fechar Detalhes
                </button>
              </div>
            </div>
          </div>

          {/* Preview do Relatório (simulado) */}
          <div style={{ marginTop: '1.5rem' }}>
            <h4 style={{ margin: '0 0 1rem 0', color: '#374151' }}>👁️ Preview do Conteúdo</h4>
            <div style={{
              background: '#f8fafc',
              padding: '1rem',
              borderRadius: '8px',
              fontFamily: 'monospace',
              fontSize: '0.9rem',
              color: '#374151',
              maxHeight: '300px',
              overflowY: 'auto'
            }}>
              <pre style={{ margin: 0 }}>
{`RELATÓRIO: ${selectedReport.title}
=====================================

DATA DE GERAÇÃO: ${new Date(selectedReport.date).toLocaleDateString('pt-BR')}
TIPO: ${reportTypes.find(t => t.value === selectedReport.type).label}

RESUMO EXECUTIVO
----------------
Este relatório contém informações detalhadas sobre o uso do sistema
Tutor Copiloto durante o período analisado.

ESTATÍSTICAS PRINCIPAIS
-----------------------
- Total de Usuários Ativos: 1,247
- Sessões de Chat: 3,492
- Consultas de IA: 8,156
- Tempo Médio de Resposta: 1.2s

DETALHES TÉCNICOS
-----------------
- Backend .NET Status: Online
- Backend Node.js Status: Online
- Banco de Dados: PostgreSQL
- Cache: Redis Cluster

CONCLUSÃO
---------
O sistema apresentou performance satisfatória durante o período,
com todos os serviços funcionando corretamente.

=====================================
Fim do Relatório`}
              </pre>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
