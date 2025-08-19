import type { DocumentChunk } from '@tutor-copiloto/schemas';

export interface PromptContext {
  userId: string;
  conversationId: string;
  repoContext?: {
    name: string;
    branch: string;
    files: string[];
  };
  ragCitations?: DocumentChunk[];
  userLevel?: 'beginner' | 'intermediate' | 'advanced';
  persona?: 'tutor' | 'reviewer' | 'mentor';
  costMode?: 'low-cost' | 'deep-analysis';
}

export interface PromptTemplate {
  system: string;
  instructions: string;
  examples?: string;
}

export class PromptBuilder {
  private context: PromptContext;

  constructor(context: PromptContext) {
    this.context = context;
  }

  // Sistema base para o tutor
  getSystemPrompt(): string {
    const persona = this.getPersonaInstructions();
    const pedagogicalRules = this.getPedagogicalRules();
    const outputFormat = this.getOutputFormat();
    
    return `${persona}

${pedagogicalRules}

${outputFormat}

Contexto da sessão:
- Usuário: ${this.context.userId}
- Nível: ${this.context.userLevel || 'intermediate'}
- Modo: ${this.context.costMode || 'deep-analysis'}
- Persona: ${this.context.persona || 'tutor'}`;
  }

  private getPersonaInstructions(): string {
    switch (this.context.persona) {
      case 'tutor':
        return `Você é um Tutor Copiloto experiente e pedagógico. Sua missão é ajudar estudantes e desenvolvedores a aprender, depurar e melhorar código de forma educativa.

PRINCÍPIOS PEDAGÓGICOS:
- Faça perguntas de verificação antes de entregar soluções completas
- Estimule o raciocínio crítico através de pistas e orientações
- Adapte explicações ao nível do usuário
- Celebre progresso e aprendizado`;

      case 'reviewer':
        return `Você é um Code Reviewer experiente. Foque em qualidade de código, boas práticas, segurança e performance.

FOCOS DE REVISÃO:
- Legibilidade e manutenibilidade
- Padrões e convenções
- Potenciais bugs e vulnerabilidades
- Oportunidades de refatoração`;

      case 'mentor':
        return `Você é um Mentor de Carreira em tecnologia. Ajude com decisões técnicas, arquitetura e crescimento profissional.

ÁREAS DE MENTORIA:
- Escolhas arquiteturais
- Melhores práticas da indústria
- Desenvolvimento de carreira
- Liderança técnica`;

      default:
        return this.getPersonaInstructions();
    }
  }

  private getPedagogicalRules(): string {
    return `REGRAS PEDAGÓGICAS:
1. Nunca revele respostas de avaliações diretamente - dê pistas e peça tentativas
2. Ao usar ferramentas, explique o porquê e inclua referências do RAG quando aplicável
3. Para iniciantes: use linguagem simples e mais exemplos
4. Para avançados: seja mais direto e foque em nuances
5. Sempre cite fontes quando usar informações do RAG
6. Mantenha respostas focadas e práticas`;
  }

  private getOutputFormat(): string {
    return `FORMATO DE SAÍDA:
Estruture respostas em JSON quando apropriado:
{
  "explanation": "Explicação principal",
  "steps": ["Passo 1", "Passo 2", ...],
  "tools_used": ["ferramenta1", "ferramenta2"],
  "citations": [{"source": "...", "content": "..."}],
  "next_steps": "Sugestões para continuar"
}`;
  }

  // Template para explicar código
  getCodeExplanationPrompt(code: string, filePath?: string): PromptTemplate {
    const citations = this.formatCitations();
    
    return {
      system: this.getSystemPrompt(),
      instructions: `Analise o código fornecido e explique:

1. O que o código faz (propósito geral)
2. Como funciona (lógica principal)
3. Possíveis melhorias ou problemas
4. Conceitos importantes demonstrados

${citations}

Código para análise:
\`\`\`${this.getLanguageFromPath(filePath)}
${code}
\`\`\`

${filePath ? `Arquivo: ${filePath}` : ''}`,
      examples: `Exemplo de resposta estruturada:
{
  "explanation": "Este código implementa um algoritmo de ordenação...",
  "steps": [
    "Define a função de comparação",
    "Aplica o algoritmo quicksort recursivamente",
    "Retorna o array ordenado"
  ],
  "concepts": ["recursão", "divisão e conquista", "complexidade O(n log n)"],
  "improvements": ["Adicionar validação de entrada", "Otimizar para arrays pequenos"],
  "citations": []
}`
    };
  }

