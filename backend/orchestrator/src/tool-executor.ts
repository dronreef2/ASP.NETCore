import { exec } from 'child_process';
import { promisify } from 'util';
import fs from 'fs/promises';
import path from 'path';
import type {
  RunTestsInput,
  ReadRepoInput,
  StaticCheckInput,
  SearchDocsInput,
  ExplainDiffInput,
} from '@tutor-copiloto/schemas';
import { LlamaIndexService } from './services/llamaindex.js';

const execAsync = promisify(exec);

export class ToolExecutor {
  private readonly containerLimits = {
    memory: '512m',
    cpus: '0.5',
    timeout: 120000, // 2 minutes
  };

  async runTests(input: RunTestsInput): Promise<any> {
    const { repoPath, testCmd, timeoutSec = 120 } = input;
    
    try {
      // Executa em container isolado com limites
      const containerCmd = `docker run --rm \\
        --memory=${this.containerLimits.memory} \\
        --cpus=${this.containerLimits.cpus} \\
        -v "${path.resolve(repoPath)}:/workspace" \\
        -w /workspace \\
        --network none \\
        node:18-alpine \\
        timeout ${timeoutSec}s ${testCmd}`;

      const { stdout, stderr } = await execAsync(containerCmd, {
        timeout: timeoutSec * 1000,
        maxBuffer: 1024 * 1024, // 1MB
      });

      // Tenta parsear resultados em formato JUnit/JSON
      let results: any;
      try {
        results = JSON.parse(stdout);
      } catch {
        // Se não for JSON, retorna texto bruto
        results = {
          raw_output: stdout,
          stderr: stderr || undefined,
          format: 'text',
        };
      }

      return {
        success: stderr ? false : true,
        results,
        command: testCmd,
        duration: timeoutSec,
        repository: repoPath,
      };
    } catch (error) {
      return {
        success: false,
        error: error instanceof Error ? error.message : 'Erro ao executar testes',
        command: testCmd,
        repository: repoPath,
      };
    }
  }

  async readRepo(input: ReadRepoInput): Promise<any> {
    const { repoPath, paths, lines, commitHash } = input;

    try {
      const results: any = {};

      if (commitHash) {
        // Lê arquivos de um commit específico
        const { stdout } = await execAsync(
          `git -C "${repoPath}" show ${commitHash} --name-only`,
          { timeout: 10000 }
        );
        results.commit_files = stdout.trim().split('\\n');
      }

      if (paths) {
        // Lê arquivos específicos
        results.files = {};
        
        for (const filePath of paths) {
          const fullPath = path.join(repoPath, filePath);
          
          try {
            const content = await fs.readFile(fullPath, 'utf-8');
            
            if (lines) {
              const fileLines = content.split('\\n');
              const selectedLines = fileLines.slice(lines.start - 1, lines.end);
              results.files[filePath] = {
                content: selectedLines.join('\\n'),
                lines: lines,
                total_lines: fileLines.length,
              };
            } else {
              results.files[filePath] = {
                content,
                total_lines: content.split('\\n').length,
              };
            }
          } catch (fileError) {
            results.files[filePath] = {
              error: `Não foi possível ler o arquivo: ${fileError}`,
            };
          }
        }
      } else {
        // Lista estrutura do repositório
        const { stdout } = await execAsync(
          `find "${repoPath}" -type f -name "*.ts" -o -name "*.js" -o -name "*.py" -o -name "*.java" | head -20`,
          { timeout: 10000 }
        );
        
        results.structure = stdout.trim().split('\\n').map(f => 
          path.relative(repoPath, f)
        );
      }

      return results;
    } catch (error) {
      return {
        error: error instanceof Error ? error.message : 'Erro ao ler repositório',
        repository: repoPath,
      };
    }
  }

