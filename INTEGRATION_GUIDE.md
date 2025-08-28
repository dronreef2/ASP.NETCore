# Tutor Copiloto - Integração Completa com GitHub Chat MCP

## Visão Geral

Esta integração combina o poder do **Tutor Copiloto** (sistema .NET de análise de código) com o **GitHub Chat MCP** (servidor MCP para análise inteligente de repositórios GitHub).

## Arquitetura da Integração

```
┌─────────────────────────────────┐    HTTP     ┌─────────────────────────────────┐
│                                 │             │                                 │
│   Tutor Copiloto (.NET Core)    │◄──────────►│  GitHub Chat MCP (Python)      │
│                                 │             │                                 │
│  • Análise de estrutura         │             │  • Indexação inteligente       │
│  • Deployments                  │             │  • Consultas contextuais      │
│  • Relatórios                   │             │  • Análise de arquitetura     │
│  • API REST                     │             │  • Suporte a conversas       │
│                                 │             │                                 │
└─────────────────────────────────┘             └─────────────────────────────────┘
              │                                           │
              │                                           │
              ▼                                           ▼
┌─────────────────────────────────┐             ┌─────────────────────────────────┐
│                                 │             │                                 │
│  PostgreSQL / SQLite           │             │  GitHub Chat API              │
│  (Dados persistentes)          │◄──────────►│  (Análise externa)            │
│                                 │             │                                 │
└─────────────────────────────────┘             └─────────────────────────────────┘
```

## Funcionalidades Integradas

### 🔍 **Análise Estrutural (.NET)**
- Contagem de arquivos e diretórios
- Detecção de linguagens de programação
- Análise de extensões de arquivo
- Estrutura de dependências

### 🧠 **Análise Inteligente (Python/MCP)**
- Indexação semântica de repositórios
- Perguntas contextuais sobre código
- Análise de arquitetura
- Detecção de padrões
- Suporte a conversas multi-turn

### 🚀 **Funcionalidades Combinadas**
- Análise completa em uma única chamada
- Relatórios unificados
- Cache inteligente
- API REST unificada

## Instalação e Configuração

### 1. Pré-requisitos

```bash
# Python 3.12+
# .NET 8.0+
# uv (gerenciador de pacotes Python)
# Git
```

### 2. Clonagem e Setup

```bash
# Clonar o repositório (se não tiver)
git clone https://github.com/seu-usuario/tutor-copiloto.git
cd tutor-copiloto

# Configurar a integração Python
cd python-integration
cp .env.example .env
# Edite o .env com suas configurações
```

### 3. Configuração das Chaves API

**Arquivo `.env` (python-integration):**
```env
GITHUB_API_KEY=sua_chave_github_aqui
MCP_SERVER_HOST=localhost
MCP_SERVER_PORT=8001
```

**Arquivo `appsettings.json` (.NET):**
```json
{
  "GitHub": {
    "Integration": {
      "BaseUrl": "http://localhost:8001",
      "TimeoutSeconds": 30
    }
  }
}
```

## Como Usar

### Opção 1: Execução Manual

```bash
# Terminal 1: Iniciar o servidor .NET
cd dotnet-backend
dotnet run

# Terminal 2: Iniciar a integração Python
cd ../python-integration
./run-integration.sh
```

### Opção 2: Usando Docker

```bash
# Na raiz do projeto
docker-compose -f python-integration/docker-compose.yml up --build
```

### Opção 3: Scripts Automáticos

```bash
# Setup completo
./scripts/dev-setup.sh

# Iniciar tudo
./scripts/start-integration.sh
```

## Endpoints da API

### Análise Completa (Recomendado)

```bash
curl -X POST http://localhost:5000/api/github-integration/analyze-and-query \
  -H "Content-Type: application/json" \
  -d '{
    "repoUrl": "https://github.com/microsoft/vscode",
    "questions": [
      "Qual é a arquitetura principal?",
      "Quais tecnologias são usadas?",
      "Como está organizado o projeto?"
    ]
  }'
```

### Indexação Individual

```bash
curl -X POST http://localhost:5000/api/github-integration/index \
  -H "Content-Type: application/json" \
  -d '{
    "repoUrl": "https://github.com/microsoft/vscode",
    "branch": "main"
  }'
```

### Consultas Individuais

```bash
curl -X POST http://localhost:5000/api/github-integration/query \
  -H "Content-Type: application/json" \
  -d '{
    "repoUrl": "https://github.com/microsoft/vscode",
    "question": "Como funciona o sistema de extensões?"
  }'
```

