import React, { useState, useRef, useEffect } from 'react';
import { sendChat } from '../api/chat';

export default function ChatSection() {
  const [messages, setMessages] = useState([
    {
      id: 1,
      type: 'bot',
      content: 'Olá! Sou seu assistente inteligente. Como posso ajudar você hoje?',
      timestamp: new Date()
    }
  ]);
  const [inputMessage, setInputMessage] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [selectedModel, setSelectedModel] = useState('llamaindex');
  const messagesEndRef = useRef(null);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  const handleSendMessage = async (e) => {
    e.preventDefault();

    if (!inputMessage.trim() || isLoading) return;

    const userMessage = {
      id: Date.now(),
      type: 'user',
      content: inputMessage,
      timestamp: new Date()
    };

    setMessages(prev => [...prev, userMessage]);
    setInputMessage('');
    setIsLoading(true);

    try {
      const response = await sendChat(inputMessage, 'user-' + Date.now());

      const botMessage = {
        id: Date.now() + 1,
        type: 'bot',
        content: response.message || response.content || 'Desculpe, não consegui processar sua mensagem.',
        timestamp: new Date()
      };

      setMessages(prev => [...prev, botMessage]);
    } catch (error) {
      console.error('Erro ao enviar mensagem:', error);

      const errorMessage = {
        id: Date.now() + 1,
        type: 'bot',
        content: 'Desculpe, ocorreu um erro ao processar sua mensagem. Tente novamente.',
        timestamp: new Date()
      };

      setMessages(prev => [...prev, errorMessage]);
    } finally {
      setIsLoading(false);
    }
  };

  const clearChat = () => {
    setMessages([
      {
        id: 1,
        type: 'bot',
        content: 'Olá! Sou seu assistente inteligente. Como posso ajudar você hoje?',
        timestamp: new Date()
      }
    ]);
  };

  const formatTime = (date) => {
    return date.toLocaleTimeString('pt-BR', {
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  return (
    <div className="grid">
      <div className="card" style={{ height: 'calc(100vh - 200px)', display: 'flex', flexDirection: 'column' }}>
        <div className="card-header">
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <div>
              <h3 className="card-title">💬 Chat Inteligente</h3>
              <p className="card-description">Converse com LlamaIndex AI</p>
            </div>

            <div style={{ display: 'flex', gap: '1rem', alignItems: 'center' }}>
              <div style={{
                padding: '0.5rem 1rem',
                borderRadius: '8px',
                background: '#f3f4f6',
                color: '#374151',
                fontSize: '0.9rem',
                fontWeight: '500'
              }}>
                🔗 LlamaIndex
              </div>

              <button
                onClick={clearChat}
                className="btn btn-secondary"
                style={{ fontSize: '0.9rem' }}
              >
                🗑️ Limpar Chat
              </button>
            </div>
          </div>
        </div>

        {/* Área de mensagens */}
        <div style={{
          flex: 1,
          overflowY: 'auto',
          padding: '1rem',
          background: '#f8fafc',
          borderRadius: '8px',
          margin: '0 1.5rem 1rem',
          display: 'flex',
          flexDirection: 'column',
          gap: '1rem'
        }}>
          {messages.map((message) => (
            <div
              key={message.id}
              style={{
                display: 'flex',
                justifyContent: message.type === 'user' ? 'flex-end' : 'flex-start',
                marginBottom: '0.5rem'
              }}
            >
              <div
                style={{
                  maxWidth: '70%',
                  padding: '0.75rem 1rem',
                  borderRadius: '18px',
                  background: message.type === 'user'
                    ? 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)'
                    : 'white',
                  color: message.type === 'user' ? 'white' : '#374151',
                  boxShadow: '0 2px 4px rgba(0, 0, 0, 0.1)',
                  border: message.type === 'bot' ? '1px solid #e5e7eb' : 'none'
                }}
              >
                <div style={{ marginBottom: '0.25rem' }}>
                  {message.content}
                </div>
                <div style={{
                  fontSize: '0.75rem',
                  opacity: 0.7,
                  textAlign: message.type === 'user' ? 'right' : 'left'
                }}>
                  {formatTime(message.timestamp)}
                </div>
              </div>
            </div>
          ))}

          {isLoading && (
            <div style={{ display: 'flex', justifyContent: 'flex-start' }}>
              <div style={{
                padding: '0.75rem 1rem',
                borderRadius: '18px',
                background: 'white',
                border: '1px solid #e5e7eb',
                boxShadow: '0 2px 4px rgba(0, 0, 0, 0.1)'
              }}>
                <div className="loading">
                  <div className="spinner"></div>
                  Digitando...
                </div>
              </div>
            </div>
          )}

          <div ref={messagesEndRef} />
        </div>

        {/* Formulário de entrada */}
        <div style={{
          padding: '1rem 1.5rem',
          borderTop: '1px solid #e5e7eb',
          background: 'white'
        }}>
          <form onSubmit={handleSendMessage} style={{ display: 'flex', gap: '1rem' }}>
            <input
              type="text"
              value={inputMessage}
              onChange={(e) => setInputMessage(e.target.value)}
              placeholder="Digite sua mensagem..."
              disabled={isLoading}
              style={{
                flex: 1,
                padding: '0.75rem 1rem',
                border: '1px solid #d1d5db',
                borderRadius: '25px',
                fontSize: '1rem',
                outline: 'none'
              }}
              onFocus={(e) => e.target.style.borderColor = '#667eea'}
              onBlur={(e) => e.target.style.borderColor = '#d1d5db'}
            />

            <button
              type="submit"
              disabled={isLoading || !inputMessage.trim()}
              className="btn btn-primary"
              style={{
                borderRadius: '25px',
                padding: '0.75rem 1.5rem',
                opacity: (isLoading || !inputMessage.trim()) ? 0.6 : 1
              }}
            >
              {isLoading ? '⏳' : '📤'}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}