  async staticCheck(input: StaticCheckInput): Promise<any> {
    const { repoPath, linter = 'eslint', paths } = input;

    try {
      let lintCmd: string;
      const targetPaths = paths ? paths.join(' ') : '.';

      switch (linter) {
        case 'eslint':
          lintCmd = `npx eslint ${targetPaths} --format json`;
          break;
        case 'semgrep':
          lintCmd = `semgrep --config=auto ${targetPaths} --json`;
          break;
        case 'sonarjs':
          lintCmd = `npx eslint ${targetPaths} --format json --plugin @typescript-eslint/eslint-plugin`;
          break;
        default:
          throw new Error(`Linter não suportado: ${linter}`);
      }

      const containerCmd = `docker run --rm \\
        --memory=${this.containerLimits.memory} \\
        --cpus=${this.containerLimits.cpus} \\
        -v "${path.resolve(repoPath)}:/workspace" \\
        -w /workspace \\
        --network none \\
        node:18-alpine \\
        sh -c "npm install -g eslint @typescript-eslint/eslint-plugin && ${lintCmd}"`;

      const { stdout, stderr } = await execAsync(containerCmd, {
        timeout: this.containerLimits.timeout,
        maxBuffer: 1024 * 1024,
      });

      let issues: any[];
      try {
        issues = JSON.parse(stdout);
      } catch {
        issues = [{
          severity: 'error',
          message: 'Falha ao parsear resultado do linter',
          raw_output: stdout,
          stderr,
        }];
      }

      return {
        linter,
        issues_count: Array.isArray(issues) ? issues.length : 1,
        issues,
        paths: paths || ['all'],
      };
    } catch (error) {
      return {
        error: error instanceof Error ? error.message : 'Erro na análise estática',
        linter,
        repository: repoPath,
      };
    }
  }

  async searchDocs(input: SearchDocsInput): Promise<any> {
    const { query, maxResults = 5, threshold = 0.75 } = input;

    try {
      // Usa LlamaIndex para busca semântica real
      const results = await this.llamaIndex.semanticSearch(
        query,
        'tutor-copiloto-docs', // Nome do índice principal
        {
          topK: maxResults,
          threshold,
          includeMetadata: true,
        }
      );

      return {
        query,
        results_count: results.results.length,
        results: results.results.map(result => ({
          id: result.id,
          content: result.content,
          source: result.metadata.source || 'unknown',
          score: result.score,
          metadata: result.metadata,
        })),
        threshold,
        max_results: maxResults,
        usage: results.usage,
      };
    } catch (error) {
      console.error('Erro na busca LlamaIndex:', error);
      
      // Fallback para resultados mockados
      const mockResults = [
        {
          id: 'doc-1',
          content: `Exemplo de documentação sobre ${query}`,
          source: 'docs/tutorial.md',
          score: 0.89,
          metadata: {
            type: 'markdown',
            section: 'introduction',
          },
        },
        {
          id: 'doc-2', 
          content: `Mais informações relacionadas a ${query}`,
          source: 'docs/advanced.md',
          score: 0.82,
          metadata: {
            type: 'markdown',
            section: 'advanced-topics',
          },
        },
      ];

      const filteredResults = mockResults
        .filter(r => r.score >= threshold)
        .slice(0, maxResults);

      return {
        query,
        results_count: filteredResults.length,
        results: filteredResults,
        threshold,
        max_results: maxResults,
        fallback: true,
        error: error instanceof Error ? error.message : 'Erro desconhecido',
      };
    }
  }

  async explainDiff(input: ExplainDiffInput): Promise<any> {
    const { oldContent, newContent, filePath } = input;

    try {
      // Cria arquivos temporários para usar git diff
      const tempDir = await fs.mkdtemp('/tmp/diff-');
      const oldFile = path.join(tempDir, 'old.txt');
      const newFile = path.join(tempDir, 'new.txt');

      await fs.writeFile(oldFile, oldContent);
      await fs.writeFile(newFile, newContent);

      // Gera diff unificado
      const { stdout: diffOutput } = await execAsync(
        `diff -u "${oldFile}" "${newFile}" || true`,
        { timeout: 5000 }
      );

      // Calcula estatísticas
      const oldLines = oldContent.split('\\n');
      const newLines = newContent.split('\\n');
      
      const stats = {
        lines_added: newLines.length - oldLines.length,
        lines_removed: oldLines.length > newLines.length ? oldLines.length - newLines.length : 0,
        total_changes: Math.abs(newLines.length - oldLines.length),
      };

      // Cleanup
      await fs.rm(tempDir, { recursive: true });

      return {
        file_path: filePath,
        diff: diffOutput,
        statistics: stats,
        summary: this.generateDiffSummary(stats, filePath),
      };
    } catch (error) {
      return {
        error: error instanceof Error ? error.message : 'Erro ao explicar diff',
        file_path: filePath,
      };
    }
  }

  private generateDiffSummary(stats: any, filePath?: string): string {
    const { lines_added, lines_removed, total_changes } = stats;
    
    if (total_changes === 0) {
      return 'Nenhuma alteração detectada';
    }

    let summary = `${total_changes} linha(s) alterada(s)`;
    
    if (lines_added > 0) {
      summary += `, ${lines_added} adicionada(s)`;
    }
    
    if (lines_removed > 0) {
      summary += `, ${lines_removed} removida(s)`;
    }

    if (filePath) {
      summary += ` em ${filePath}`;
    }

    return summary;
  }
}
