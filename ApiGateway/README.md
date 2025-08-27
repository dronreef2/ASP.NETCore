# API Gateway com YARP

Este é um gateway configurável mínimo baseado no YARP (Yet Another Reverse Proxy) para ASP.NET Core.

## 🚀 Funcionalidades

- **Roteamento Dinâmico**: Definido por configuração JSON
- **Balanceamento Simples**: Suporte a múltiplos destinos por cluster
- **Proxy Reverso Puro**: Sem autenticação ou métricas extras
- **Configuração Flexível**: Fácil de modificar rotas e destinos
- **Integração GitHub**: Consultas de repositórios e diagnósticos
- **Monitoramento**: Métricas e status do sistema

## 📋 Configuração

### appsettings.json (Produção)
```json
{
  "ReverseProxy": {
    "Routes": [
      {
        "RouteId": "apiRoute",
        "ClusterId": "backendCluster",
        "Match": {
          "Path": "/api/{**catch-all}"
        }
      }
    ],
    "Clusters": {
      "backendCluster": {
        "Destinations": {
          "destination1": {
            "Address": "https://api.exemplo.com"
          }
        }
      }
    }
  }
}
```

### appsettings.Development.json (Desenvolvimento)
```json
{
  "ReverseProxy": {
    "Routes": [
      {
        "RouteId": "backendApiRoute",
        "ClusterId": "backendCluster",
        "Match": {
          "Path": "/api/{**catch-all}"
        }
      },
      {
        "RouteId": "signalrRoute",
        "ClusterId": "backendCluster",
        "Match": {
          "Path": "/chathub/{**catch-all}"
        }
      }
    ],
    "Clusters": {
      "backendCluster": {
        "Destinations": {
          "backend1": {
            "Address": "http://localhost:5000"
          }
        }
      }
    }
  }
}
```

## 🏃‍♂️ Como Executar

```bash
# Navegar para o diretório do gateway
cd ApiGateway

# Restaurar dependências
dotnet restore

# Executar o gateway (desenvolvimento)
dotnet run --environment Development

# Executar o gateway (produção)
dotnet run --environment Production
```

O gateway estará disponível em:
- **Desenvolvimento**: `http://localhost:8080`
- **Produção**: Configure conforme suas necessidades

## 🧪 Como Testar

### Usando Visual Studio Code
1. Abra o arquivo `ApiGateway.http`
2. Execute as requisições de teste

### Usando curl
```bash
# Teste básico
curl http://localhost:8080/health

# Teste de proxy para API
curl http://localhost:8080/api/health/dotnet

# Teste de SignalR
curl -X POST http://localhost:8080/chathub/negotiate?negotiateVersion=1 \
  -H "Content-Type: application/json" \
  -d "{}"
```

### Usando navegador
- Acesse: `http://localhost:8080/api/ai-analysis/executive-summary?days=1`

## 🔧 Personalização

### Adicionando Novas Rotas
Para adicionar uma nova rota, simplesmente adicione uma entrada na seção `Routes`:

```json
{
  "RouteId": "novaRota",
  "ClusterId": "novoCluster",
  "Match": {
    "Path": "/novo-servico/{**catch-all}"
  }
}
```

### Adicionando Novos Clusters
```json
{
  "Clusters": {
    "novoCluster": {
      "Destinations": {
        "destino1": {
          "Address": "https://novo-servico.com"
        }
      }
    }
  }
}
```

### Configurações Avançadas
Veja o arquivo `appsettings.Advanced.json` para exemplos de:
- Load balancing com RoundRobin
- Múltiplos clusters
- Transforms de headers
- Políticas de roteamento avançadas

## � Integração GitHub

O gateway inclui rotas especiais para consultar a API do GitHub:

