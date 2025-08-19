import * as vscode from 'vscode';
import { TutorCopilotoProvider } from './provider.js';
import { ChatPanel } from './chat-panel.js';
import { ApiClient } from './api-client.js';

let tutorProvider: TutorCopilotoProvider;
let apiClient: ApiClient;

export function activate(context: vscode.ExtensionContext): void {
  console.log('🎓 Tutor Copiloto ativado!');

  // Inicializa serviços
  apiClient = new ApiClient();
  tutorProvider = new TutorCopilotoProvider(context, apiClient);

  // Registra comandos
  const commands = [
    vscode.commands.registerCommand('tutorCopiloto.explainCode', () => explainSelectedCode()),
    vscode.commands.registerCommand('tutorCopiloto.writeTests', () => generateTests()),
    vscode.commands.registerCommand('tutorCopiloto.fixCode', () => fixAndRefactor()),
    vscode.commands.registerCommand('tutorCopiloto.assessCode', () => assessCode()),
    vscode.commands.registerCommand('tutorCopiloto.openChat', () => openChat()),
    vscode.commands.registerCommand('tutorCopiloto.runScenario', () => runScenario()),
  ];

  // Registra tree view
  const treeView = vscode.window.createTreeView('tutorCopilotoChat', {
    treeDataProvider: tutorProvider,
    showCollapseAll: true,
  });

  // Adiciona ao contexto
  context.subscriptions.push(...commands, treeView);

  // Define contexto para when clauses
  vscode.commands.executeCommand('setContext', 'tutorCopiloto.enabled', true);

  // Auto-conecta se configurado
  const config = vscode.workspace.getConfiguration('tutorCopiloto');
  if (config.get<boolean>('autoConnect', true)) {
    tutorProvider.connect();
  }
}

export function deactivate(): void {
  if (tutorProvider) {
    tutorProvider.disconnect();
  }
}

// Comandos implementados

async function explainSelectedCode(): Promise<void> {
  const editor = vscode.window.activeTextEditor;
  if (!editor) {
    vscode.window.showWarningMessage('Nenhum editor ativo');
    return;
  }

  const selection = editor.selection;
  const selectedText = editor.document.getText(selection);
  
  if (!selectedText.trim()) {
    vscode.window.showWarningMessage('Selecione um trecho de código para explicar');
    return;
  }

  const filePath = editor.document.uri.fsPath;
  const relativePath = vscode.workspace.asRelativePath(filePath);

  try {
    // Mostra progresso
    await vscode.window.withProgress({
      location: vscode.ProgressLocation.Notification,
      title: 'Explicando código...',
      cancellable: false,
    }, async () => {
      const response = await apiClient.explainCode(selectedText, relativePath);
      
      // Abre painel de chat com a explicação
      const panel = ChatPanel.createOrShow(vscode.window.activeTextEditor?.viewColumn);
      panel.addMessage('user', `Explique este código:\n\`\`\`\n${selectedText}\n\`\`\``);
      panel.addMessage('assistant', response.response);
    });
  } catch (error) {
    vscode.window.showErrorMessage(`Erro ao explicar código: ${error}`);
  }
}

async function generateTests(): Promise<void> {
  const editor = vscode.window.activeTextEditor;
  if (!editor) {
    vscode.window.showWarningMessage('Nenhum editor ativo');
    return;
  }

  const selection = editor.selection;
  const selectedText = editor.document.getText(selection);
  
  if (!selectedText.trim()) {
    vscode.window.showWarningMessage('Selecione código para gerar testes');
    return;
  }

  const filePath = editor.document.uri.fsPath;
  const relativePath = vscode.workspace.asRelativePath(filePath);

  try {
    await vscode.window.withProgress({
      location: vscode.ProgressLocation.Notification,
      title: 'Gerando testes...',
      cancellable: false,
    }, async () => {
      const response = await apiClient.generateTests(selectedText, relativePath);
      
      // Cria arquivo de teste
      const testFileName = getTestFileName(relativePath);
      const testUri = vscode.Uri.file(vscode.workspace.workspaceFolders![0].uri.fsPath + '/' + testFileName);
      
      const edit = new vscode.WorkspaceEdit();
      edit.createFile(testUri, { ignoreIfExists: true });
      edit.insert(testUri, new vscode.Position(0, 0), response.response);
      
      await vscode.workspace.applyEdit(edit);
      
      // Abre arquivo de teste
      const doc = await vscode.workspace.openTextDocument(testUri);
      await vscode.window.showTextDocument(doc);
      
      vscode.window.showInformationMessage('Testes gerados com sucesso!');
    });
  } catch (error) {
    vscode.window.showErrorMessage(`Erro ao gerar testes: ${error}`);
  }
}

