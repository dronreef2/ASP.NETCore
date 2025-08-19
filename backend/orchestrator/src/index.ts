import Fastify from 'fastify';
import cors from '@fastify/cors';
import websocket from '@fastify/websocket';
import swagger from '@fastify/swagger';
import swaggerUi from '@fastify/swagger-ui';
import { ClaudeService } from './claude.js';
import { PromptBuilder } from '@tutor-copiloto/prompts';
import type { ChatMessage, Conversation } from '@tutor-copiloto/schemas';
import { v4 as uuidv4 } from 'uuid';

const fastify = Fastify({
  logger: {
    level: 'info',
    transport: {
      target: 'pino-pretty',
      options: {
        colorize: true
      }
    }
  }
});

// Plugins
await fastify.register(cors, {
  origin: ['http://localhost:3000', 'http://localhost:5173', 'vscode-webview://*'],
  credentials: true,
});

await fastify.register(websocket);

await fastify.register(swagger, {
  swagger: {
    info: {
      title: 'Tutor Copiloto API',
      description: 'API do orquestrador para o Tutor Copiloto',
      version: '1.0.0',
    },
    host: 'localhost:8080',
    schemes: ['http', 'https'],
    consumes: ['application/json'],
    produces: ['application/json'],
  },
});

await fastify.register(swaggerUi, {
  routePrefix: '/docs',
  uiConfig: {
    docExpansion: 'full',
    deepLinking: false,
  },
});

// Serviços
const claudeService = new ClaudeService();

// Armazenamento temporário (em produção usar Redis/PostgreSQL)
const conversations = new Map<string, Conversation>();
const activeConnections = new Map<string, any>();

// Schemas para validação
const chatRequestSchema = {
  type: 'object',
  required: ['message', 'userId'],
  properties: {
    message: { type: 'string' },
    userId: { type: 'string' },
    conversationId: { type: 'string' },
    context: {
      type: 'object',
      properties: {
        repoPath: { type: 'string' },
        filePath: { type: 'string' },
        selectedCode: { type: 'string' },
        userLevel: { type: 'string', enum: ['beginner', 'intermediate', 'advanced'] },
        persona: { type: 'string', enum: ['tutor', 'reviewer', 'mentor'] },
        costMode: { type: 'string', enum: ['low-cost', 'deep-analysis'] },
      },
    },
  },
};

// Rotas REST
fastify.post('/api/chat', {
  schema: {
    body: chatRequestSchema,
    response: {
      200: {
        type: 'object',
        properties: {
          conversationId: { type: 'string' },
          response: { type: 'string' },
        },
      },
    },
  },
}, async (request, reply) => {
  const { message, userId, conversationId: existingConversationId, context } = request.body as any;
  
  try {
    // Cria ou recupera conversação
    const conversationId = existingConversationId || uuidv4();
    let conversation = conversations.get(conversationId);
    
    if (!conversation) {
      conversation = {
        id: conversationId,
        userId,
        messages: [],
        context,
        createdAt: new Date(),
        updatedAt: new Date(),
      };
      conversations.set(conversationId, conversation);
    }

    // Adiciona mensagem do usuário
    const userMessage: ChatMessage = {
      id: uuidv4(),
      role: 'user',
      content: message,
      timestamp: new Date(),
      metadata: context,
    };
    
    conversation.messages.push(userMessage);

    // Prepara prompt com contexto
    const promptBuilder = new PromptBuilder({
      userId,
      conversationId,
      userLevel: context?.userLevel,
      persona: context?.persona,
      costMode: context?.costMode,
      repoContext: context?.repoPath ? {
        name: context.repoPath,
        branch: 'main',
        files: [],
      } : undefined,
    });

    // Converte mensagens para formato Claude
    const claudeMessages = conversation.messages.map(msg => ({
      role: msg.role === 'system' ? 'user' : msg.role,
      content: msg.content,
    }));

    // Adiciona prompt do sistema se for primeira mensagem
    if (conversation.messages.length === 1) {
      claudeMessages.unshift({
        role: 'user',
        content: promptBuilder.getSystemPrompt(),
      });
    }

    // Streaming response (para REST, coletamos tudo)
    let fullResponse = '';
    
    for await (const chunk of claudeService.streamChatWithTools(
      claudeMessages as any[],
      {
        model: context?.costMode === 'low-cost' ? 'claude-3-haiku-20240307' : 'claude-3-5-sonnet-20241022',
        maxTokens: 4096,
      },
      conversationId,
      userId
    )) {
      fullResponse += chunk;
    }

    // Adiciona resposta do assistente
    const assistantMessage: ChatMessage = {
      id: uuidv4(),
      role: 'assistant',
      content: fullResponse,
      timestamp: new Date(),
    };
    
    conversation.messages.push(assistantMessage);
    conversation.updatedAt = new Date();

    return {
      conversationId,
      response: fullResponse,
    };
  } catch (error) {
    request.log.error(error);
    reply.status(500);
    return {
      error: 'Erro interno do servidor',
      message: error instanceof Error ? error.message : 'Erro desconhecido',
    };
  }
});

