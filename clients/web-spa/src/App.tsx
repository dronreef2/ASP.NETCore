import React, { useState } from 'react';
import { Routes, Route } from 'react-router-dom';
import { Sidebar } from './components/Sidebar';
import { ChatInterface } from './components/ChatInterface';
import { CodeAnalyzer } from './components/CodeAnalyzer';
import { AssessmentPanel } from './components/AssessmentPanel';
import { SettingsPanel } from './components/SettingsPanel';
import { Header } from './components/Header';

function App() {
  const [activeTab, setActiveTab] = useState('chat');
  const [sidebarOpen, setSidebarOpen] = useState(true);

  return (
    <div className="flex h-screen bg-gray-100 dark:bg-gray-900">
      {/* Sidebar */}
      <Sidebar 
        isOpen={sidebarOpen}
        activeTab={activeTab}
        onTabChange={setActiveTab}
        onToggle={() => setSidebarOpen(!sidebarOpen)}
      />

      {/* Main Content */}
      <div className="flex-1 flex flex-col">
        {/* Header */}
        <Header onMenuToggle={() => setSidebarOpen(!sidebarOpen)} />

        {/* Content Area */}
        <main className="flex-1 overflow-hidden">
          <Routes>
            <Route path="/" element={
              <div className="h-full">
                {activeTab === 'chat' && <ChatInterface />}
                {activeTab === 'analyzer' && <CodeAnalyzer />}
                {activeTab === 'assessment' && <AssessmentPanel />}
                {activeTab === 'settings' && <SettingsPanel />}
              </div>
            } />
          </Routes>
        </main>
      </div>
    </div>
  );
}

export default App;
