import React, { useState, useEffect } from 'react';

export default function SecuritySection() {
  const [securityScan, setSecurityScan] = useState(null);
  const [vulnerabilities, setVulnerabilities] = useState([]);
  const [selectedDeployment, setSelectedDeployment] = useState('');
  const [isScanning, setIsScanning] = useState(false);
  const [scanResults, setScanResults] = useState(null);

  useEffect(() => {
    loadVulnerabilities();
  }, []);

  const loadVulnerabilities = () => {
    // Simular carregamento de vulnerabilidades
    const mockVulnerabilities = [
      {
        id: 'CVE-2024-001',
        severity: 'high',
        title: 'SQL Injection Vulnerability',
        description: 'Possible SQL injection in user input validation',
        affectedComponent: 'UserController.cs',
        cvssScore: 8.5,
        status: 'open',
        discoveredAt: new Date(Date.now() - 86400000),
        remediation: 'Implement parameterized queries'
      },
      {
        id: 'CVE-2024-002',
        severity: 'medium',
        title: 'Weak Password Policy',
        description: 'Password requirements are too lenient',
        affectedComponent: 'AuthService.cs',
        cvssScore: 6.2,
        status: 'in-progress',
        discoveredAt: new Date(Date.now() - 43200000),
        remediation: 'Enforce minimum 12 characters with special symbols'
      },
      {
        id: 'CVE-2024-003',
        severity: 'low',
        title: 'Information Disclosure',
        description: 'Error messages reveal internal system information',
        affectedComponent: 'ErrorHandler.cs',
        cvssScore: 3.1,
        status: 'resolved',
        discoveredAt: new Date(Date.now() - 21600000),
        remediation: 'Sanitize error messages before displaying'
      }
    ];
    setVulnerabilities(mockVulnerabilities);
  };

  const runSecurityScan = async () => {
    if (!selectedDeployment) {
      alert('Selecione um deployment para análise');
      return;
    }

    setIsScanning(true);
    try {
      // Simular análise de segurança
      await new Promise(resolve => setTimeout(resolve, 5000));

      const mockResults = {
        deploymentId: selectedDeployment,
        scanTime: new Date(),
        overallScore: 78,
        findings: {
          critical: 0,
          high: 2,
          medium: 3,
          low: 5,
          info: 8
        },
        compliance: {
          owasp: 85,
          pci: 92,
          gdpr: 76
        },
        recommendations: [
          'Implementar rate limiting nas APIs',
          'Adicionar validação de entrada mais rigorosa',
          'Configurar headers de segurança HTTP',
          'Implementar logging de segurança',
          'Revisar permissões de banco de dados'
        ]
      };

      setScanResults(mockResults);
    } catch (error) {
      console.error('Erro na análise de segurança:', error);
    } finally {
      setIsScanning(false);
    }
  };

  const getSeverityColor = (severity) => {
    switch (severity) {
      case 'critical': return '#7c2d12';
      case 'high': return '#dc2626';
      case 'medium': return '#d97706';
      case 'low': return '#65a30d';
      case 'info': return '#6b7280';
      default: return '#6b7280';
    }
  };

  const getSeverityBg = (severity) => {
    switch (severity) {
      case 'critical': return '#fef2f2';
      case 'high': return '#fef2f2';
      case 'medium': return '#fffbeb';
      case 'low': return '#f0fdf4';
      case 'info': return '#f9fafb';
      default: return '#f9fafb';
    }
  };

  const getStatusColor = (status) => {
    switch (status) {
      case 'open': return '#dc2626';
      case 'in-progress': return '#d97706';
      case 'resolved': return '#10b981';
      default: return '#6b7280';
    }
  };

  const getStatusText = (status) => {
    switch (status) {
      case 'open': return 'Aberto';
      case 'in-progress': return 'Em Andamento';
      case 'resolved': return 'Resolvido';
      default: return 'Desconhecido';
    }
  };

  return (
    <div className="grid">
      <div className="card">
        <div className="card-header">
          <h3 className="card-title">🔒 Análise de Segurança</h3>
          <p className="card-description">Scanner de vulnerabilidades e análise de segurança</p>
        </div>

        {/* Scanner de Segurança */}
        <div className="form-group">
          <h4 style={{ margin: '0 0 1rem 0', color: '#374151' }}>🔍 Scanner de Segurança</h4>

          <div style={{ display: 'flex', gap: '1rem', alignItems: 'flex-end' }}>
            <div style={{ flex: 1 }}>
              <label className="form-label">Deployment para Análise</label>
              <select
                value={selectedDeployment}
                onChange={(e) => setSelectedDeployment(e.target.value)}
                className="form-input"
              >
                <option value="">Selecione um deployment...</option>
                <option value="dep-001">ASP.NETCore - v1.2.3</option>
                <option value="dep-002">React App - v2.1.0</option>
                <option value="dep-003">Node.js API - v1.5.2</option>
              </select>
            </div>

            <button
              onClick={runSecurityScan}
              disabled={isScanning || !selectedDeployment}
              className="btn btn-primary"
            >
              {isScanning ? (
                <div className="loading">
                  <div className="spinner"></div>
                  Escaneando...
                </div>
              ) : (
                '🔍 Iniciar Scan'
              )}
            </button>
          </div>
        </div>
      </div>

      {/* Resultados do Scan */}
      {scanResults && (
        <div className="card">
          <div className="card-header">
            <h3 className="card-title">📊 Resultados da Análise</h3>
            <p className="card-description">Scan realizado em {scanResults.scanTime.toLocaleString('pt-BR')}</p>
          </div>

          {/* Score Geral */}
          <div style={{ textAlign: 'center', marginBottom: '2rem' }}>
            <div style={{
              width: '120px',
              height: '120px',
              borderRadius: '50%',
              background: `conic-gradient(#10b981 0% ${scanResults.overallScore}%, #e5e7eb ${scanResults.overallScore}% 100%)`,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              margin: '0 auto 1rem',
              fontSize: '1.5rem',
              fontWeight: '700',
              color: '#374151'
            }}>
              {scanResults.overallScore}
            </div>
            <div style={{ fontSize: '1.2rem', fontWeight: '600', color: '#374151' }}>
              Score de Segurança
            </div>
            <div style={{ color: '#6b7280' }}>
              {scanResults.overallScore >= 80 ? '🟢 Seguro' :
               scanResults.overallScore >= 60 ? '🟡 Atenção' : '🔴 Crítico'}
            </div>
          </div>

          {/* Findings por Severidade */}
          <div style={{ marginBottom: '2rem' }}>
            <h4 style={{ margin: '0 0 1rem 0', color: '#374151' }}>🚨 Findings por Severidade</h4>
            <div className="grid grid-5">
              <div style={{ textAlign: 'center', padding: '1rem', background: '#fef2f2', borderRadius: '8px' }}>
                <div style={{ fontSize: '1.5rem', fontWeight: '700', color: '#7c2d12' }}>
                  {scanResults.findings.critical}
                </div>
                <div style={{ fontSize: '0.9rem', color: '#7c2d12' }}>Críticos</div>
              </div>

              <div style={{ textAlign: 'center', padding: '1rem', background: '#fef2f2', borderRadius: '8px' }}>
                <div style={{ fontSize: '1.5rem', fontWeight: '700', color: '#dc2626' }}>
                  {scanResults.findings.high}
                </div>
                <div style={{ fontSize: '0.9rem', color: '#dc2626' }}>Altos</div>
              </div>

              <div style={{ textAlign: 'center', padding: '1rem', background: '#fffbeb', borderRadius: '8px' }}>
                <div style={{ fontSize: '1.5rem', fontWeight: '700', color: '#d97706' }}>
                  {scanResults.findings.medium}
                </div>
                <div style={{ fontSize: '0.9rem', color: '#d97706' }}>Médios</div>
              </div>

              <div style={{ textAlign: 'center', padding: '1rem', background: '#f0fdf4', borderRadius: '8px' }}>
                <div style={{ fontSize: '1.5rem', fontWeight: '700', color: '#65a30d' }}>
                  {scanResults.findings.low}
                </div>
                <div style={{ fontSize: '0.9rem', color: '#65a30d' }}>Baixos</div>
              </div>

              <div style={{ textAlign: 'center', padding: '1rem', background: '#f9fafb', borderRadius: '8px' }}>
                <div style={{ fontSize: '1.5rem', fontWeight: '700', color: '#6b7280' }}>
                  {scanResults.findings.info}
                </div>
                <div style={{ fontSize: '0.9rem', color: '#6b7280' }}>Info</div>
              </div>
            </div>
          </div>

          {/* Compliance */}
          <div style={{ marginBottom: '2rem' }}>
            <h4 style={{ margin: '0 0 1rem 0', color: '#374151' }}>📋 Compliance</h4>
            <div className="grid grid-3">
              <div style={{ textAlign: 'center', padding: '1rem', background: '#f8fafc', borderRadius: '8px' }}>
                <div style={{ fontSize: '1.5rem', fontWeight: '700', color: '#374151' }}>
                  {scanResults.compliance.owasp}%
                </div>
                <div style={{ fontSize: '0.9rem', color: '#6b7280' }}>OWASP</div>
              </div>

              <div style={{ textAlign: 'center', padding: '1rem', background: '#f8fafc', borderRadius: '8px' }}>
                <div style={{ fontSize: '1.5rem', fontWeight: '700', color: '#374151' }}>
                  {scanResults.compliance.pci}%
                </div>
                <div style={{ fontSize: '0.9rem', color: '#6b7280' }}>PCI DSS</div>
              </div>

              <div style={{ textAlign: 'center', padding: '1rem', background: '#f8fafc', borderRadius: '8px' }}>
                <div style={{ fontSize: '1.5rem', fontWeight: '700', color: '#374151' }}>
                  {scanResults.compliance.gdpr}%
                </div>
                <div style={{ fontSize: '0.9rem', color: '#6b7280' }}>GDPR</div>
              </div>
            </div>
          </div>

          {/* Recomendações */}
          <div>
            <h4 style={{ margin: '0 0 1rem 0', color: '#374151' }}>💡 Recomendações de Segurança</h4>
            <ul style={{ color: '#6b7280', lineHeight: '1.6' }}>
              {scanResults.recommendations.map((rec, index) => (
                <li key={index}>• {rec}</li>
              ))}
            </ul>
          </div>
        </div>
      )}

      {/* Vulnerabilidades Conhecidas */}
      <div className="card">
        <div className="card-header">
          <h3 className="card-title">🚨 Vulnerabilidades Conhecidas</h3>
          <p className="card-description">Issues de segurança identificadas no sistema</p>
        </div>

        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
          {vulnerabilities.map((vuln) => (
            <div key={vuln.id} style={{
              padding: '1rem',
              border: `2px solid ${getSeverityColor(vuln.severity)}`,
              borderRadius: '8px',
              background: getSeverityBg(vuln.severity)
            }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '0.5rem' }}>
                <div>
                  <div style={{ fontWeight: '600', marginBottom: '0.25rem', color: '#374151' }}>
                    {vuln.id}: {vuln.title}
                  </div>
                  <div style={{ fontSize: '0.9rem', color: '#6b7280', marginBottom: '0.5rem' }}>
                    {vuln.description}
                  </div>
                  <div style={{ fontSize: '0.8rem', color: '#9ca3af' }}>
                    📁 {vuln.affectedComponent} • 🔢 CVSS: {vuln.cvssScore}
                  </div>
                </div>

                <div style={{ textAlign: 'right' }}>
                  <div style={{
                    padding: '0.25rem 0.75rem',
                    borderRadius: '12px',
                    background: getSeverityColor(vuln.severity),
                    color: 'white',
                    fontSize: '0.8rem',
                    fontWeight: '500',
                    marginBottom: '0.5rem'
                  }}>
                    {vuln.severity.toUpperCase()}
                  </div>
                  <div style={{
                    padding: '0.25rem 0.75rem',
                    borderRadius: '12px',
                    background: getStatusColor(vuln.status),
                    color: 'white',
                    fontSize: '0.8rem',
                    fontWeight: '500'
                  }}>
                    {getStatusText(vuln.status)}
                  </div>
                </div>
              </div>

              <div style={{ fontSize: '0.9rem', color: '#6b7280', marginBottom: '0.5rem' }}>
                🛠️ <strong>Remediação:</strong> {vuln.remediation}
              </div>

              <div style={{ fontSize: '0.8rem', color: '#9ca3af' }}>
                📅 Descoberto em {vuln.discoveredAt.toLocaleDateString('pt-BR')}
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Ações de Segurança */}
      <div className="card">
        <div className="card-header">
          <h3 className="card-title">🛡️ Ações de Segurança</h3>
          <p className="card-description">Ferramentas e ações para melhorar a segurança</p>
        </div>

        <div className="grid grid-2">
          <button className="btn btn-primary">
            🔐 Gerar Relatório de Segurança
          </button>

          <button className="btn btn-primary">
            📊 Dashboard de Segurança
          </button>

          <button className="btn btn-secondary">
            🚨 Configurar Alertas
          </button>

          <button className="btn btn-secondary">
            📚 Base de Conhecimento
          </button>
        </div>
      </div>
    </div>
  );
}
