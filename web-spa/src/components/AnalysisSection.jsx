import React, { useState } from 'react';
import { runPrompt } from '../api/ia';

export default function AnalysisSection() {
  const [analysisType, setAnalysisType] = useState('document');
  const [inputText, setInputText] = useState('');
  const [file, setFile] = useState(null);
  const [isLoading, setIsLoading] = useState(false);
  const [results, setResults] = useState(null);
  const [error, setError] = useState(null);

  const analysisTypes = [
    { value: 'document', label: '📄 Análise de Documento', description: 'Analisa conteúdo de texto ou documentos' },
    { value: 'code', label: '💻 Análise de Código', description: 'Analisa e otimiza código fonte' },
    { value: 'sentiment', label: '😊 Análise de Sentimento', description: 'Identifica emoções e sentimentos no texto' },
    { value: 'summary', label: '📋 Resumo Inteligente', description: 'Gera resumos automáticos do conteúdo' },
    { value: 'keywords', label: '🏷️ Extração de Palavras-chave', description: 'Identifica termos importantes no texto' }
  ];

  const handleFileUpload = (e) => {
    const selectedFile = e.target.files[0];
    setFile(selectedFile);

    if (selectedFile) {
      const reader = new FileReader();
      reader.onload = (event) => {
        setInputText(event.target.result);
      };
      reader.readAsText(selectedFile);
    }
  };

  const handleAnalyze = async () => {
    if (!inputText.trim()) {
      setError('Por favor, insira um texto ou selecione um arquivo para análise.');
      return;
    }

    setIsLoading(true);
    setError(null);
    setResults(null);

    try {
      const response = await runPrompt({
        text: inputText,
        type: analysisType
      });
      setResults(response);
    } catch (err) {
      console.error('Erro na análise:', err);
      setError('Erro ao realizar análise. Tente novamente.');
    } finally {
      setIsLoading(false);
    }
  };

  const renderResults = () => {
    if (!results) return null;

    return (
      <div className="card">
        <div className="card-header">
          <h3 className="card-title">📊 Resultados da Análise</h3>
          <p className="card-description">Análise realizada com Semantic Kernel</p>
        </div>

        <div style={{ background: '#f8fafc', padding: '1rem', borderRadius: '8px', fontFamily: 'monospace' }}>
          <pre style={{ margin: 0, whiteSpace: 'pre-wrap', color: '#374151' }}>
            {typeof results === 'string' ? results : JSON.stringify(results, null, 2)}
          </pre>
        </div>
      </div>
    );
  };

  return (
    <div className="grid">
      <div className="card">
        <div className="card-header">
          <h3 className="card-title">🧠 Análise Inteligente com Semantic Kernel</h3>
          <p className="card-description">Utilize IA avançada para analisar textos, códigos e documentos</p>
        </div>

        {/* Tipo de Análise */}
        <div className="form-group">
          <label className="form-label">Tipo de Análise:</label>
          <div className="grid grid-2">
            {analysisTypes.map((type) => (
              <div
                key={type.value}
                onClick={() => setAnalysisType(type.value)}
                style={{
                  padding: '1rem',
                  border: `2px solid ${analysisType === type.value ? '#667eea' : '#e5e7eb'}`,
                  borderRadius: '8px',
                  cursor: 'pointer',
                  background: analysisType === type.value ? '#f0f4ff' : 'white',
                  transition: 'all 0.3s ease'
                }}
              >
                <div style={{ fontWeight: '600', marginBottom: '0.25rem' }}>
                  {type.label}
                </div>
                <div style={{ fontSize: '0.9rem', color: '#6b7280' }}>
                  {type.description}
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Upload de Arquivo */}
        <div className="form-group">
          <label className="form-label">📎 Upload de Arquivo (opcional):</label>
          <input
            type="file"
            accept=".txt,.md,.js,.py,.cs,.java,.html,.css,.json"
            onChange={handleFileUpload}
            className="form-input"
            style={{ padding: '0.5rem' }}
          />
          {file && (
            <div style={{ marginTop: '0.5rem', fontSize: '0.9rem', color: '#10b981' }}>
              ✅ Arquivo selecionado: {file.name}
            </div>
          )}
        </div>

        {/* Entrada de Texto */}
        <div className="form-group">
          <label className="form-label">📝 Texto para Análise:</label>
          <textarea
            value={inputText}
            onChange={(e) => setInputText(e.target.value)}
            placeholder="Cole seu texto aqui ou faça upload de um arquivo..."
            className="form-input form-textarea"
            disabled={isLoading}
          />
        </div>

        {/* Erro */}
        {error && (
          <div style={{
            padding: '1rem',
            background: '#fef2f2',
            border: '1px solid #fecaca',
            borderRadius: '8px',
            color: '#dc2626',
            marginBottom: '1rem'
          }}>
            ⚠️ {error}
          </div>
        )}

        {/* Botão de Análise */}
        <div style={{ display: 'flex', gap: '1rem' }}>
          <button
            onClick={handleAnalyze}
            disabled={isLoading || !inputText.trim()}
            className="btn btn-primary"
            style={{ flex: 1 }}
          >
            {isLoading ? (
              <div className="loading">
                <div className="spinner"></div>
                Analisando...
              </div>
            ) : (
              '🚀 Iniciar Análise'
            )}
          </button>

          <button
            onClick={() => {
              setInputText('');
              setFile(null);
              setResults(null);
              setError(null);
            }}
            className="btn btn-secondary"
          >
            🗑️ Limpar
          </button>
        </div>
      </div>

      {/* Exemplos de Uso */}
      <div className="card">
        <div className="card-header">
          <h3 className="card-title">💡 Exemplos de Uso</h3>
          <p className="card-description">Ideias para aproveitar ao máximo a análise inteligente</p>
        </div>

        <div className="grid grid-2">
          <div style={{ padding: '1rem', background: '#f8fafc', borderRadius: '8px' }}>
            <h4 style={{ margin: '0 0 0.5rem 0', color: '#374151' }}>📄 Análise de Documentos</h4>
            <p style={{ margin: 0, fontSize: '0.9rem', color: '#6b7280' }}>
              Faça upload de contratos, relatórios ou documentos para extrair insights,
              identificar cláusulas importantes ou detectar inconsistências.
            </p>
          </div>

          <div style={{ padding: '1rem', background: '#f8fafc', borderRadius: '8px' }}>
            <h4 style={{ margin: '0 0 0.5rem 0', color: '#374151' }}>💻 Revisão de Código</h4>
            <p style={{ margin: 0, fontSize: '0.9rem', color: '#6b7280' }}>
              Cole trechos de código para identificar bugs, sugerir melhorias
              ou otimizar algoritmos automaticamente.
            </p>
          </div>

          <div style={{ padding: '1rem', background: '#f8fafc', borderRadius: '8px' }}>
            <h4 style={{ margin: '0 0 0.5rem 0', color: '#374151' }}>😊 Análise de Sentimentos</h4>
            <p style={{ margin: 0, fontSize: '0.9rem', color: '#6b7280' }}>
              Analise comentários, reviews ou textos para entender a opinião
              geral e identificar pontos positivos/negativos.
            </p>
          </div>

          <div style={{ padding: '1rem', background: '#f8fafc', borderRadius: '8px' }}>
            <h4 style={{ margin: '0 0 0.5rem 0', color: '#374151' }}>📋 Resumos Automáticos</h4>
            <p style={{ margin: 0, fontSize: '0.9rem', color: '#6b7280' }}>
              Transforme textos longos em resumos concisos mantendo
              os pontos mais importantes e relevantes.
            </p>
          </div>
        </div>
      </div>

      {/* Resultados */}
      {renderResults()}
    </div>
  );
}