async function fixAndRefactor(): Promise<void> {
  const editor = vscode.window.activeTextEditor;
  if (!editor) {
    vscode.window.showWarningMessage('Nenhum editor ativo');
    return;
  }

  const selection = editor.selection;
  const selectedText = editor.document.getText(selection);
  
  if (!selectedText.trim()) {
    vscode.window.showWarningMessage('Selecione código para corrigir');
    return;
  }

  const filePath = editor.document.uri.fsPath;
  const relativePath = vscode.workspace.asRelativePath(filePath);

  // Pede para o usuário descrever o problema
  const issue = await vscode.window.showInputBox({
    prompt: 'Descreva o problema ou melhoria desejada',
    placeHolder: 'Ex: Otimizar performance, corrigir bug, melhorar legibilidade...',
  });

  if (!issue) return;

  try {
    await vscode.window.withProgress({
      location: vscode.ProgressLocation.Notification,
      title: 'Analisando e corrigindo...',
      cancellable: false,
    }, async () => {
      const response = await apiClient.fixCode(selectedText, issue, relativePath);
      
      // Abre diff editor se houver sugestão de código
      if (response.response.includes('```')) {
        // Extrai código sugerido
        const codeMatch = response.response.match(/```[\w]*\n([\s\S]*?)\n```/);
        if (codeMatch) {
          const suggestedCode = codeMatch[1];
          
          // Cria arquivo temporário com sugestão
          const tempUri = vscode.Uri.parse(`untitled:suggested-${Date.now()}.${getFileExtension(relativePath)}`);
          const tempDoc = await vscode.workspace.openTextDocument(tempUri);
          const edit = new vscode.WorkspaceEdit();
          edit.insert(tempUri, new vscode.Position(0, 0), suggestedCode);
          await vscode.workspace.applyEdit(edit);
          
          // Abre diff
          await vscode.commands.executeCommand('vscode.diff', 
            editor.document.uri, 
            tempDoc.uri, 
            `${relativePath} ↔ Sugestão`
          );
        }
      }
      
      // Mostra explicação no chat
      const panel = ChatPanel.createOrShow(vscode.window.activeTextEditor?.viewColumn);
      panel.addMessage('user', `Corrigir: ${issue}\n\`\`\`\n${selectedText}\n\`\`\``);
      panel.addMessage('assistant', response.response);
    });
  } catch (error) {
    vscode.window.showErrorMessage(`Erro ao corrigir código: ${error}`);
  }
}

async function assessCode(): Promise<void> {
  const editor = vscode.window.activeTextEditor;
  if (!editor) {
    vscode.window.showWarningMessage('Nenhum editor ativo');
    return;
  }

  const document = editor.document;
  const fullText = document.getText();
  const filePath = document.uri.fsPath;
  const relativePath = vscode.workspace.asRelativePath(filePath);

  // Pede rubrica ou critérios
  const rubric = await vscode.window.showInputBox({
    prompt: 'Critérios de avaliação (opcional)',
    placeHolder: 'Ex: funcionalidade, legibilidade, performance, testes...',
  });

  try {
    await vscode.window.withProgress({
      location: vscode.ProgressLocation.Notification,
      title: 'Avaliando código...',
      cancellable: false,
    }, async () => {
      const response = await apiClient.assessCode(fullText, rubric || 'qualidade geral', relativePath);
      
      // Mostra resultado em painel
      const panel = ChatPanel.createOrShow(vscode.ViewColumn.Beside);
      panel.addMessage('user', `Avaliar arquivo: ${relativePath}`);
      panel.addMessage('assistant', response.response);
    });
  } catch (error) {
    vscode.window.showErrorMessage(`Erro ao avaliar código: ${error}`);
  }
}

async function openChat(): Promise<void> {
  const panel = ChatPanel.createOrShow(vscode.ViewColumn.Beside);
  panel.focus();
}

async function runScenario(): Promise<void> {
  // Lista cenários disponíveis
  const scenarios = [
    'Depuração de bug',
    'Revisão de código',
    'Otimização de performance',
    'Implementação de feature',
    'Refatoração',
    'Criação de testes',
  ];

  const selected = await vscode.window.showQuickPick(scenarios, {
    placeHolder: 'Selecione um cenário de aprendizado',
  });

  if (!selected) return;

  const panel = ChatPanel.createOrShow(vscode.ViewColumn.Beside);
  panel.addMessage('user', `Quero praticar: ${selected}`);
  
  try {
    const response = await apiClient.runScenario(selected);
    panel.addMessage('assistant', response.response);
  } catch (error) {
    vscode.window.showErrorMessage(`Erro ao executar cenário: ${error}`);
  }
}

// Funções auxiliares

function getTestFileName(filePath: string): string {
  const ext = getFileExtension(filePath);
  const name = filePath.replace(/\.[^/.]+$/, '');
  
  // Convenções por linguagem
  switch (ext) {
    case 'ts':
    case 'js':
      return `${name}.test.${ext}`;
    case 'py':
      return `test_${name.split('/').pop()}.py`;
    case 'java':
      return `${name}Test.java`;
    case 'cs':
      return `${name}Tests.cs`;
    default:
      return `${name}.test.${ext}`;
  }
}

function getFileExtension(filePath: string): string {
  return filePath.split('.').pop() || '';
}
