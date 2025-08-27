import React, { useState } from 'react';
import { explainWithLlama } from '../api/llama';

export default function SearchSection() {
  const [searchQuery, setSearchQuery] = useState('');
  const [searchType, setSearchType] = useState('search');
  const [isLoading, setIsLoading] = useState(false);
  const [results, setResults] = useState(null);
  const [error, setError] = useState(null);
  const [searchHistory, setSearchHistory] = useState([]);

  const searchTypes = [
    { value: 'search', label: '🔍 Busca Semântica', description: 'Busca por similaridade semântica no conteúdo' },
    { value: 'explain', label: '📖 Explicação Inteligente', description: 'Explica conceitos e gera exemplos práticos' }
  ];

  const handleSearch = async () => {
    if (!searchQuery.trim()) {
      setError('Por favor, insira uma consulta para busca.');
      return;
    }

    setIsLoading(true);
    setError(null);
    setResults(null);

    try {
      let response;

      if (searchType === 'search') {
        // Para busca, também usamos explain por enquanto
        response = await explainWithLlama(searchQuery);
      } else {
        response = await explainWithLlama(searchQuery);
      }

      setResults(response);

      // Adicionar ao histórico
      setSearchHistory(prev => [{
        query: searchQuery,
        type: searchType,
        timestamp: new Date(),
        results: response
      }, ...prev.slice(0, 9)]); // Manter apenas as últimas 10 buscas

    } catch (err) {
      console.error('Erro na busca:', err);
      setError('Erro ao realizar busca. Verifique se o backend está rodando.');
    } finally {
      setIsLoading(false);
    }
  };

  const handleQuickSearch = (query) => {
    setSearchQuery(query);
    setSearchType('search');
  };

  const renderResults = () => {
    if (!results) return null;

    return (
      <div className="card">
        <div className="card-header">
          <h3 className="card-title">
            {searchType === 'search' ? '🔍 Resultados da Busca' : '📖 Explicação'}
          </h3>
          <p className="card-description">
            Resultados obtidos via LlamaIndex - {searchType === 'search' ? 'Busca Semântica' : 'Explicação Inteligente'}
          </p>
        </div>

        <div style={{ background: '#f8fafc', padding: '1.5rem', borderRadius: '8px' }}>
          {searchType === 'search' ? (
            <div>
              {results.documents && results.documents.length > 0 ? (
                <div>
                  <h4 style={{ margin: '0 0 1rem 0', color: '#374151' }}>
                    📄 Documentos Encontrados ({results.documents.length})
                  </h4>
                  {results.documents.map((doc, index) => (
                    <div key={index} style={{
                      background: 'white',
                      padding: '1rem',
                      borderRadius: '8px',
                      marginBottom: '1rem',
                      border: '1px solid #e5e7eb'
                    }}>
                      <div style={{ fontWeight: '600', marginBottom: '0.5rem', color: '#374151' }}>
                        Documento {index + 1}
                      </div>
                      <div style={{ color: '#6b7280', lineHeight: '1.6' }}>
                        {doc.content || doc.text || JSON.stringify(doc)}
                      </div>
                      {doc.score && (
                        <div style={{ marginTop: '0.5rem', fontSize: '0.8rem', color: '#10b981' }}>
                          📊 Relevância: {(doc.score * 100).toFixed(1)}%
                        </div>
                      )}
                    </div>
                  ))}
                </div>
              ) : (
                <div style={{ textAlign: 'center', color: '#6b7280' }}>
                  Nenhum documento encontrado para a consulta.
                </div>
              )}
            </div>
          ) : (
            <div>
              <h4 style={{ margin: '0 0 1rem 0', color: '#374151' }}>
                📖 Explicação do Conceito
              </h4>
              <div style={{
                background: 'white',
                padding: '1.5rem',
                borderRadius: '8px',
                border: '1px solid #e5e7eb',
                lineHeight: '1.7',
                color: '#374151'
              }}>
                {typeof results === 'string' ? results : JSON.stringify(results, null, 2)}
              </div>
            </div>
          )}
        </div>
      </div>
    );
  };

  const quickSearches = [
    'Como funciona machine learning?',
    'Explicar algoritmos de busca',
    'Conceitos de inteligência artificial',
    'O que é processamento de linguagem natural?',
    'Diferenças entre SQL e NoSQL'
  ];

  return (
    <div className="grid">
      <div className="card">
        <div className="card-header">
          <h3 className="card-title">🔍 Busca Semântica com LlamaIndex</h3>
          <p className="card-description">Busque informações de forma inteligente usando IA e RAG</p>
        </div>

        {/* Tipo de Busca */}
        <div className="form-group">
          <label className="form-label">Tipo de Consulta:</label>
          <div className="grid grid-2">
            {searchTypes.map((type) => (
              <div
                key={type.value}
                onClick={() => setSearchType(type.value)}
                style={{
                  padding: '1rem',
                  border: `2px solid ${searchType === type.value ? '#667eea' : '#e5e7eb'}`,
                  borderRadius: '8px',
                  cursor: 'pointer',
                  background: searchType === type.value ? '#f0f4ff' : 'white',
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

        {/* Consulta */}
        <div className="form-group">
          <label className="form-label">
            {searchType === 'search' ? '🔍 Sua Consulta:' : '📖 Conceito para Explicar:'}
          </label>
          <input
            type="text"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            placeholder={
              searchType === 'search'
                ? 'Ex: "Como funciona machine learning?"'
                : 'Ex: "algoritmos de ordenação"'
            }
            className="form-input"
            disabled={isLoading}
            onKeyPress={(e) => e.key === 'Enter' && handleSearch()}
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

        {/* Botão de Busca */}
        <button
          onClick={handleSearch}
          disabled={isLoading || !searchQuery.trim()}
          className="btn btn-primary"
          style={{ width: '100%' }}
        >
          {isLoading ? (
            <div className="loading">
              <div className="spinner"></div>
              {searchType === 'search' ? 'Buscando...' : 'Explicando...'}
            </div>
          ) : (
            `🚀 ${searchType === 'search' ? 'Realizar Busca' : 'Gerar Explicação'}`
          )}
        </button>
      </div>

      {/* Buscas Rápidas */}
      <div className="card">
        <div className="card-header">
          <h3 className="card-title">⚡ Buscas Rápidas</h3>
          <p className="card-description">Exemplos de consultas para testar o sistema</p>
        </div>

        <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.5rem' }}>
          {quickSearches.map((query, index) => (
            <button
              key={index}
              onClick={() => handleQuickSearch(query)}
              className="btn btn-secondary"
              style={{ fontSize: '0.9rem', padding: '0.5rem 1rem' }}
            >
              {query}
            </button>
          ))}
        </div>
      </div>

      {/* Histórico de Buscas */}
      {searchHistory.length > 0 && (
        <div className="card">
          <div className="card-header">
            <h3 className="card-title">📚 Histórico de Buscas</h3>
            <p className="card-description">Suas últimas consultas realizadas</p>
          </div>

          <div style={{ maxHeight: '300px', overflowY: 'auto' }}>
            {searchHistory.map((item, index) => (
              <div key={index} style={{
                padding: '0.75rem',
                borderBottom: index < searchHistory.length - 1 ? '1px solid #e5e7eb' : 'none',
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center'
              }}>
                <div style={{ flex: 1 }}>
                  <div style={{ fontWeight: '500', marginBottom: '0.25rem' }}>
                    {item.query}
                  </div>
                  <div style={{ fontSize: '0.8rem', color: '#6b7280' }}>
                    {item.type === 'search' ? '🔍 Busca' : '📖 Explicação'} •
                    {item.timestamp.toLocaleTimeString('pt-BR')}
                  </div>
                </div>
                <button
                  onClick={() => {
                    setSearchQuery(item.query);
                    setSearchType(item.type);
                    setResults(item.results);
                  }}
                  className="btn btn-secondary"
                  style={{ fontSize: '0.8rem', padding: '0.25rem 0.75rem' }}
                >
                  🔄
                </button>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Resultados */}
      {renderResults()}
    </div>
  );
}