  // Template para gerar testes
  getTestGenerationPrompt(code: string, filePath: string): PromptTemplate {
    return {
      system: this.getSystemPrompt(),
      instructions: `Analise o código e gere testes abrangentes. Use a ferramenta run_tests para validar.

ESTRATÉGIA DE TESTES:
1. Casos normais (happy path)
2. Casos extremos (edge cases)
3. Casos de erro
4. Mocks quando necessário

Código para testar:
\`\`\`${this.getLanguageFromPath(filePath)}
${code}
\`\`\`

Arquivo: ${filePath}

Após gerar os testes, use run_tests para executá-los e itere até que passem.`,
      examples: `Para usar a ferramenta de testes:
{
  "tool": "run_tests",
  "input": {
    "repoPath": ".",
    "testCmd": "npm test -- --testNamePattern='FunctionName'",
    "timeoutSec": 120
  }
}`
    };
  }

  // Template para assessment/avaliação
  getAssessmentPrompt(task: string, rubric: string, userCode: string): PromptTemplate {
    return {
      system: this.getSystemPrompt() + `

MODO AVALIAÇÃO ATIVO:
- Use a ferramenta run_tests para executar testes
- Aplique a rubrica fornecida rigorosamente
- Forneça feedback construtivo e específico
- NÃO revele soluções completas, apenas oriente`,
      instructions: `Avalie a submissão do usuário conforme a tarefa e rubrica.

TAREFA:
${task}

RUBRICA:
${rubric}

CÓDIGO SUBMETIDO:
\`\`\`
${userCode}
\`\`\`

PROCESSO DE AVALIAÇÃO:
1. Execute os testes relevantes
2. Verifique conformidade com requisitos
3. Aplique cada critério da rubrica
4. Gere feedback específico e construtivo
5. Sugira próximos passos sem revelar solução`,
      examples: `Exemplo de avaliação:
{
  "score": 75,
  "maxScore": 100,
  "feedback": "Implementação funcional mas pode melhorar...",
  "rubric_results": [
    {"criterion": "Funcionalidade", "points": 8, "feedback": "Passa nos testes básicos"},
    {"criterion": "Qualidade", "points": 6, "feedback": "Código legível mas falta documentação"}
  ],
  "next_steps": "Adicione comentários e trate casos extremos"
}`
    };
  }

  // Template para refatoração e correções
  getRefactorPrompt(code: string, issue: string, filePath: string): PromptTemplate {
    return {
      system: this.getSystemPrompt(),
      instructions: `Analise o problema identificado e proponha refatoração.

PROBLEMA IDENTIFICADO:
${issue}

CÓDIGO ATUAL:
\`\`\`${this.getLanguageFromPath(filePath)}
${code}
\`\`\`

Arquivo: ${filePath}

PROCESSO:
1. Use static_check para identificar problemas adicionais
2. Proponha solução com explain_diff
3. Execute testes para validar
4. Explique as mudanças e benefícios

Foque em:
- Corrigir o problema específico
- Manter funcionalidade existente
- Melhorar legibilidade e manutenibilidade
- Seguir boas práticas`,
      examples: `Ferramentas para refatoração:
{
  "tool": "static_check",
  "input": {"repoPath": ".", "linter": "eslint", "paths": ["${filePath}"]}
}

{
  "tool": "explain_diff", 
  "input": {"oldContent": "...", "newContent": "...", "filePath": "${filePath}"}
}`
    };
  }

  private formatCitations(): string {
    if (!this.context.ragCitations?.length) return '';
    
    const citations = this.context.ragCitations
      .map((chunk, idx) => `[${idx + 1}] ${chunk.source}: ${chunk.content.substring(0, 200)}...`)
      .join('\n');
    
    return `FONTES DISPONÍVEIS:
${citations}

Cite as fontes relevantes na sua resposta usando [número].`;
  }

  private getLanguageFromPath(filePath?: string): string {
    if (!filePath) return '';
    
    const ext = filePath.split('.').pop()?.toLowerCase();
    const langMap: Record<string, string> = {
      'ts': 'typescript',
      'js': 'javascript',
      'py': 'python',
      'java': 'java',
      'cs': 'csharp',
      'cpp': 'cpp',
      'go': 'go',
      'rs': 'rust',
    };
    
    return langMap[ext || ''] || '';
  }
}
