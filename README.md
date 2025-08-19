# 🎓 Tutor Copiloto

> Um assistente IA multimodal e extensível para aprendizado e desenvolvimento de código

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.5+-blue.svg)](https://www.typescriptlang.org/)
[![Node.js](https://img.shields.io/badge/Node.js-18+-green.svg)](https://nodejs.org/)
[![React](https://img.shields.io/badge/React-18+-61DAFB.svg)](https://reactjs.org/)
[![Kubernetes](https://img.shields.io/badge/Kubernetes-1.25+-326CE5.svg)](https://kubernetes.io/)

## 🌟 Características Principais

- **🤖 IA Avançada**: Integração com Anthropic Claude para raciocínio complexo e tool use
- **🔄 Streaming em Tempo Real**: Experiência interativa com baixa latência
- **🛠️ Tool Use Robusto**: Execução de testes, análise de código, linting e mais
- **📚 RAG Inteligente**: Base de conhecimento indexada com citações precisas
- **👨‍🏫 Pedagogia Adaptativa**: Personas ajustáveis (tutor, revisor, mentor)
- **🎯 Múltiplas Interfaces**: VS Code extension + Web SPA + API REST
- **🔒 Segurança Empresarial**: Isolamento, RBAC, auditoria completa
- **📊 Observabilidade Total**: Telemetria, custos, métricas educacionais

## 🏗️ Arquitetura

Esta plataforma oferece **duas implementações de backend**:

### 🟢 Node.js + TypeScript (Principal)
```mermaid
graph TB
    subgraph "Clientes"
        VSCode[VS Code Extension]
        WebSPA[React Web SPA]
        Mobile[Mobile App]
    end
    
    subgraph "Backend Node.js"
        LB[Load Balancer]
        BFF[Orquestrador BFF<br/>Node.js + Fastify]
        Tools[Tool Executors<br/>Docker Containers]
    end
    
    subgraph "IA & Dados"
        Claude[Anthropic Claude]
        RAG[(RAG Database<br/>pgvector)]
        Embeddings[Embeddings Service]
    end
    
    VSCode --> LB
    WebSPA --> LB
    Mobile --> LB
    LB --> BFF
    BFF --> Claude
    BFF --> Tools
    BFF --> RAG
```

### 🔵 ASP.NET Core + C# (Alternativo)
```mermaid
graph TB
    subgraph "Interface Web"
        RazorPages[Razor Pages + i18n]
        SignalRHub[SignalR Hub]
    end
    
    subgraph "Backend .NET"
        Controllers[API Controllers]
        Services[Business Services]
        EF[Entity Framework]
    end
    
    subgraph "Banco Compartilhado"
        PostgreSQL[(PostgreSQL + pgvector)]
        Redis[(Redis Cache)]
    end
    
    RazorPages --> Controllers
    SignalRHub --> Services
    Controllers --> Services
    Services --> EF
    EF --> PostgreSQL
    Services --> Redis
```

## 🚀 Quick Start

Escolha uma das implementações de backend:

### 🟢 Opção 1: Node.js Backend (Recomendado)

```bash
# 1. Clone o repositório
git clone https://github.com/org/tutor-copiloto.git
cd tutor-copiloto

# 2. Instalar dependências
npm install

# 3. Configurar ambiente
cp .env.example .env
# Editar .env com:
# - ANTHROPIC_API_KEY (obrigatório)
# - LLAMAINDEX_API_KEY (obrigatório) 
# - REDIS_URL (se usando Redis Cloud)

# 4. Iniciar serviços
docker-compose up -d

# 5. Desenvolver
npm run dev
```

**Acesso:** 
- Web Interface: http://localhost:3000
- API Docs: http://localhost:8080/docs
- VS Code Extension: Press F5

### 🔵 Opção 2: ASP.NET Core Backend

```bash
# 1. Navegar para backend .NET
cd dotnet-backend

# 2. Restaurar dependências
dotnet restore

# 3. Configurar banco e Redis
export POSTGRES_URL="Host=localhost;Database=tutordb;Username=tutor;Password=copiloto123"
export REDIS_URL="redis://localhost:6379"

# 4. Executar
dotnet run
```

**Acesso:**
- Interface Web: http://localhost:5000
- API Swagger: http://localhost:5000/swagger
- SignalR Hub: ws://localhost:5000/chathub

## 💡 Funcionalidades

### 🟢 Node.js Backend
- **Tool Use Avançado**: Execução segura de código em containers
- **Streaming IA**: Respostas em tempo real com Claude
- **RAG Inteligente**: Base de conhecimento com LlamaIndex
- **VS Code Extension**: Integração nativa com editor
- **Web SPA**: Interface React moderna

### 🔵 ASP.NET Core Backend  
- **SignalR**: Chat tempo real e pair programming
- **Razor Pages**: Interface web com internacionalização
- **Entity Framework**: ORM robusto com PostgreSQL
- **Relatórios**: Análises educacionais detalhadas
- **API REST**: Endpoints Swagger documentados

### Para Estudantes
- **Explain Code**: Selecione código → explicação pedagógica detalhada
- **Write Tests**: Geração automática de testes com execução
- **Fix & Refactor**: Sugestões de melhorias com diff visual
- **Assessment**: Avaliação automática com feedback construtivo
- **Interactive Chat**: Conversas educacionais com contexto do projeto

### Para Educadores
- **Rubric-based Assessment**: Avaliação baseada em critérios personalizados
- **Progress Tracking**: Acompanhamento de evolução dos alunos
- **Custom Scenarios**: Criação de exercícios práticos
- **Anti-Cheating**: Políticas que incentivam aprendizado, não cola
- **Analytics Dashboard**: Métricas pedagógicas detalhadas

### Para Desenvolvedores
- **Code Review**: Análise automatizada com foco em qualidade
- **Documentation**: Geração de documentação contextual
- **Bug Detection**: Identificação proativa de problemas
- **Architecture Guidance**: Mentoria em decisões técnicas
- **Performance Analysis**: Sugestões de otimização

## 🛠️ Comandos Disponíveis

### VS Code Extension
| Comando | Atalho | Descrição |
|---------|--------|-----------|
| Explicar Código | `Ctrl+Shift+E` | Explica trecho selecionado |
| Gerar Testes | - | Cria testes para código selecionado |
| Corrigir Código | - | Sugere melhorias e correções |
| Avaliar Código | - | Avaliação pedagógica completa |
| Abrir Chat | `Ctrl+Shift+T` | Interface de chat interativo |

### API Endpoints
```typescript
POST /api/chat              // Chat streaming com tool use
GET  /api/conversations     // Histórico de conversas
POST /api/assess           // Avaliação de código
POST /api/tools/run-tests  // Execução de testes
GET  /api/health           // Health check
```

## 🔧 Configuração

### Variáveis de Ambiente
```env
# Essenciais
ANTHROPIC_API_KEY=sk-ant-xxxx
POSTGRES_URL=postgresql://user:pass@localhost:5432/tutordb
REDIS_URL=redis://localhost:6379

# Opcionais
LOG_LEVEL=info
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317
COST_MODE=deep-analysis
USER_LEVEL=intermediate
PERSONA=tutor
```

### Configuração da Extensão VS Code
```json
{
  "tutorCopiloto.apiUrl": "http://localhost:8080",
  "tutorCopiloto.userLevel": "intermediate",
  "tutorCopiloto.persona": "tutor",
  "tutorCopiloto.costMode": "deep-analysis",
  "tutorCopiloto.autoConnect": true
}
```

## 📊 Monitoramento

### Métricas Principais
- **Request Rate**: Requisições por segundo
- **Latency**: P95/P99 de tempo de resposta  
- **Cost Tracking**: Custo por usuário/sessão
- **Educational**: Taxa de conclusão, tempo de aprendizado
- **Tool Success**: Taxa de sucesso das ferramentas

### Dashboards Incluídos
- 📈 **Performance**: Latência e throughput
- 💰 **Cost Analysis**: Custos por modelo e usuário
- 🎓 **Educational**: Métricas pedagógicas
- 🔧 **Infrastructure**: Recursos e saúde do sistema

## 🚢 Deploy

### Kubernetes (Recomendado)
```bash
# Build e push das imagens
npm run build:docker
npm run push:registry

# Deploy no cluster
kubectl apply -f infra/deployments/kubernetes/

# Verificar status
kubectl get pods -n tutor-copiloto
```

### Docker Compose (Desenvolvimento)
```bash
docker-compose -f docker-compose.prod.yml up -d
```

Veja [docs/deployment.md](docs/deployment.md) para instruções detalhadas.

## 🧪 Testes

```bash
# Testes unitários
npm run test

# Testes de integração
npm run test:integration

# Testes E2E pedagógicos
npm run test:e2e

# Coverage
npm run test:coverage
```

## 🤝 Contribuindo

1. Fork o projeto
2. Crie uma branch (`git checkout -b feature/nova-funcionalidade`)
3. Commit suas mudanças (`git commit -m 'Adiciona nova funcionalidade'`)
4. Push para a branch (`git push origin feature/nova-funcionalidade`)
5. Abra um Pull Request

Veja [CONTRIBUTING.md](CONTRIBUTING.md) para diretrizes detalhadas.

## 📄 Licença

Este projeto está licenciado sob a MIT License - veja [LICENSE](LICENSE) para detalhes.

## 🙋‍♂️ Suporte

- 📧 **Email**: suporte@tutor-copiloto.com
- 💬 **Discord**: [tutor-copiloto](https://discord.gg/tutor-copiloto)
- 📚 **Docs**: [docs.tutor-copiloto.com](https://docs.tutor-copiloto.com)
- 🐛 **Issues**: [GitHub Issues](https://github.com/org/tutor-copiloto/issues)

## 🗺️ Roadmap

- [ ] **Q4 2025**: Integração com mais LLMs (OpenAI, Gemini)
- [ ] **Q1 2026**: Mobile app (Flutter)
- [ ] **Q2 2026**: Integração com LMS (Moodle, Canvas)
- [ ] **Q3 2026**: Multi-tenant architecture
- [ ] **Q4 2026**: Advanced analytics e ML insights

---

<div align="center">

**Feito com ❤️ para a comunidade educacional**

[⭐ Star no GitHub](https://github.com/org/tutor-copiloto) • [🐦 Twitter](https://twitter.com/tutorcopiloto) • [📱 LinkedIn](https://linkedin.com/company/tutor-copiloto)

</div>