### Status da Integração

```bash
curl http://localhost:5000/api/github-integration/status
```

## Exemplos de Uso

### 1. Análise de um Framework Popular

```bash
curl -X POST http://localhost:5000/api/github-integration/analyze-and-query \
  -H "Content-Type: application/json" \
  -d '{
    "repoUrl": "https://github.com/facebook/react",
    "questions": [
      "Qual é a arquitetura do React?",
      "Como funciona o Virtual DOM?",
      "Quais são os principais hooks?"
    ]
  }'
```

### 2. Análise de um Projeto Empresarial

```bash
curl -X POST http://localhost:5000/api/github-integration/analyze-and-query \
  -H "Content-Type: application/json" \
  -d '{
    "repoUrl": "https://github.com/microsoft/TypeScript",
    "questions": [
      "Como funciona o compilador TypeScript?",
      "Qual é a estratégia de tipos?",
      "Como são implementadas as decorators?"
    ]
  }'
```

### 3. Conversa Interativa

```bash
# Primeira pergunta
curl -X POST http://localhost:5000/api/github-integration/query \
  -H "Content-Type: application/json" \
  -d '{
    "repoUrl": "https://github.com/AsyncFuncAI/deepwiki-open",
    "question": "Qual é o propósito deste projeto?"
  }'

# Seguimento com histórico
curl -X POST http://localhost:5000/api/github-integration/query \
  -H "Content-Type: application/json" \
  -d '{
    "repoUrl": "https://github.com/AsyncFuncAI/deepwiki-open",
    "question": "Como implementar uma funcionalidade similar?",
    "conversationHistory": [
      {"role": "user", "content": "Qual é o propósito deste projeto?"},
      {"role": "assistant", "content": "Este é um sistema de análise de documentação usando IA..."}
    ]
  }'
```

## Monitoramento e Debugging

### Logs

```bash
# Logs do .NET
tail -f dotnet-backend/logs/tutor-copiloto-.txt

# Logs do Python (no terminal onde está rodando)
# Aparecem automaticamente no console
```

### Health Checks

```bash
# .NET API
curl http://localhost:5000/health

# Integração Python
curl http://localhost:8001/health

# Status da integração
curl http://localhost:5000/api/github-integration/status
```

### Debugging com MCP Inspector

```bash
# Instalar MCP Inspector
npm install -g @modelcontextprotocol/inspector

# Executar inspector
npx @modelcontextprotocol/inspector uvx github-chat-mcp
```

## Configurações Avançadas

### Cache e Performance

```json
// appsettings.json
{
  "GitHub": {
    "Integration": {
      "BaseUrl": "http://localhost:8001",
      "TimeoutSeconds": 60,
      "RetryAttempts": 3,
      "CacheEnabled": true,
      "CacheTTLMinutes": 30
    }
  }
}
```

### Configuração do MCP Server

```env
# .env (python-integration)
GITHUB_API_KEY=your_key_here
MCP_SERVER_HOST=0.0.0.0
MCP_SERVER_PORT=8001
LOG_LEVEL=DEBUG
UV_CACHE_DIR=/tmp/uv-cache
```

## Troubleshooting

### Problemas Comuns

1. **Erro de conexão com GitHub API**
   ```bash
   # Verificar chave da API
   echo $GITHUB_API_KEY

   # Testar conectividade
   curl -H "Authorization: Bearer $GITHUB_API_KEY" https://api.github.com/user
   ```

2. **Timeout na indexação**
   ```json
   // Aumentar timeout no appsettings.json
   {
     "GitHub": {
       "Integration": {
         "TimeoutSeconds": 120
       }
     }
   }
   ```

3. **Erro de dependências Python**
   ```bash
   cd python-integration
   rm -rf .venv
   uv sync --reinstall
   ```

4. **Portas ocupadas**
   ```bash
   # Verificar portas
   lsof -i :5000
   lsof -i :8001

   # Mudar portas se necessário
   export MCP_SERVER_PORT=8002
   ```

### Suporte

Para problemas específicos:

1. **Verificar logs detalhados**
2. **Testar endpoints individualmente**
3. **Usar MCP Inspector para debugging**
4. **Verificar configurações de rede/firewall**

## Contribuição

Para contribuir com melhorias na integração:

1. Fork o repositório
2. Crie uma branch para sua feature
3. Implemente as mudanças
4. Adicione testes
5. Submeta um Pull Request

## Licença

Esta integração é parte do projeto Tutor Copiloto e segue a mesma licença MIT.
