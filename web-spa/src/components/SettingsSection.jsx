import React, { useState } from 'react';

export default function SettingsSection() {
  const [settings, setSettings] = useState({
    theme: 'light',
    language: 'pt-BR',
    notifications: true,
    autoSave: true,
    maxFileSize: 10,
    apiTimeout: 30,
    enableTelemetry: true,
    debugMode: false
  });

  const [isSaving, setIsSaving] = useState(false);
  const [saveMessage, setSaveMessage] = useState('');

  const handleSettingChange = (key, value) => {
    setSettings(prev => ({
      ...prev,
      [key]: value
    }));
  };

  const saveSettings = async () => {
    setIsSaving(true);
    setSaveMessage('');

    try {
      // Simular salvamento
      await new Promise(resolve => setTimeout(resolve, 1000));

      // Salvar no localStorage (simulação)
      localStorage.setItem('tutor-copiloto-settings', JSON.stringify(settings));

      setSaveMessage('✅ Configurações salvas com sucesso!');
    } catch (error) {
      setSaveMessage('❌ Erro ao salvar configurações.');
    } finally {
      setIsSaving(false);
    }
  };

  const resetSettings = () => {
    const defaultSettings = {
      theme: 'light',
      language: 'pt-BR',
      notifications: true,
      autoSave: true,
      maxFileSize: 10,
      apiTimeout: 30,
      enableTelemetry: true,
      debugMode: false
    };

    setSettings(defaultSettings);
    setSaveMessage('🔄 Configurações restauradas para padrão.');
  };

  const exportSettings = () => {
    const dataStr = JSON.stringify(settings, null, 2);
    const dataUri = 'data:application/json;charset=utf-8,'+ encodeURIComponent(dataStr);

    const exportFileDefaultName = 'tutor-copiloto-settings.json';

    const linkElement = document.createElement('a');
    linkElement.setAttribute('href', dataUri);
    linkElement.setAttribute('download', exportFileDefaultName);
    linkElement.click();
  };

  const importSettings = (event) => {
    const file = event.target.files[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = (e) => {
        try {
          const importedSettings = JSON.parse(e.target.result);
          setSettings(importedSettings);
          setSaveMessage('✅ Configurações importadas com sucesso!');
        } catch (error) {
          setSaveMessage('❌ Erro ao importar configurações.');
        }
      };
      reader.readAsText(file);
    }
  };

  return (
    <div className="grid">
      <div className="card">
        <div className="card-header">
          <h3 className="card-title">⚙️ Configurações do Sistema</h3>
          <p className="card-description">Personalize a experiência e comportamento da aplicação</p>
        </div>

        {/* Aparência */}
        <div className="form-group">
          <h4 style={{ margin: '0 0 1rem 0', color: '#374151' }}>🎨 Aparência</h4>

          <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
            <div>
              <label className="form-label">Tema:</label>
              <select
                value={settings.theme}
                onChange={(e) => handleSettingChange('theme', e.target.value)}
                className="form-input"
              >
                <option value="light">☀️ Claro</option>
                <option value="dark">🌙 Escuro</option>
                <option value="auto">🔄 Automático</option>
              </select>
            </div>

            <div>
              <label className="form-label">Idioma:</label>
              <select
                value={settings.language}
                onChange={(e) => handleSettingChange('language', e.target.value)}
                className="form-input"
              >
                <option value="pt-BR">🇧🇷 Português (Brasil)</option>
                <option value="en-US">🇺🇸 English</option>
                <option value="es-ES">🇪🇸 Español</option>
              </select>
            </div>
          </div>
        </div>

        {/* Comportamento */}
        <div className="form-group">
          <h4 style={{ margin: '0 0 1rem 0', color: '#374151' }}>🔧 Comportamento</h4>

          <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
            <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
              <input
                type="checkbox"
                checked={settings.notifications}
                onChange={(e) => handleSettingChange('notifications', e.target.checked)}
              />
              🔔 Habilitar notificações
            </label>

            <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
              <input
                type="checkbox"
                checked={settings.autoSave}
                onChange={(e) => handleSettingChange('autoSave', e.target.checked)}
              />
              💾 Salvamento automático
            </label>

            <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
              <input
                type="checkbox"
                checked={settings.enableTelemetry}
                onChange={(e) => handleSettingChange('enableTelemetry', e.target.checked)}
              />
              📊 Enviar telemetria anônima
            </label>

            <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
              <input
                type="checkbox"
                checked={settings.debugMode}
                onChange={(e) => handleSettingChange('debugMode', e.target.checked)}
              />
              🐛 Modo de depuração
            </label>
          </div>
        </div>

        {/* Performance */}
        <div className="form-group">
          <h4 style={{ margin: '0 0 1rem 0', color: '#374151' }}>⚡ Performance</h4>

          <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
            <div>
              <label className="form-label">Tamanho máximo de arquivo (MB):</label>
              <input
                type="number"
                value={settings.maxFileSize}
                onChange={(e) => handleSettingChange('maxFileSize', parseInt(e.target.value))}
                className="form-input"
                min="1"
                max="100"
              />
            </div>

            <div>
              <label className="form-label">Timeout da API (segundos):</label>
              <input
                type="number"
                value={settings.apiTimeout}
                onChange={(e) => handleSettingChange('apiTimeout', parseInt(e.target.value))}
                className="form-input"
                min="5"
                max="300"
              />
            </div>
          </div>
        </div>

        {/* Mensagem de Status */}
        {saveMessage && (
          <div style={{
            padding: '1rem',
            background: saveMessage.includes('✅') ? '#f0fdf4' : '#fef2f2',
            border: `1px solid ${saveMessage.includes('✅') ? '#bbf7d0' : '#fecaca'}`,
            borderRadius: '8px',
            color: saveMessage.includes('✅') ? '#166534' : '#dc2626',
            marginBottom: '1rem'
          }}>
            {saveMessage}
          </div>
        )}

        {/* Ações */}
        <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap' }}>
          <button
            onClick={saveSettings}
            disabled={isSaving}
            className="btn btn-primary"
          >
            {isSaving ? (
              <div className="loading">
                <div className="spinner"></div>
                Salvando...
              </div>
            ) : (
              '💾 Salvar Configurações'
            )}
          </button>

          <button
            onClick={resetSettings}
            className="btn btn-secondary"
          >
            🔄 Restaurar Padrões
          </button>

          <button
            onClick={exportSettings}
            className="btn btn-secondary"
          >
            ⬇️ Exportar Configurações
          </button>

          <label className="btn btn-secondary" style={{ cursor: 'pointer' }}>
            ⬆️ Importar Configurações
            <input
              type="file"
              accept=".json"
              onChange={importSettings}
              style={{ display: 'none' }}
            />
          </label>
        </div>
      </div>

      {/* Informações do Sistema */}
      <div className="card">
        <div className="card-header">
          <h3 className="card-title">ℹ️ Informações do Sistema</h3>
          <p className="card-description">Detalhes técnicos e versões dos componentes</p>
        </div>

        <div className="grid grid-2">
          <div>
            <h4 style={{ margin: '0 0 1rem 0', color: '#374151' }}>🔧 Backend</h4>
            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem', fontSize: '0.9rem' }}>
              <div><strong>.NET Core:</strong> 8.0</div>
              <div><strong>Semantic Kernel:</strong> 1.6.1</div>
              <div><strong>Entity Framework:</strong> 8.0.8</div>
              <div><strong>PostgreSQL:</strong> 8.0.8</div>
              <div><strong>Redis:</strong> 2.8.16</div>
            </div>
          </div>

          <div>
            <h4 style={{ margin: '0 0 1rem 0', color: '#374151' }}>🟢 Frontend</h4>
            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem', fontSize: '0.9rem' }}>
              <div><strong>React:</strong> 18.2.0</div>
              <div><strong>Vite:</strong> 4.5.14</div>
              <div><strong>Node.js:</strong> Backend 18.x</div>
              <div><strong>LlamaIndex:</strong> Latest</div>
              <div><strong>OpenAI API:</strong> 4.x</div>
            </div>
          </div>
        </div>

        <div style={{ marginTop: '1.5rem' }}>
          <h4 style={{ margin: '0 0 1rem 0', color: '#374151' }}>📊 Recursos do Sistema</h4>
          <div className="grid grid-4">
            <div className="stats-card">
              <div className="stats-icon">💾</div>
              <div className="stats-number">4.2GB</div>
              <div className="stats-label">Memória Usada</div>
            </div>

            <div className="stats-card">
              <div className="stats-icon">⚡</div>
              <div className="stats-number">67%</div>
              <div className="stats-label">CPU</div>
            </div>

            <div className="stats-card">
              <div className="stats-icon">💽</div>
              <div className="stats-number">23GB</div>
              <div className="stats-label">Disco Usado</div>
            </div>

            <div className="stats-card">
              <div className="stats-icon">🌐</div>
              <div className="stats-number">99.9%</div>
              <div className="stats-label">Uptime</div>
            </div>
          </div>
        </div>
      </div>

      {/* Logs do Sistema */}
      <div className="card">
        <div className="card-header">
          <h3 className="card-title">📋 Logs do Sistema</h3>
          <p className="card-description">Últimas atividades e eventos registrados</p>
        </div>

        <div style={{ maxHeight: '300px', overflowY: 'auto', fontFamily: 'monospace', fontSize: '0.8rem' }}>
          <div style={{ padding: '0.5rem 0', borderBottom: '1px solid #e5e7eb' }}>
            <span style={{ color: '#10b981' }}>[INFO]</span> 2025-01-27 14:32:15 - Sistema iniciado com sucesso
          </div>
          <div style={{ padding: '0.5rem 0', borderBottom: '1px solid #e5e7eb' }}>
            <span style={{ color: '#10b981' }}>[INFO]</span> 2025-01-27 14:32:16 - Conexão com banco estabelecida
          </div>
          <div style={{ padding: '0.5rem 0', borderBottom: '1px solid #e5e7eb' }}>
            <span style={{ color: '#f59e0b' }}>[WARN]</span> 2025-01-27 14:32:17 - Cache Redis com latência elevada
          </div>
          <div style={{ padding: '0.5rem 0', borderBottom: '1px solid #e5e7eb' }}>
            <span style={{ color: '#10b981' }}>[INFO]</span> 2025-01-27 14:32:18 - Serviços de IA inicializados
          </div>
          <div style={{ padding: '0.5rem 0', borderBottom: '1px solid #e5e7eb' }}>
            <span style={{ color: '#10b981' }}>[INFO]</span> 2025-01-27 14:32:19 - Frontend conectado ao backend
          </div>
          <div style={{ padding: '0.5rem 0', borderBottom: '1px solid #e5e7eb' }}>
            <span style={{ color: '#3b82f6' }}>[DEBUG]</span> 2025-01-27 14:32:20 - Usuário logado: admin
          </div>
          <div style={{ padding: '0.5rem 0' }}>
            <span style={{ color: '#10b981' }}>[INFO]</span> 2025-01-27 14:32:21 - Sistema pronto para uso
          </div>
        </div>

        <div style={{ marginTop: '1rem', display: 'flex', gap: '0.5rem' }}>
          <button className="btn btn-secondary" style={{ fontSize: '0.9rem' }}>
            📄 Baixar Logs Completos
          </button>
          <button className="btn btn-secondary" style={{ fontSize: '0.9rem' }}>
            🗑️ Limpar Logs
          </button>
        </div>
      </div>
    </div>
  );
}
