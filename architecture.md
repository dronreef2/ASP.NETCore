Visão resumida

Um Tutor Copiloto multimodal e extensível que ajuda devs/alunos a aprender, depurar e melhorar código. Suporta:

UI: VS Code extension (principal UX) + Web SPA (React).

Orquestrador: BFF (Node/ASP.NET minimal API — escolha flexível).

Lógica LLM: Anthropic (Claude) para raciocínio complexo, streaming e tool use.

RAG: indexação de docs, repositórios e materiais de curso.

Ferramentas (tool use): execução de testes, leitura de repositório, linting, sandboxed execution.

Observabilidade e controle de custos.

1. Objetivos técnicos (nível sênior)

Experiência interativa com streaming de tokens (baixa latência).

Tool use robusto: LLM coordena ferramentas (run_tests, read_repo, search_docs, static_check, explain_diff).

Suporte a contexto longo (analisar repositórios inteiros com chunking + Sonnet/long-context models).

Segurança/isolamento (containers, limites de recursos).

Telemetria fincada em tokens, latência, custo por ação, sucesso de avaliação.

Testes automatizados: unit, integração tool_use, E2E pedagógico.

2. Arquitetura (alto nível)
clients/
  vscode-ext/    <-- extensão TypeScript
  web-spa/       <-- React (Vite/Next)
backend/
  orchestrator/  <-- Node (Fastify) ou ASP.NET Core Minimal API
  tools/         <-- microservices (test-runner, indexer, static-check)
infra/
  rag/           <-- vector DB (pgvector/Weaviate), embeddings worker
  observability/ <-- Prometheus/Grafana, OpenTelemetry collector
  deployments/   <-- helm/terraform
shared/
  prompts/
  schemas/
  policy/


Fluxo simplificado:

Usuário solicita ação (ex: “Explique este PR”).

Front envia mensagem ao BFF.

BFF formata prompt, injeta contexto RAG e envia p/ Anthropic (streaming).

Claude responde. Se stop_reason == tool_use, BFF executa a(s) ferramenta(s) (em container isolado) e retorna resultados ao modelo.

Modelo finaliza resposta; BFF persiste telemetria, armazena histórico e entrega ao cliente.

Escolha de BFF:

Node (TypeScript): melhor integração com SDKs JS (Anthropic, kiota clients) e ferramentas dev.

ASP.NET Core: se organização já usa .NET; ótimo para APIs minimal e hospedagem em Azure.

(Se precisar: gerar clientes de APIs com Kiota para serviços REST internos que precisem ser chamados pelo orquestrador.)

3. Features principais

Chat tutor (explicações passo-a-passo, perguntas de sondagem).

Explain this code: seleção de trecho → análise + sugestões.

Write tests: gera testes e executa run_tests até passar.

Fix & Refactor: propõe patch + explain_diff.

Assessment: run tests + rubric → nota + feedback detalhado.

RAG-powered citations: quando usa docs internas, cita trechos.

Session history com checkpoints e re-playing.

Role-based personas (tutor, reviewer, mentor).

Cost mode: low-cost (Haiku) vs deep-analysis (Sonnet).

4. Prompts — guia prático (templates)

princípio: system define persona + regras pedagógicas; assistant produz raciocínio visível; user fornece contexto.

System (exemplo)

Você é Tutor Copiloto experiente. Seja pedagógico: faça perguntas de verificação antes de entregar solução completa.
Ao usar ferramentas, explique por que e inclua referências do RAG.
Não revele respostas de avaliações diretamente; dê pistas e peça tentativa.
Formato de saída: JSON com campos {explanation, steps, tools_used, citations}.


User (exemplo)

User: "Explique o bug neste arquivo src/foo.ts entre linhas 120-160. Mostre possíveis causas, testes a adicionar e um patch mínimo."
Context: [repo metadata], [recent failing test output]


Tool-invocation instruction (exemplo curto)

Quando pedir run_tests, devolver apenas JSON:

{ "tool": "run_tests", "input": {"repo":"...","cmd":"npm test -- --testNamePattern='Foo'"} }

5. RAG — pipeline e políticas

Pipeline:

Ingestão: docs (markdown/HTML/PDF), readme, artigos, exercícios, repositórios.

Chunking: tamanho semântico (e.g., 1k-2k tokens), overlaps 20%.

Embeddings: usar embedding que balanceie custo/acurácia (provider pluggable).

Indexação: pgvector (Sev1) ou Weaviate (Sev2); metadados: path, repo, commit, source_type.

Query: HyDE (generate hypothetical answer) → search → rerank (BM25 + semantic) → return top-k passages + offsets.

Prompt assembly: incluir somatória de citações (trecho + source + score) e instrução para citar fonte.

Políticas:

Nunca inserir trechos com PII no prompt.

Limiar de similaridade para citar (p.ex.: >=0.75 cos sim).

RGC (rate gating) para evitar custos: cache results por query-hash.

6. Tool Use — contrato & exemplos

Definir cada tool com schema JSON, timeouts e sandbox.

Tool: run_tests

{
  "name":"run_tests",
  "description":"Executa os testes no repositório dado e retorna JUnit/JSON.",
  "input_schema": {
    "type":"object",
    "properties":{
      "repoPath":{"type":"string"},
      "testCmd":{"type":"string"},
      "timeoutSec":{"type":"integer"}
    },
    "required":["repoPath","testCmd"]
  },
  "timeout":120
}


