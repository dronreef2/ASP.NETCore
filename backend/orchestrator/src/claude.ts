import Anthropic from '@anthropic-ai/sdk';
import type { 
  ClaudeTool, 
  ClaudeToolUse, 
  ClaudeToolResult, 
  ToolResult,
  RunTestsInput,
  ReadRepoInput,
  StaticCheckInput,
  SearchDocsInput,
  ExplainDiffInput
} from '@tutor-copiloto/schemas';
import { ToolExecutor } from './tool-executor.js';
import { TelemetryService } from './telemetry.js';

export interface ClaudeStreamingOptions {
  model?: 'claude-3-5-sonnet-20241022' | 'claude-3-haiku-20240307';
  maxTokens?: number;
  temperature?: number;
  stopSequences?: string[];
}

export class ClaudeService {
  private anthropic: Anthropic;
  private toolExecutor: ToolExecutor;
  private telemetry: TelemetryService;

  constructor() {
    this.anthropic = new Anthropic({
      apiKey: process.env.ANTHROPIC_API_KEY,
    });
    this.toolExecutor = new ToolExecutor();
    this.telemetry = new TelemetryService();
  }

  // Define as ferramentas disponíveis para Claude
  private getAvailableTools(): ClaudeTool[] {
    return [
      {
        name: 'run_tests',
        description: 'Executa testes no repositório e retorna resultados em formato JUnit/JSON',
        input_schema: {
          type: 'object',
          properties: {
            repoPath: { type: 'string', description: 'Caminho para o repositório' },
            testCmd: { type: 'string', description: 'Comando de teste a executar' },
            timeoutSec: { type: 'integer', description: 'Timeout em segundos', default: 120 }
          },
          required: ['repoPath', 'testCmd']
        }
      },
      {
        name: 'read_repo',
        description: 'Lê trechos de arquivos do repositório por path/lines',
        input_schema: {
          type: 'object',
          properties: {
            repoPath: { type: 'string', description: 'Caminho para o repositório' },
            paths: { 
              type: 'array', 
              items: { type: 'string' }, 
              description: 'Caminhos dos arquivos para ler' 
            },
            lines: {
              type: 'object',
              properties: {
                start: { type: 'integer' },
                end: { type: 'integer' }
              },
              description: 'Intervalo de linhas para ler'
            },
            commitHash: { type: 'string', description: 'Hash do commit específico' }
          },
          required: ['repoPath']
        }
      },
      {
        name: 'static_check',
        description: 'Executa análise estática (linting) no código',
        input_schema: {
          type: 'object',
          properties: {
            repoPath: { type: 'string', description: 'Caminho para o repositório' },
            linter: { 
              type: 'string', 
              enum: ['eslint', 'semgrep', 'sonarjs'],
              default: 'eslint',
              description: 'Ferramenta de linting a usar'
            },
            paths: {
              type: 'array',
              items: { type: 'string' },
              description: 'Caminhos específicos para analisar'
            }
          },
          required: ['repoPath']
        }
      },
      {
        name: 'search_docs',
        description: 'Busca semântica na base de conhecimento RAG',
        input_schema: {
          type: 'object',
          properties: {
            query: { type: 'string', description: 'Consulta de busca' },
            maxResults: { type: 'integer', default: 5, description: 'Máximo de resultados' },
            threshold: { type: 'number', default: 0.75, description: 'Limiar de similaridade' }
          },
          required: ['query']
        }
      },
      {
        name: 'explain_diff',
        description: 'Explica diferenças entre duas versões de código',
        input_schema: {
          type: 'object',
          properties: {
            oldContent: { type: 'string', description: 'Conteúdo original' },
            newContent: { type: 'string', description: 'Conteúdo modificado' },
            filePath: { type: 'string', description: 'Caminho do arquivo (opcional)' }
          },
          required: ['oldContent', 'newContent']
        }
      }
    ];
  }

