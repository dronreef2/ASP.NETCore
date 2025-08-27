import React, { useState } from 'react';
import Dashboard from './components/Dashboard';
import ChatSection from './components/ChatSection';
import ChatSignalRSection from './components/ChatSignalRSection';
import AnalysisSection from './components/AnalysisSection';
import SearchSection from './components/SearchSection';
import ReportsSection from './components/ReportsSection';
import SettingsSection from './components/SettingsSection';
import DeploymentSection from './components/DeploymentSection';
import GitHubSection from './components/GitHubSection';
import MonitoringSection from './components/MonitoringSection';
import SecuritySection from './components/SecuritySection';
import DatabaseSection from './components/DatabaseSection';
import './App.css';

export default function App() {
  const [activeSection, setActiveSection] = useState('dashboard');

  const renderSection = () => {
    switch (activeSection) {
      case 'dashboard':
        return <Dashboard />;
      case 'chat':
        return <ChatSection />;
      case 'chat-signalr':
        return <ChatSignalRSection />;
      case 'analysis':
        return <AnalysisSection />;
      case 'search':
        return <SearchSection />;
      case 'reports':
        return <ReportsSection />;
      case 'deployments':
        return <DeploymentSection />;
      case 'github':
        return <GitHubSection />;
      case 'monitoring':
        return <MonitoringSection />;
      case 'security':
        return <SecuritySection />;
      case 'database':
        return <DatabaseSection />;
      case 'settings':
        return <SettingsSection />;
      default:
        return <Dashboard />;
    }
  };

  return (
    <div className="app">
      <nav className="sidebar">
        <div className="sidebar-header">
          <h2>🚀 Tutor Copiloto</h2>
          <p>Centro de Controle IA</p>
        </div>

        <div className="sidebar-menu">
          <button
            className={`menu-item ${activeSection === 'dashboard' ? 'active' : ''}`}
            onClick={() => setActiveSection('dashboard')}
          >
            📊 Dashboard
          </button>

          <div className="menu-group">
            <div className="menu-group-title">🤖 IA & CONVERSAÇÃO</div>
            <button
              className={`menu-item ${activeSection === 'chat' ? 'active' : ''}`}
              onClick={() => setActiveSection('chat')}
            >
              💬 Chat
            </button>

            <button
              className={`menu-item ${activeSection === 'chat-signalr' ? 'active' : ''}`}
              onClick={() => setActiveSection('chat-signalr')}
            >
              🔴 Chat SignalR
            </button>

            <button
              className={`menu-item ${activeSection === 'analysis' ? 'active' : ''}`}
              onClick={() => setActiveSection('analysis')}
            >
              🧠 Análise Inteligente
            </button>

            <button
              className={`menu-item ${activeSection === 'search' ? 'active' : ''}`}
              onClick={() => setActiveSection('search')}
            >
              🔍 Busca Semântica
            </button>
          </div>

          <div className="menu-group">
            <div className="menu-group-title">🚀 DEPLOYMENT & CI/CD</div>
            <button
              className={`menu-item ${activeSection === 'deployments' ? 'active' : ''}`}
              onClick={() => setActiveSection('deployments')}
            >
              🚀 Deployments
            </button>

            <button
              className={`menu-item ${activeSection === 'github' ? 'active' : ''}`}
              onClick={() => setActiveSection('github')}
            >
              🐙 GitHub
            </button>

            <button
              className={`menu-item ${activeSection === 'monitoring' ? 'active' : ''}`}
              onClick={() => setActiveSection('monitoring')}
            >
              📊 Monitoramento
            </button>
          </div>

          <div className="menu-group">
            <div className="menu-group-title">🔒 SEGURANÇA & SISTEMA</div>
            <button
              className={`menu-item ${activeSection === 'security' ? 'active' : ''}`}
              onClick={() => setActiveSection('security')}
            >
              🔒 Segurança
            </button>

            <button
              className={`menu-item ${activeSection === 'database' ? 'active' : ''}`}
              onClick={() => setActiveSection('database')}
            >
              🗄️ Banco de Dados
            </button>

            <button
              className={`menu-item ${activeSection === 'reports' ? 'active' : ''}`}
              onClick={() => setActiveSection('reports')}
            >
              📋 Relatórios
            </button>
          </div>

          <button
            className={`menu-item ${activeSection === 'settings' ? 'active' : ''}`}
            onClick={() => setActiveSection('settings')}
          >
            ⚙️ Configurações
          </button>
        </div>

        <div className="sidebar-footer">
          <div className="status-indicator">
            <span className="status-dot"></span>
            <span>Sistema Online</span>
          </div>
        </div>
      </nav>

      <main className="main-content">
        <header className="main-header">
          <h1>
            {activeSection === 'dashboard' && 'Dashboard Principal'}
            {activeSection === 'chat' && 'Chat Inteligente'}
            {activeSection === 'chat-signalr' && 'Chat SignalR em Tempo Real'}
            {activeSection === 'analysis' && 'Análise com Semantic Kernel'}
            {activeSection === 'search' && 'Busca Semântica LlamaIndex'}
            {activeSection === 'reports' && 'Sistema de Relatórios'}
            {activeSection === 'deployments' && 'Gerenciamento de Deployments'}
            {activeSection === 'github' && 'Integração GitHub'}
            {activeSection === 'monitoring' && 'Monitoramento & Observabilidade'}
            {activeSection === 'security' && 'Análise de Segurança'}
            {activeSection === 'database' && 'Banco de Dados & Cache'}
            {activeSection === 'settings' && 'Configurações do Sistema'}
          </h1>
        </header>

        <div className="content-area">
          {renderSection()}
        </div>
      </main>
    </div>
  );
}
