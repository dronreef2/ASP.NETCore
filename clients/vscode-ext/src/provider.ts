import * as vscode from 'vscode';
import { ApiClient } from './api-client.js';

export class TutorCopilotoProvider implements vscode.TreeDataProvider<ChatItem> {
  private _onDidChangeTreeData: vscode.EventEmitter<ChatItem | undefined | null | void> = new vscode.EventEmitter<ChatItem | undefined | null | void>();
  readonly onDidChangeTreeData: vscode.Event<ChatItem | undefined | null | void> = this._onDidChangeTreeData.event;

  private conversations: any[] = [];
  private connected = false;

  constructor(
    private context: vscode.ExtensionContext,
    private apiClient: ApiClient
  ) {}

  refresh(): void {
    this._onDidChangeTreeData.fire();
  }

  getTreeItem(element: ChatItem): vscode.TreeItem {
    return element;
  }

  async getChildren(element?: ChatItem): Promise<ChatItem[]> {
    if (!this.connected) {
      return [
        new ChatItem(
          'Desconectado',
          'Clique para conectar ao servidor',
          vscode.TreeItemCollapsibleState.None,
          {
            command: 'tutorCopiloto.connect',
            title: 'Conectar'
          }
        )
      ];
    }

    if (!element) {
      // Root level - mostra conversações
      try {
        this.conversations = await this.apiClient.getConversations();
        
        const items: ChatItem[] = [
          new ChatItem(
            'Nova Conversa',
            'Iniciar nova conversa',
            vscode.TreeItemCollapsibleState.None,
            {
              command: 'tutorCopiloto.openChat',
              title: 'Nova Conversa'
            },
            'add'
          )
        ];

        this.conversations.forEach(conv => {
          items.push(
            new ChatItem(
              conv.lastMessage || 'Conversa vazia',
              `${conv.messageCount} mensagens - ${new Date(conv.updatedAt).toLocaleDateString()}`,
              vscode.TreeItemCollapsibleState.None,
              {
                command: 'tutorCopiloto.openConversation',
                title: 'Abrir Conversa',
                arguments: [conv.id]
              },
              'comment-discussion'
            )
          );
        });

        return items;
      } catch (error) {
        return [
          new ChatItem(
            'Erro',
            `Falha ao carregar conversas: ${error}`,
            vscode.TreeItemCollapsibleState.None
          )
        ];
      }
    }

    return [];
  }

  async connect(): Promise<void> {
    try {
      const healthy = await this.apiClient.checkHealth();
      if (healthy) {
        this.connected = true;
        vscode.window.showInformationMessage('✅ Conectado ao Tutor Copiloto');
      } else {
        throw new Error('Servidor não responde');
      }
    } catch (error) {
      this.connected = false;
      vscode.window.showErrorMessage(`❌ Falha ao conectar: ${error}`);
    }
    
    this.refresh();
  }

  disconnect(): void {
    this.connected = false;
    this.refresh();
  }
}

class ChatItem extends vscode.TreeItem {
  constructor(
    public readonly label: string,
    public readonly tooltip: string,
    public readonly collapsibleState: vscode.TreeItemCollapsibleState,
    public readonly command?: vscode.Command,
    iconName?: string
  ) {
    super(label, collapsibleState);
    
    this.tooltip = tooltip;
    this.description = tooltip;
    
    if (iconName) {
      this.iconPath = new vscode.ThemeIcon(iconName);
    }
  }
}