  // Streaming chat com tool use
  async *streamChatWithTools(
    messages: Anthropic.MessageParam[],
    options: ClaudeStreamingOptions = {},
    conversationId: string,
    userId: string
  ): AsyncGenerator<string, void, unknown> {
    const startTime = Date.now();
    let inputTokens = 0;
    let outputTokens = 0;
    
    try {
      const stream = await this.anthropic.messages.create({
        model: options.model || 'claude-3-5-sonnet-20241022',
        max_tokens: options.maxTokens || 4096,
        temperature: options.temperature || 0.7,
        messages,
        tools: this.getAvailableTools(),
        stream: true,
        stop_sequences: options.stopSequences,
      });

      let currentToolUse: ClaudeToolUse | null = null;
      let accumulatedText = '';

      for await (const chunk of stream) {
        switch (chunk.type) {
          case 'message_start':
            inputTokens = chunk.message.usage.input_tokens;
            break;

          case 'content_block_start':
            if (chunk.content_block.type === 'tool_use') {
              currentToolUse = {
                type: 'tool_use',
                id: chunk.content_block.id,
                name: chunk.content_block.name,
                input: {},
              };
            }
            break;

          case 'content_block_delta':
            if (chunk.delta.type === 'text_delta') {
              accumulatedText += chunk.delta.text;
              yield chunk.delta.text;
            } else if (chunk.delta.type === 'input_json_delta' && currentToolUse) {
              // Acumula input da ferramenta
              try {
                const partialInput = JSON.parse(chunk.delta.partial_json);
                currentToolUse.input = { ...currentToolUse.input, ...partialInput };
              } catch {
                // JSON parcial, continua acumulando
              }
            }
            break;

          case 'content_block_stop':
            if (currentToolUse) {
              // Executa a ferramenta
              yield `\\n\\n🔧 Executando ferramenta: ${currentToolUse.name}...\\n\\n`;
              
              const toolResult = await this.executeTool(currentToolUse);
              
              // Adiciona resultado aos mensagens e continua conversação
              const updatedMessages: Anthropic.MessageParam[] = [
                ...messages,
                {
                  role: 'assistant',
                  content: [
                    ...(accumulatedText ? [{ type: 'text' as const, text: accumulatedText }] : []),
                    currentToolUse,
                  ],
                },
                {
                  role: 'user',
                  content: [{
                    type: 'tool_result',
                    tool_use_id: currentToolUse.id,
                    content: toolResult.success 
                      ? JSON.stringify(toolResult.data, null, 2)
                      : `Erro: ${toolResult.error}`,
                    is_error: !toolResult.success,
                  }],
                },
              ];

              // Continua a conversa recursivamente
              yield* this.streamChatWithTools(
                updatedMessages, 
                options, 
                conversationId, 
                userId
              );
              
              currentToolUse = null;
            }
            break;

          case 'message_delta':
            if (chunk.usage) {
              outputTokens = chunk.usage.output_tokens;
            }
            break;

          case 'message_stop':
            // Log telemetria
            await this.telemetry.logEvent({
              eventType: 'request',
              conversationId,
              userId,
              timestamp: new Date(),
              data: {
                model: options.model || 'claude-3-5-sonnet-20241022',
                duration: Date.now() - startTime,
                stopReason: 'end_turn',
              },
              tokens: { input: inputTokens, output: outputTokens },
              cost: this.calculateCost(options.model || 'claude-3-5-sonnet-20241022', inputTokens, outputTokens),
            });
            break;
        }
      }
    } catch (error) {
      // Log erro
      await this.telemetry.logEvent({
        eventType: 'error',
        conversationId,
        userId,
        timestamp: new Date(),
        data: {
          error: error instanceof Error ? error.message : 'Unknown error',
          duration: Date.now() - startTime,
        },
      });
      
      throw error;
    }
  }

  // Executa uma ferramenta específica
  private async executeTool(toolUse: ClaudeToolUse): Promise<ToolResult> {
    const startTime = Date.now();
    
    try {
      let result: any;
      
      switch (toolUse.name) {
        case 'run_tests':
          result = await this.toolExecutor.runTests(toolUse.input as RunTestsInput);
          break;
          
        case 'read_repo':
          result = await this.toolExecutor.readRepo(toolUse.input as ReadRepoInput);
          break;
          
        case 'static_check':
          result = await this.toolExecutor.staticCheck(toolUse.input as StaticCheckInput);
          break;
          
        case 'search_docs':
          result = await this.toolExecutor.searchDocs(toolUse.input as SearchDocsInput);
          break;
          
        case 'explain_diff':
          result = await this.toolExecutor.explainDiff(toolUse.input as ExplainDiffInput);
          break;
          
        default:
          throw new Error(`Ferramenta desconhecida: ${toolUse.name}`);
      }

      return {
        success: true,
        data: result,
        duration: Date.now() - startTime,
        metadata: {
          toolName: toolUse.name,
          toolId: toolUse.id,
        },
      };
    } catch (error) {
      return {
        success: false,
        data: null,
        error: error instanceof Error ? error.message : 'Erro desconhecido',
        duration: Date.now() - startTime,
        metadata: {
          toolName: toolUse.name,
          toolId: toolUse.id,
        },
      };
    }
  }

  // Calcula custo estimado baseado no modelo e tokens
  private calculateCost(model: string, inputTokens: number, outputTokens: number): number {
    // Preços aproximados por 1M tokens (MTok)
    const pricing: Record<string, { input: number; output: number }> = {
      'claude-3-5-sonnet-20241022': { input: 3.00, output: 15.00 },
      'claude-3-haiku-20240307': { input: 0.25, output: 1.25 },
    };

    const modelPricing = pricing[model] || pricing['claude-3-5-sonnet-20241022'];
    
    return (
      (inputTokens / 1_000_000) * modelPricing.input +
      (outputTokens / 1_000_000) * modelPricing.output
    );
  }
}
