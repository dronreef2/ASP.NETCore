import axios, { AxiosInstance } from 'axios';
import * as vscode from 'vscode';

export interface ApiResponse {
  conversationId: string;
  response: string;
}

export class ApiClient {
  private client: AxiosInstance;
  private baseUrl: string;

  constructor() {
    this.baseUrl = this.getApiUrl();
    this.client = axios.create({
      baseURL: this.baseUrl,
      timeout: 60000,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    // Atualiza URL quando configuração muda
    vscode.workspace.onDidChangeConfiguration((e) => {
      if (e.affectsConfiguration('tutorCopiloto.apiUrl')) {
        this.baseUrl = this.getApiUrl();
        this.client.defaults.baseURL = this.baseUrl;
      }
    });
  }

  private getApiUrl(): string {
    const config = vscode.workspace.getConfiguration('tutorCopiloto');
    return config.get<string>('apiUrl', 'http://localhost:8080');
  }

  private getContext(): any {
    const config = vscode.workspace.getConfiguration('tutorCopiloto');
    const workspaceFolder = vscode.workspace.workspaceFolders?.[0];
    
    return {
      userLevel: config.get<string>('userLevel', 'intermediate'),
      persona: config.get<string>('persona', 'tutor'),
      costMode: config.get<string>('costMode', 'deep-analysis'),
      repoPath: workspaceFolder?.uri.fsPath,
    };
  }

  async explainCode(code: string, filePath: string): Promise<ApiResponse> {
    const response = await this.client.post('/api/chat', {
      message: `Explique este código do arquivo ${filePath}:\n\`\`\`\n${code}\n\`\`\``,
      userId: this.getUserId(),
      context: {
        ...this.getContext(),
        filePath,
        selectedCode: code,
      },
    });

    return response.data;
  }

  async generateTests(code: string, filePath: string): Promise<ApiResponse> {
    const response = await this.client.post('/api/chat', {
      message: `Gere testes abrangentes para este código do arquivo ${filePath}:\n\`\`\`\n${code}\n\`\`\``,
      userId: this.getUserId(),
      context: {
        ...this.getContext(),
        filePath,
        selectedCode: code,
      },
    });

    return response.data;
  }

  async fixCode(code: string, issue: string, filePath: string): Promise<ApiResponse> {
    const response = await this.client.post('/api/chat', {
      message: `Analise e corrija este problema: "${issue}"\n\nCódigo do arquivo ${filePath}:\n\`\`\`\n${code}\n\`\`\``,
      userId: this.getUserId(),
      context: {
        ...this.getContext(),
        filePath,
        selectedCode: code,
      },
    });

    return response.data;
  }

  async assessCode(code: string, rubric: string, filePath: string): Promise<ApiResponse> {
    const response = await this.client.post('/api/chat', {
      message: `Avalie este código do arquivo ${filePath} usando os critérios: "${rubric}"\n\n\`\`\`\n${code}\n\`\`\``,
      userId: this.getUserId(),
      context: {
        ...this.getContext(),
        filePath,
      },
    });

    return response.data;
  }

  async runScenario(scenario: string): Promise<ApiResponse> {
    const response = await this.client.post('/api/chat', {
      message: `Quero praticar o cenário: "${scenario}". Crie um exercício prático para mim.`,
      userId: this.getUserId(),
      context: this.getContext(),
    });

    return response.data;
  }

  async sendMessage(message: string, conversationId?: string): Promise<ApiResponse> {
    const response = await this.client.post('/api/chat', {
      message,
      userId: this.getUserId(),
      conversationId,
      context: this.getContext(),
    });

    return response.data;
  }

  async getConversations(): Promise<any[]> {
    const response = await this.client.get(`/api/conversations/${this.getUserId()}`);
    return response.data.conversations;
  }

  async getConversation(conversationId: string): Promise<any> {
    const response = await this.client.get(`/api/conversations/${this.getUserId()}/${conversationId}`);
    return response.data;
  }

  private getUserId(): string {
    // Em produção, usaria identificador único do usuário
    // Por enquanto, usa uma combinação de workspace + machine ID
    const workspaceFolder = vscode.workspace.workspaceFolders?.[0];
    const workspaceName = workspaceFolder?.name || 'default';
    const machineId = vscode.env.machineId;
    
    return `${machineId}-${workspaceName}`;
  }

  // Health check
  async checkHealth(): Promise<boolean> {
    try {
      const response = await this.client.get('/health');
      return response.status === 200;
    } catch {
      return false;
    }
  }
}