// WebSocket para streaming em tempo real
fastify.register(async function (fastify) {
  fastify.get('/api/chat/stream', { websocket: true }, (connection, req) => {
    const connectionId = uuidv4();
    activeConnections.set(connectionId, connection);
    
    connection.socket.on('message', async (message) => {
      try {
        const data = JSON.parse(message.toString());
        const { message: userMessage, userId, conversationId: existingConversationId, context } = data;

        // Similar ao endpoint REST, mas com streaming real
        const conversationId = existingConversationId || uuidv4();
        let conversation = conversations.get(conversationId);
        
        if (!conversation) {
          conversation = {
            id: conversationId,
            userId,
            messages: [],
            context,
            createdAt: new Date(),
            updatedAt: new Date(),
          };
          conversations.set(conversationId, conversation);
        }

        // Adiciona mensagem do usuário
        const userMsg: ChatMessage = {
          id: uuidv4(),
          role: 'user',
          content: userMessage,
          timestamp: new Date(),
          metadata: context,
        };
        
        conversation.messages.push(userMsg);

        // Prepara prompt
        const promptBuilder = new PromptBuilder({
          userId,
          conversationId,
          userLevel: context?.userLevel,
          persona: context?.persona,
          costMode: context?.costMode,
        });

        const claudeMessages = conversation.messages.map(msg => ({
          role: msg.role === 'system' ? 'user' : msg.role,
          content: msg.content,
        }));

        if (conversation.messages.length === 1) {
          claudeMessages.unshift({
            role: 'user',
            content: promptBuilder.getSystemPrompt(),
          });
        }

        // Envia confirmação de início
        connection.socket.send(JSON.stringify({
          type: 'stream_start',
          conversationId,
        }));

        let fullResponse = '';

        // Stream em tempo real
        for await (const chunk of claudeService.streamChatWithTools(
          claudeMessages as any[],
          {
            model: context?.costMode === 'low-cost' ? 'claude-3-haiku-20240307' : 'claude-3-5-sonnet-20241022',
          },
          conversationId,
          userId
        )) {
          fullResponse += chunk;
          
          // Envia chunk para cliente
          connection.socket.send(JSON.stringify({
            type: 'stream_chunk',
            chunk,
            conversationId,
          }));
        }

        // Finaliza stream
        connection.socket.send(JSON.stringify({
          type: 'stream_end',
          conversationId,
          fullResponse,
        }));

        // Salva resposta completa
        const assistantMessage: ChatMessage = {
          id: uuidv4(),
          role: 'assistant',
          content: fullResponse,
          timestamp: new Date(),
        };
        
        conversation.messages.push(assistantMessage);
        conversation.updatedAt = new Date();

      } catch (error) {
        connection.socket.send(JSON.stringify({
          type: 'error',
          error: error instanceof Error ? error.message : 'Erro desconhecido',
        }));
      }
    });

    connection.socket.on('close', () => {
      activeConnections.delete(connectionId);
    });
  });
});

// Rota para listar conversações
fastify.get('/api/conversations/:userId', async (request, reply) => {
  const { userId } = request.params as { userId: string };
  
  const userConversations = Array.from(conversations.values())
    .filter(conv => conv.userId === userId)
    .map(conv => ({
      id: conv.id,
      createdAt: conv.createdAt,
      updatedAt: conv.updatedAt,
      messageCount: conv.messages.length,
      lastMessage: conv.messages[conv.messages.length - 1]?.content.substring(0, 100) + '...',
    }));

  return { conversations: userConversations };
});

// Rota para obter conversação específica
fastify.get('/api/conversations/:userId/:conversationId', async (request, reply) => {
  const { userId, conversationId } = request.params as { userId: string; conversationId: string };
  
  const conversation = conversations.get(conversationId);
  
  if (!conversation || conversation.userId !== userId) {
    reply.status(404);
    return { error: 'Conversação não encontrada' };
  }

  return conversation;
});

// Health check
fastify.get('/health', async () => {
  return { 
    status: 'healthy', 
    timestamp: new Date().toISOString(),
    version: '1.0.0',
  };
});

// Inicialização
const start = async (): Promise<void> => {
  try {
    const port = parseInt(process.env.PORT || '8080');
    const host = process.env.HOST || '0.0.0.0';
    
    await fastify.listen({ port, host });
    
    fastify.log.info(`🚀 Tutor Copiloto Orchestrator rodando em http://${host}:${port}`);
    fastify.log.info(`📚 Documentação disponível em http://${host}:${port}/docs`);
  } catch (err) {
    fastify.log.error(err);
    process.exit(1);
  }
};

start();