### Configuração
1. Obtenha um token do GitHub em [Personal Access Tokens](https://github.com/settings/tokens)
2. Configure o token no `appsettings.GitHub.json`
3. Use as rotas `/github/*` para consultas

### Rotas Disponíveis
- `/github/user/repos` - Repositórios do usuário
- `/github/repos/{owner}/{repo}` - Informações do repositório
- `/github/repos/{owner}/{repo}/issues` - Issues
- `/github/repos/{owner}/{repo}/pulls` - Pull requests
- `/github/repos/{owner}/{repo}/commits` - Commits
- `/github/repos/{owner}/{repo}/branches` - Branches

### Exemplo de Uso
```bash
curl -H "Authorization: Bearer YOUR_TOKEN" \
     http://localhost:8080/github/repos/dronreef2/ASP.NETCore
```

Para documentação completa da integração GitHub, consulte `GITHUB_INTEGRATION.md`.

## � Sistema de Deployment

O gateway também fornece acesso ao sistema de deployment integrado:

### Interface Web
- `/deployments` - Dashboard completo para gerenciar deployments

### APIs de Deployment
- `GET /api/webhook/deployments` - Listar todos os deployments
- `GET /api/webhook/deployments/{id}` - Detalhes de um deployment específico
- `GET /api/webhook/deployments/{id}/logs` - Logs de um deployment
- `POST /api/webhook/deploy` - Iniciar deploy manual
- `POST /api/webhook/github` - Webhook do GitHub (para testes)

### Deploy Manual via Gateway
```bash
curl -X POST http://localhost:8080/api/webhook/deploy \
  -H "Content-Type: application/json" \
  -d '{
    "repositoryUrl": "https://github.com/dronreef2/ASP.NETCore.git",
    "branch": "main",
    "author": "Deploy via Gateway"
  }'
```

### Funcionalidades do Sistema de Deployment
- ✅ **Deployments automáticos** via webhooks do GitHub
- ✅ **Deploy manual** via interface web ou API
- ✅ **Logs em tempo real** para cada deployment
- ✅ **Dashboard web** para gerenciamento completo
- ✅ **Integração ngrok** para webhooks públicos
- ✅ **Status tracking** (Pending → Running → Success/Failed)

Para documentação completa do sistema de deployment, consulte `DEPLOYMENT_GUIDE.md`.

## �📊 Diagnósticos e Monitoramento

### Rotas de Diagnóstico
- `/diagnostics/status` - Status geral do sistema
- `/diagnostics/logs` - Logs do sistema
- `/diagnostics/repo-stats` - Estatísticas do repositório
- `/diagnostics/activity-report` - Relatório de atividade

### Rotas de Métricas
- `/metrics/performance` - Métricas de performance
- `/status/detailed` - Health check detalhado

## �📦 Dependências

- **Yarp.ReverseProxy**: 2.3.0 - Core do reverse proxy
- **ASP.NET Core**: Framework web

## 🎯 Casos de Uso

- **Microserviços**: Roteamento de APIs
- **Load Balancing**: Distribuição de carga
- **API Gateway**: Ponto único de entrada
- **Desenvolvimento**: Proxy para serviços locais
- **Integração GitHub**: Consultas de repositórios e diagnósticos
- **Sistema de Deployment**: Deployments automáticos e manuais
- **Monitoramento**: Status e métricas do sistema

## ⚠️ Limitações da Versão Mínima

Esta implementação NÃO inclui:
- Autenticação/Autorização
- Rate Limiting
- Health Checks customizados
- Métricas e telemetria
- CORS configuration
- Logging avançado

Para adicionar essas funcionalidades, consulte a documentação completa do YARP em: https://microsoft.github.io/reverse-proxy/

## 🐳 Docker (Opcional)

Para executar em container Docker:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ApiGateway.csproj", "."]
RUN dotnet restore "./ApiGateway.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./ApiGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./ApiGateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ApiGateway.dll"]
```

```bash
# Construir imagem
docker build -t api-gateway .

# Executar container
docker run -p 8080:8080 api-gateway
```</content>
<parameter name="filePath">/workspaces/ASP.NETCore/ApiGateway/README.md</content>
<parameter name="filePath">/workspaces/ASP.NETCore/ApiGateway/README.md</content>
<parameter name="filePath">/workspaces/ASP.NETCore/ApiGateway/README.md