Tool: read_repo

retorna trechos de arquivos por path/lines, commit hash, e hash de conteúdo.

Tool: static_check

roda ESLint/semgrep → retorna lista de issues com severidade.

Execução

Cada tool roda em container isolado com limites (CPU 0.5, RAM 512MB, timeout 120s).

Outputs sanitizados (strip secrets) antes de retornar ao modelo.

Fluxo tool_use

Model returns tool request (tool_use).

Orquestrador valida schema, enfileira execução.

Tool executes, stores result in artifact store (S3).

Orquestrador returns tool_result to model and resumes conversation.

7. Telemetria & observability

Métricas a capturar:

Request volume (per endpoint)

Latência (P90/P95) por etapa: BFF→LLM, tool exec, total RTT

Tokens in/out per request

Model chosen, model latency, stop_reason distribution

Cost per request (estimated $)

Tool invocation counts, durations, failure rates

Test-run success rates, flakiness

User engagement: session length, messages per session, acceptance of suggestions

Stack recomendado:

OpenTelemetry + collector → Prometheus + Grafana (dashboards)

Logs estruturados -> ELK / Datadog

Traces para cada conversation id (correlacionar tools)

Alertas:

Custo diário > threshold

Tool failure rate > X%

Latency SLO violation (p.ex., LLM latency P95 > 5s)

8. Testes (senior)

Unit: prompts templates, prompt assembler, tool adapters (mock Anthropic).

Integration: simulate tool_use loops end-to-end with local test doubles for Anthropic (deterministic responses).

Contract tests: tool schemas validated; each tool has contract tests.

E2E pedagógico: cenários de aula (iniciante → resolveu; pede dica; gera teste; corrige).

Security tests: prompt injection, data exfiltration, secrets leak simulation.

Chaos tests: degrade LLM (delays/errors), tool container crash.

Performance: concurrency tests, cost per user simulation.

Automação:

CI: run unit/integration on PRs.

Scheduled nightly E2E.

Canary deploy with canary tests for model change.

9. Custos & estratégia de mitigação

Modelo mix: Haiku (cheaper) para respostas rápidas e Sonnet para raciocínio pesado.

Prompt caching: cachear respostas a prompts imutáveis (instruções longas).

Tool gating: exigir confirmação do usuário antes de executar ferramentas custosas (run_tests full suite).

Sampling: reduzir max_tokens para tarefas não críticas.

Budgeting: per-org and per-user quota; rate-limits and daily caps.

Observability: daily billing ingest; alerts on cost spikes.

Estimativa tática:

Perfil de uso: 1000 sessões/dia, média 1.5 tool calls/sessão, 2k tokens/session → calcular com precificação do provedor e ajustar mix/model.

10. Deploy & infra (produção)

Containerize serviços (Docker).

Orquestração: Kubernetes (preferred) ou Azure Container Apps / ECS.

Vector DB: managed or self-hosted (weaviate/pgvector).

Object storage: S3/Blob para artifacts (test results, patches).

Secrets: HashiCorp Vault / Azure Key Vault.

CI/CD: GitHub Actions / Azure DevOps + Terraform + Helm charts.

Autoscaling: HPA baseado em CPU/queue-length; scale pods das tools independentemente.

Blue/Green ou Canary para deploys de orquestrador e mudança de model config.

Edge: CDN para SPA assets.

Segurança:

Network policies, egress restrictions, VPC endpoints.

DLP: bloquear upload de secrets into RAG.

Data retention policies & purge.

11. Governance, ética e proteção pedagógica

Política de não-revelação de soluções completas para avaliações.

Logging de interações de avaliação com retenção curta.

Consent prompts quando armazenar código do aluno.

Red-team contínuo para prompt injection.

12. Plano de entregas (roadmap simplificado — 6 sprints)

Sprint 0 — Setup (1 semana)

Monorepo scaffold, infra dev, CI, secrets.
Sprint 1 — MVP chat + streaming (2 semanas)

BFF, Anthropic integration (streaming), VS Code chat minimal.
Sprint 2 — Tool use básico (2 semanas)

run_tests (container), read_repo, tool loop.
Sprint 3 — RAG + citations (2 semanas)

indexer, vector DB, prompt assembly with citations.
Sprint 4 — Assessment & rubric (2 semanas)

runner, scoring, feedback generation.
Sprint 5 — Production hardening (2–3 semanas)

Telemetria, quotas, canary deploy, security audits.

13. Artefatos entregáveis (do repositório)

apps/orchestrator/src/claude.ts — adapter com handling de tool_use (ex.: o snippet que você já tem).

apps/vscode-ext/ — extensão com comandos: explain, write tests, run scenario.

packages/prompts/ — templates + tests de regressão de prompt.

infra/ — terraform/helm + manifestos de k8s.

docs/ — políticas (privacy, cost, evaluation rubric).

14. Observações práticas (baseado nas duas conversas)

Use Kiota para gerar clientes TypeScript de APIs REST internas/externas (útil para orquestrador chamar serviços REST sem escrever SDKs).

Considere Node para agilidade e integração com SDKs TS (Anthropic JS), e ASP.NET se você quiser unificar com infra .NET (kiota + ASP.NET minimal APIs funcionam bem).

Garanta moduleResolution: NodeNext e imports .js nos builds TypeScript ao trabalhar com ESM (lembrete do tutorial Kiota).