// SignalR Service para integração com o backend
class SignalRService {
  constructor() {
    this.connection = null;
    this.isConnected = false;
    this.eventHandlers = {};
  }

  // Inicializar conexão SignalR
  async initialize(token) {
    try {
      // Importar SignalR dinamicamente
      const { HubConnectionBuilder, LogLevel } = await import('@microsoft/signalr');

      this.connection = new HubConnectionBuilder()
        .withUrl('http://localhost:5000/chathub', {
          accessTokenFactory: () => token,
          withCredentials: true
        })
        .withAutomaticReconnect()
        .configureLogging(LogLevel.Information)
        .build();

      // Configurar event handlers
      this.setupEventHandlers();

      await this.connection.start();
      this.isConnected = true;
      console.log('SignalR conectado com sucesso!');

      return true;
    } catch (error) {
      console.error('Erro ao conectar SignalR:', error);
      this.isConnected = false;
      return false;
    }
  }

  // Configurar handlers de eventos
  setupEventHandlers() {
    if (!this.connection) return;

    // Evento quando usuário conecta
    this.connection.on('UserConnected', (data) => {
      this.triggerEvent('userConnected', data);
    });

    // Evento quando usuário desconecta
    this.connection.on('UserDisconnected', (data) => {
      this.triggerEvent('userDisconnected', data);
    });

    // Evento quando recebe mensagem
    this.connection.on('ReceiveMessage', (message) => {
      this.triggerEvent('receiveMessage', message);
    });

    // Evento quando mensagem é enviada
    this.connection.on('MessageSent', (data) => {
      this.triggerEvent('messageSent', data);
    });

    // Evento de erro
    this.connection.on('Error', (error) => {
      this.triggerEvent('error', error);
    });

    // Evento quando tutor está digitando
    this.connection.on('TutorTyping', (data) => {
      this.triggerEvent('tutorTyping', data);
    });

    // Eventos de pair programming
    this.connection.on('PairProgrammingInvite', (data) => {
      this.triggerEvent('pairProgrammingInvite', data);
    });

    this.connection.on('PairProgrammingAccepted', (data) => {
      this.triggerEvent('pairProgrammingAccepted', data);
    });

    // Eventos de grupo
    this.connection.on('UserJoinedGroup', (data) => {
      this.triggerEvent('userJoinedGroup', data);
    });

    this.connection.on('UserLeftGroup', (data) => {
      this.triggerEvent('userLeftGroup', data);
    });

    this.connection.on('CodeShared', (data) => {
      this.triggerEvent('codeShared', data);
    });
  }

  // Registrar handler de evento
  on(event, handler) {
    if (!this.eventHandlers[event]) {
      this.eventHandlers[event] = [];
    }
    this.eventHandlers[event].push(handler);
  }

  // Remover handler de evento
  off(event, handler) {
    if (this.eventHandlers[event]) {
      const index = this.eventHandlers[event].indexOf(handler);
      if (index > -1) {
        this.eventHandlers[event].splice(index, 1);
      }
    }
  }

  // Disparar evento
  triggerEvent(event, data) {
    if (this.eventHandlers[event]) {
      this.eventHandlers[event].forEach(handler => {
        handler(data);
      });
    }
  }

  // Enviar mensagem
  async sendMessage(message) {
    if (!this.connection || !this.isConnected) {
      throw new Error('SignalR não está conectado');
    }

    try {
      await this.connection.invoke('SendMessage', message);
    } catch (error) {
      console.error('Erro ao enviar mensagem:', error);
      throw error;
    }
  }

  // Ingressar em grupo
  async joinGroup(groupName) {
    if (!this.connection || !this.isConnected) {
      throw new Error('SignalR não está conectado');
    }

    try {
      await this.connection.invoke('JoinGroup', groupName);
    } catch (error) {
      console.error('Erro ao ingressar no grupo:', error);
      throw error;
    }
  }

  // Sair de grupo
  async leaveGroup(groupName) {
    if (!this.connection || !this.isConnected) {
      throw new Error('SignalR não está conectado');
    }

    try {
      await this.connection.invoke('LeaveGroup', groupName);
    } catch (error) {
      console.error('Erro ao sair do grupo:', error);
      throw error;
    }
  }

  // Iniciar pair programming
  async startPairProgramming(partnerId) {
    if (!this.connection || !this.isConnected) {
      throw new Error('SignalR não está conectado');
    }

    try {
      await this.connection.invoke('StartPairProgramming', partnerId);
    } catch (error) {
      console.error('Erro ao iniciar pair programming:', error);
      throw error;
    }
  }

  // Aceitar pair programming
  async acceptPairProgramming(sessionId) {
    if (!this.connection || !this.isConnected) {
      throw new Error('SignalR não está conectado');
    }

    try {
      await this.connection.invoke('AcceptPairProgramming', sessionId);
    } catch (error) {
      console.error('Erro ao aceitar pair programming:', error);
      throw error;
    }
  }

  // Compartilhar código
  async shareCode(sessionId, code, filename) {
    if (!this.connection || !this.isConnected) {
      throw new Error('SignalR não está conectado');
    }

    try {
      await this.connection.invoke('ShareCode', sessionId, code, filename);
    } catch (error) {
      console.error('Erro ao compartilhar código:', error);
      throw error;
    }
  }

  // Desconectar
  async disconnect() {
    if (this.connection && this.isConnected) {
      try {
        await this.connection.stop();
        this.isConnected = false;
        console.log('SignalR desconectado');
      } catch (error) {
        console.error('Erro ao desconectar SignalR:', error);
      }
    }
  }

  // Verificar status da conexão
  getConnectionStatus() {
    if (!this.connection) return 'disconnected';

    switch (this.connection.state) {
      case 0: return 'connecting';
      case 1: return 'connected';
      case 2: return 'reconnecting';
      case 3: return 'disconnected';
      default: return 'unknown';
    }
  }
}

// Exportar instância singleton
const signalRService = new SignalRService();
export default signalRService;
