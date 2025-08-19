import * as vscode from 'vscode';
import * as path from 'path';

export class ChatPanel {
  public static currentPanel: ChatPanel | undefined;
  private static readonly viewType = 'tutorCopilotoChat';

  private readonly _panel: vscode.WebviewPanel;
  private readonly _extensionUri: vscode.Uri;
  private _disposables: vscode.Disposable[] = [];
  private _messages: Array<{ role: string; content: string; timestamp: Date }> = [];

  public static createOrShow(column?: vscode.ViewColumn): ChatPanel {
    const col = column || vscode.window.activeTextEditor?.viewColumn;

    // Se já existe, apenas mostra
    if (ChatPanel.currentPanel) {
      ChatPanel.currentPanel._panel.reveal(col);
      return ChatPanel.currentPanel;
    }

    // Senão, cria novo painel
    const panel = vscode.window.createWebviewPanel(
      ChatPanel.viewType,
      'Tutor Copiloto',
      col || vscode.ViewColumn.One,
      {
        enableScripts: true,
        retainContextWhenHidden: true,
        localResourceRoots: [],
      }
    );

    ChatPanel.currentPanel = new ChatPanel(panel, vscode.extensions.getExtension('tutor-copiloto.tutor-copiloto-vscode')!.extensionUri);
    return ChatPanel.currentPanel;
  }

  private constructor(panel: vscode.WebviewPanel, extensionUri: vscode.Uri) {
    this._panel = panel;
    this._extensionUri = extensionUri;

    // Configura HTML inicial
    this._update();

    // Escuta quando painel é fechado
    this._panel.onDidDispose(() => this.dispose(), null, this._disposables);

    // Escuta mensagens do webview
    this._panel.webview.onDidReceiveMessage(
      (message) => {
        switch (message.command) {
          case 'sendMessage':
            this._handleMessage(message.text);
            break;
          case 'clearChat':
            this._messages = [];
            this._update();
            break;
        }
      },
      null,
      this._disposables
    );
  }

  public addMessage(role: 'user' | 'assistant', content: string): void {
    this._messages.push({
      role,
      content,
      timestamp: new Date(),
    });
    this._update();
  }

  public focus(): void {
    this._panel.reveal(vscode.ViewColumn.Beside);
  }

  private async _handleMessage(text: string): Promise<void> {
    // Adiciona mensagem do usuário
    this.addMessage('user', text);

    try {
      // Aqui integraria com ApiClient para enviar mensagem
      // Por simplicidade, vou simular uma resposta
      setTimeout(() => {
        this.addMessage('assistant', `Recebido: ${text}`);
      }, 1000);
    } catch (error) {
      this.addMessage('assistant', `Erro: ${error}`);
    }
  }

  private _update(): void {
    this._panel.webview.html = this._getHtmlForWebview();
  }

