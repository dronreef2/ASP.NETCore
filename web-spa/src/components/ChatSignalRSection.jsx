import React, { useState, useRef, useEffect } from 'react';
import signalRService from '../api/signalr';

export default function ChatSignalRSection() {
  const [messages, setMessages] = useState([
    {
      id: 1,
      type: 'system',
      content: 'Bem-vindo ao chat SignalR! Conecte-se para começar.',
      timestamp: new Date()
    }
  ]);
  const [inputMessage, setInputMessage] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [isConnected, setIsConnected] = useState(false);
  const [connectionStatus, setConnectionStatus] = useState('disconnected');
  const [userToken, setUserToken] = useState('');
  const [userName, setUserName] = useState('');
  const messagesEndRef = useRef(null);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  // Configurar event handlers do SignalR
  useEffect(() => {
    const handleReceiveMessage = (message) => {
      const newMessage = {
        id: Date.now(),
        type: message.senderId === 'tutor-ai' ? 'tutor' : 'user',
        content: message.content,
        sender: message.senderName,
        timestamp: new Date(message.timestamp)
      };
      setMessages(prev => [...prev, newMessage]);
    };

    const handleMessageSent = (data) => {
      console.log('Mensagem enviada:', data);
    };

    const handleError = (error) => {
      const errorMessage = {
        id: Date.now(),
        type: 'error',
        content: `Erro: ${error}`,
        timestamp: new Date()
      };
      setMessages(prev => [...prev, errorMessage]);
    };

    const handleTutorTyping = (data) => {
      const typingMessage = {
        id: Date.now(),
        type: 'typing',
        content: 'Tutor está digitando...',
        timestamp: new Date()
      };
      setMessages(prev => [...prev, typingMessage]);

      // Remover mensagem de digitação após 3 segundos
      setTimeout(() => {
        setMessages(prev => prev.filter(msg => msg.type !== 'typing'));
      }, 3000);
    };

    const handleUserConnected = (data) => {
      const systemMessage = {
        id: Date.now(),
        type: 'system',
        content: `${data.message}`,
        timestamp: new Date()
      };
      setMessages(prev => [...prev, systemMessage]);
    };

    const handleUserDisconnected = (data) => {
      const systemMessage = {
        id: Date.now(),
        type: 'system',
        content: `${data.message}`,
        timestamp: new Date()
      };
      setMessages(prev => [...prev, systemMessage]);
    };

    // Registrar event handlers
    signalRService.on('receiveMessage', handleReceiveMessage);
    signalRService.on('messageSent', handleMessageSent);
    signalRService.on('error', handleError);
    signalRService.on('tutorTyping', handleTutorTyping);
    signalRService.on('userConnected', handleUserConnected);
    signalRService.on('userDisconnected', handleUserDisconnected);

    // Monitorar status da conexão
    const statusInterval = setInterval(() => {
      setConnectionStatus(signalRService.getConnectionStatus());
      setIsConnected(signalRService.isConnected);
    }, 1000);

    return () => {
      // Limpar event handlers
      signalRService.off('receiveMessage', handleReceiveMessage);
      signalRService.off('messageSent', handleMessageSent);
      signalRService.off('error', handleError);
      signalRService.off('tutorTyping', handleTutorTyping);
      signalRService.off('userConnected', handleUserConnected);
      signalRService.off('userDisconnected', handleUserDisconnected);
      clearInterval(statusInterval);
    };
  }, []);

  const connectToSignalR = async () => {
    if (!userToken.trim()) {
      alert('Por favor, obtenha um token primeiro!');
      return;
    }

    try {
      setIsLoading(true);
      const success = await signalRService.initialize(userToken);
      if (success) {
        const connectMessage = {
          id: Date.now(),
          type: 'system',
          content: '✅ Conectado ao SignalR com sucesso!',
          timestamp: new Date()
        };
        setMessages(prev => [...prev, connectMessage]);
      } else {
        throw new Error('Falha ao conectar');
      }
    } catch (error) {
      console.error('Erro ao conectar:', error);
      const errorMessage = {
        id: Date.now(),
        type: 'error',
        content: `❌ Erro ao conectar: ${error.message}`,
        timestamp: new Date()
      };
      setMessages(prev => [...prev, errorMessage]);
    } finally {
      setIsLoading(false);
    }
  };

  const disconnectFromSignalR = async () => {
    try {
      await signalRService.disconnect();
      const disconnectMessage = {
        id: Date.now(),
        type: 'system',
        content: '🔌 Desconectado do SignalR',
        timestamp: new Date()
      };
      setMessages(prev => [...prev, disconnectMessage]);
    } catch (error) {
      console.error('Erro ao desconectar:', error);
    }
  };

  const getAnonymousToken = async () => {
    try {
      const displayName = userName.trim() || `Visitante_${Date.now()}`;
      const response = await fetch('http://localhost:5000/api/auth/anonymous', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          deviceId: `device_${Date.now()}`,
          displayName: displayName
        })
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const data = await response.json();
      setUserToken(data.token);
      setUserName(data.displayName);

      const tokenMessage = {
        id: Date.now(),
        type: 'system',
        content: `🔑 Token obtido para: ${data.displayName}`,
        timestamp: new Date()
      };
      setMessages(prev => [...prev, tokenMessage]);
    } catch (error) {
      console.error('Erro ao obter token:', error);
      const errorMessage = {
        id: Date.now(),
        type: 'error',
        content: `❌ Erro ao obter token: ${error.message}`,
        timestamp: new Date()
      };
      setMessages(prev => [...prev, errorMessage]);
    }
  };

  const handleSendMessage = async (e) => {
    e.preventDefault();

    if (!inputMessage.trim() || isLoading || !isConnected) return;

    const messageData = {
      content: inputMessage,
      type: 0, // ChatMessageType.GeneralMessage
      sessionId: `session_${Date.now()}`,
      metadata: {}
    };

    try {
      await signalRService.sendMessage(messageData);

      const userMessage = {
        id: Date.now(),
        type: 'user',
        content: inputMessage,
        sender: userName,
        timestamp: new Date()
      };

      setMessages(prev => [...prev, userMessage]);
      setInputMessage('');
    } catch (error) {
      console.error('Erro ao enviar mensagem:', error);
      const errorMessage = {
        id: Date.now(),
        type: 'error',
        content: `❌ Erro ao enviar mensagem: ${error.message}`,
        timestamp: new Date()
      };
      setMessages(prev => [...prev, errorMessage]);
    }
  };

  const clearChat = () => {
    setMessages([
      {
        id: 1,
        type: 'system',
        content: 'Chat limpo! Continue conversando.',
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

  const getStatusColor = () => {
    switch (connectionStatus) {
      case 'connected': return '#10b981';
      case 'connecting': return '#f59e0b';
      case 'reconnecting': return '#f59e0b';
      default: return '#ef4444';
    }
  };

  const getStatusText = () => {
    switch (connectionStatus) {
      case 'connected': return '🟢 Conectado';
      case 'connecting': return '🟡 Conectando...';
      case 'reconnecting': return '🟡 Reconectando...';
      default: return '🔴 Desconectado';
    }
  };

  return (
    <div className="grid">
      <div className="card" style={{ height: 'calc(100vh - 200px)', display: 'flex', flexDirection: 'column' }}>
        <div className="card-header">
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <div>
              <h3 className="card-title">🚀 Chat SignalR</h3>
              <p className="card-description">Chat em tempo real com SignalR</p>
            </div>

            <div style={{ display: 'flex', gap: '1rem', alignItems: 'center' }}>
              <div style={{
                display: 'flex',
                alignItems: 'center',
                gap: '0.5rem',
                padding: '0.5rem 1rem',
                borderRadius: '20px',
                background: '#f3f4f6',
                fontSize: '0.9rem'
              }}>
                <div style={{
                  width: '8px',
                  height: '8px',
                  borderRadius: '50%',
                  background: getStatusColor()
                }}></div>
                {getStatusText()}
              </div>

              <button
                onClick={clearChat}
                className="btn btn-secondary"
                style={{ fontSize: '0.9rem' }}
              >
                🗑️ Limpar
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
                    : message.type === 'error'
                    ? '#fee2e2'
                    : message.type === 'system'
                    ? '#e0f2fe'
                    : message.type === 'tutor'
                    ? '#f0fdf4'
                    : 'white',
                  color: message.type === 'user'
                    ? 'white'
                    : message.type === 'error'
                    ? '#dc2626'
                    : message.type === 'system'
                    ? '#0369a1'
                    : message.type === 'tutor'
                    ? '#166534'
                    : '#374151',
                  boxShadow: '0 2px 4px rgba(0, 0, 0, 0.1)',
                  border: message.type === 'error'
                    ? '1px solid #fecaca'
                    : message.type === 'system'
                    ? '1px solid #bae6fd'
                    : message.type === 'tutor'
                    ? '1px solid #bbf7d0'
                    : '1px solid #e5e7eb'
                }}
              >
                {message.sender && (
                  <div style={{
                    fontSize: '0.75rem',
                    opacity: 0.7,
                    marginBottom: '0.25rem',
                    fontWeight: 'bold'
                  }}>
                    {message.sender}
                  </div>
                )}
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
                  Processando...
                </div>
              </div>
            </div>
          )}

          <div ref={messagesEndRef} />
        </div>

        {/* Área de controle de conexão */}
        <div style={{
          padding: '1rem 1.5rem',
          borderTop: '1px solid #e5e7eb',
          background: '#f9fafb'
        }}>
          <div style={{ display: 'flex', gap: '1rem', marginBottom: '1rem' }}>
            <input
              type="text"
              value={userName}
              onChange={(e) => setUserName(e.target.value)}
              placeholder="Seu nome (opcional)"
              style={{
                flex: 1,
                padding: '0.5rem 1rem',
                border: '1px solid #d1d5db',
                borderRadius: '8px',
                fontSize: '0.9rem'
              }}
            />

            <button
              onClick={getAnonymousToken}
              className="btn btn-secondary"
              style={{ fontSize: '0.9rem' }}
            >
              🔑 Obter Token
            </button>

            {!isConnected ? (
              <button
                onClick={connectToSignalR}
                disabled={!userToken || isLoading}
                className="btn btn-primary"
                style={{ fontSize: '0.9rem' }}
              >
                {isLoading ? '⏳' : '🔌 Conectar'}
              </button>
            ) : (
              <button
                onClick={disconnectFromSignalR}
                className="btn btn-danger"
                style={{ fontSize: '0.9rem' }}
              >
                🔌 Desconectar
              </button>
            )}
          </div>

          {userToken && (
            <div style={{
              padding: '0.5rem',
              background: '#f3f4f6',
              borderRadius: '8px',
              fontSize: '0.8rem',
              color: '#6b7280',
              marginBottom: '1rem',
              wordBreak: 'break-all'
            }}>
              <strong>Token:</strong> {userToken.substring(0, 50)}...
            </div>
          )}
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
              disabled={isLoading || !isConnected}
              style={{
                flex: 1,
                padding: '0.75rem 1rem',
                border: '1px solid #d1d5db',
                borderRadius: '25px',
                fontSize: '1rem',
                outline: 'none',
                opacity: (!isConnected) ? 0.5 : 1
              }}
              onFocus={(e) => e.target.style.borderColor = '#667eea'}
              onBlur={(e) => e.target.style.borderColor = '#d1d5db'}
            />

            <button
              type="submit"
              disabled={isLoading || !inputMessage.trim() || !isConnected}
              className="btn btn-primary"
              style={{
                borderRadius: '25px',
                padding: '0.75rem 1.5rem',
                opacity: (isLoading || !inputMessage.trim() || !isConnected) ? 0.6 : 1
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
