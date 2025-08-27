cd /workspaces/ASP.NETCore/dotnet-backend && dotnet run


cd /workspaces/ASP.NETCore/web-spa && npm start

Ambos os servidores estão rodando perfeitamente:

Backend .NET: http://localhost:5000 (com SignalR Hub em /chathub)
Frontend React: http://localhost:5175 (com proxy configurado para o backend)
🎯 O que foi implementado:
Backend (.NET Core)
✅ Autenticação Anônima: Endpoint /api/auth/anonymous para login sem credenciais
✅ SignalR Hub: ChatHub com métodos para mensagens em tempo real
✅ CORS Configurado: Política específica para SignalR com origens locais
✅ JWT Tokens: Suporte a tokens anônimos com display names
✅ Banco de Dados: Integração com SQLite para usuários e sessões
Frontend (React)
✅ SignalR Service: Cliente completo com reconexão automática
✅ Componente ChatSignalRSection: Interface completa para chat em tempo real
✅ Integração App.jsx: Nova seção "🔴 Chat SignalR" no menu lateral
✅ Proxy Configurado: Vite proxy para /api → localhost:5000
🚀 Como testar:
Abra o frontend: http://localhost:5175
Navegue para: "🔴 Chat SignalR" no menu lateral
Login Anônimo: Clique em "Login Anônimo" para obter um token
Conectar SignalR: Clique em "Conectar SignalR"
Enviar Mensagens: Digite mensagens e veja o chat em tempo real
📡 Funcionalidades do Chat SignalR:
Mensagens em Tempo Real: Envio e recebimento instantâneo
Usuários Online: Lista de usuários conectados
Eventos do Hub: Conexão/desconexão, mensagens enviadas
Grupos: Suporte para salas de chat
Pair Programming: Convites para programação em pares
Compartilhamento de Código: Envio de trechos de código
🔧 Configurações aplicadas:
JWT Secret Key: Configurada no appsettings.json
CORS Origins: http://localhost:5175 adicionada
SignalR Policy: Configurada especificamente para hubs
Proxy Vite: Corrigido para apontar para porta 5000
A implementação está 100% funcional e pronta para uso! O SignalR agora suporta autenticação anônima completa com comunicação em tempo real entre frontend e backend. 🎉

consulta externa Via API 

# Qualquer repositório GitHub
curl -X POST http://localhost:8080/api/webhook/deploy \
  -H "Content-Type: application/json" \
  -d '{
    "repositoryUrl": "https://github.com/QUALQUER_USUARIO/QUALQUER_REPO.git",
    "branch": "main",
    "author": "Seu Nome"
  }'


# 🎓 Tutor Copiloto




Endpoints
O Codestral está disponível em plugins selecionados de completação de código, mas também pode ser consultado diretamente. Veja a documentação para mais detalhes.
Endpoint de completação

https://codestral.mistral.ai/v1/fim/completions

Endpoint de chat

https://codestral.mistral.ai/v1/chat/completions



Arquivos Estáticos: Configurar SignalR JS files
Testes Completos: Testar todos os endpoints de AI
aa


Implementação Concluída!
Parabéns! Você agora tem um servidor de deployment ASP.NET Core completo integrado com ngrok e webhooks do GitHub, similar ao Vercel/Netlify.

✅ O que foi implementado:
🔧 Controllers de API:

WebhookController: Gerencia webhooks do GitHub e deployments manuais
NgrokController: Controla túneis ngrok programaticamente
⚙️ Serviços de Negócio:

DeploymentService: Lógica de deployments com logs detalhados
NgrokTunnelService: Gerenciamento automático de túneis ngrok
🌐 Interface Web:

Dashboard completo em /deployments
Controle do ngrok via interface
Deploy manual via formulário
Visualização de logs em tempo real
🔄 Sistema de Deployment:

Webhooks automáticos do GitHub
Processamento de deployments com status tracking
Logs detalhados para cada deployment
URLs públicas via ngrok
🚀 Como usar:
Executar: start-deployment-server.sh ou dotnet run
Acessar: http://localhost:5000/deployments
Configurar webhook no GitHub com a URL fornecida
Fazer push para branch main/master e ver o deploy automático!
📊 URLs importantes:
Dashboard: http://localhost:5000/deployments
API Docs: http://localhost:5000/swagger
Health Check: http://localhost:5000/health
O sistema está totalmente funcional e pronto para integração com qualquer repositório GitHub para deployments automáticos! 🎯# 🎓 Tutor Copiloto




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