  private _getHtmlForWebview(): string {
    const messagesHtml = this._messages
      .map(msg => `
        <div class="message ${msg.role}">
          <div class="message-header">
            <span class="role">${msg.role === 'user' ? '👤 Você' : '🎓 Tutor'}</span>
            <span class="time">${msg.timestamp.toLocaleTimeString()}</span>
          </div>
          <div class="message-content">${this._formatMessage(msg.content)}</div>
        </div>
      `)
      .join('');

    return `
      <!DOCTYPE html>
      <html lang="pt-br">
      <head>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>Tutor Copiloto</title>
        <style>
          body {
            font-family: var(--vscode-font-family);
            font-size: var(--vscode-font-size);
            color: var(--vscode-foreground);
            background-color: var(--vscode-editor-background);
            margin: 0;
            padding: 10px;
            display: flex;
            flex-direction: column;
            height: 100vh;
          }

          .header {
            padding: 10px 0;
            border-bottom: 1px solid var(--vscode-panel-border);
            margin-bottom: 10px;
          }

          .header h2 {
            margin: 0;
            color: var(--vscode-titleBar-activeForeground);
          }

          .chat-container {
            flex: 1;
            overflow-y: auto;
            padding: 10px 0;
          }

          .message {
            margin-bottom: 15px;
            padding: 10px;
            border-radius: 8px;
            border-left: 3px solid;
          }

          .message.user {
            border-left-color: var(--vscode-textLink-foreground);
            background-color: var(--vscode-textBlockQuote-background);
          }

          .message.assistant {
            border-left-color: var(--vscode-notificationsInfoIcon-foreground);
            background-color: var(--vscode-editor-inactiveSelectionBackground);
          }

          .message-header {
            display: flex;
            justify-content: space-between;
            margin-bottom: 5px;
            font-weight: bold;
          }

          .role {
            color: var(--vscode-textLink-foreground);
          }

          .time {
            font-size: 0.8em;
            color: var(--vscode-descriptionForeground);
          }

          .message-content {
            line-height: 1.5;
          }

          .message-content pre {
            background-color: var(--vscode-textCodeBlock-background);
            padding: 10px;
            border-radius: 4px;
            overflow-x: auto;
            font-family: var(--vscode-editor-font-family);
          }

          .message-content code {
            background-color: var(--vscode-textCodeBlock-background);
            padding: 2px 4px;
            border-radius: 3px;
            font-family: var(--vscode-editor-font-family);
          }

          .input-container {
            display: flex;
            gap: 10px;
            padding: 10px 0;
            border-top: 1px solid var(--vscode-panel-border);
          }

          .input-container input {
            flex: 1;
            padding: 8px;
            background-color: var(--vscode-input-background);
            border: 1px solid var(--vscode-input-border);
            color: var(--vscode-input-foreground);
            border-radius: 4px;
          }

          .input-container button {
            padding: 8px 16px;
            background-color: var(--vscode-button-background);
            color: var(--vscode-button-foreground);
            border: none;
            border-radius: 4px;
            cursor: pointer;
          }

          .input-container button:hover {
            background-color: var(--vscode-button-hoverBackground);
          }

          .input-container button:disabled {
            opacity: 0.6;
            cursor: not-allowed;
          }

          .actions {
            display: flex;
            gap: 5px;
            margin-bottom: 10px;
          }

          .actions button {
            font-size: 0.8em;
            padding: 4px 8px;
            background-color: var(--vscode-button-secondaryBackground);
            color: var(--vscode-button-secondaryForeground);
            border: none;
            border-radius: 3px;
            cursor: pointer;
          }
        </style>
      </head>
      <body>
        <div class="header">
          <h2>🎓 Tutor Copiloto</h2>
          <div class="actions">
            <button onclick="clearChat()">Limpar Chat</button>
            <button onclick="exportChat()">Exportar</button>
          </div>
        </div>

        <div class="chat-container" id="chatContainer">
          ${messagesHtml}
        </div>

        <div class="input-container">
          <input 
            type="text" 
            id="messageInput" 
            placeholder="Digite sua mensagem..." 
            onkeypress="handleKeyPress(event)"
          />
          <button onclick="sendMessage()" id="sendButton">Enviar</button>
        </div>

        <script>
          const vscode = acquireVsCodeApi();

          function sendMessage() {
            const input = document.getElementById('messageInput');
            const text = input.value.trim();
            
            if (text) {
              vscode.postMessage({
                command: 'sendMessage',
                text: text
              });
              input.value = '';
              input.focus();
            }
          }

          function handleKeyPress(event) {
            if (event.key === 'Enter') {
              sendMessage();
            }
          }

          function clearChat() {
            vscode.postMessage({
              command: 'clearChat'
            });
          }

          function exportChat() {
            // TODO: Implementar exportação
            alert('Funcionalidade em desenvolvimento');
          }

          // Auto-scroll para última mensagem
          function scrollToBottom() {
            const container = document.getElementById('chatContainer');
            container.scrollTop = container.scrollHeight;
          }

          // Foca no input quando carrega
          document.addEventListener('DOMContentLoaded', () => {
            document.getElementById('messageInput').focus();
            scrollToBottom();
          });

          // Scroll quando nova mensagem é adicionada
          const observer = new MutationObserver(() => {
            scrollToBottom();
          });

          observer.observe(document.getElementById('chatContainer'), {
            childList: true,
            subtree: true
          });
        </script>
      </body>
      </html>
    `;
  }

  private _formatMessage(content: string): string {
    // Converte markdown básico para HTML
    return content
      .replace(/```([\\s\\S]*?)```/g, '<pre><code>$1</code></pre>')
      .replace(/`([^`]+)`/g, '<code>$1</code>')
      .replace(/\\*\\*([^*]+)\\*\\*/g, '<strong>$1</strong>')
      .replace(/\\*([^*]+)\\*/g, '<em>$1</em>')
      .replace(/\\n/g, '<br>');
  }

  public dispose(): void {
    ChatPanel.currentPanel = undefined;

    this._panel.dispose();

    while (this._disposables.length) {
      const x = this._disposables.pop();
      if (x) {
        x.dispose();
      }
    }
  }
}
