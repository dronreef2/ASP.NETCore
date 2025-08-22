# 🚀 ASP.NET Core Deployment Server
teste
Um mini servidor de deployment similar ao Vercel/Netlify, integrado com ngrok para exposição pública e webhooks do GitHub para deployments automáticos.

## 🌟 Funcionalidades

- **🔄 Deployments Automáticos**: Integração com webhooks do GitHub
- **🌐 Túnel Público**: Exposição automática via ngrok
- **📊 Dashboard Web**: Interface para gerenciar deployments
- **🔍 Logs em Tempo Real**: Acompanhamento detalhado dos deployments
- **🚀 Deploy Manual**: Possibilidade de fazer deployments manuais
- **📡 API REST**: Endpoints para integração externa

## 🛠️ Tecnologias Utilizadas

- **ASP.NET Core 8.0**: Framework principal
- **SignalR**: Comunicação em tempo real
- **ngrok**: Túneis públicos para webhooks
- **Bootstrap 5**: Interface responsiva
- **Entity Framework**: Persistência de dados
- **Serilog**: Logging estruturado

## 🚀 Início Rápido

### 1. Executar o Script de Inicialização

```bash
./scripts/start-deployment-server.sh
```

Este script irá:
- Verificar/instalar dependências (ngrok, .NET SDK)
- Compilar o projeto
- Iniciar a aplicação
- Configurar o túnel ngrok automaticamente

### 2. Acessar a Interface

Após inicializar, acesse:

- **Aplicação Principal**: http://localhost:5000
- **Dashboard de Deployments**: http://localhost:5000/deployments
- **API Documentation**: http://localhost:5000/swagger
- **Health Check**: http://localhost:5000/health

## � Configuração de Webhooks GitHub

### 1. Obter URL do Webhook

Acesse `/deployments` ou `/api/ngrok/webhook-url` para obter a URL pública do webhook.

### 2. Configurar no GitHub

1. Vá para `Settings > Webhooks` do seu repositório
2. Clique em `Add webhook`
3. Cole a URL do webhook (ex: `https://abc123.ngrok.app/api/webhook/github`)
4. Selecione `application/json` como Content type
5. Selecione eventos: `push`
6. Salve o webhook

### 3. Configurar Secret (Opcional)

Para maior segurança, adicione um secret no GitHub e configure em `appsettings.json`:

```json
{
  "GitHub": {
    "WebhookSecret": "seu-secret-aqui"
  }
}
```

## 🔧 Configuração

### appsettings.json

```json
{
  "Ngrok": {
    "AutoStart": true,
    "Port": 5000,
    "ConfigPath": "/workspaces/ASP.NETCore/ngrok.yml",
    "EndpointName": "tutor-copiloto-aspnet"
  },
  "GitHub": {
    "WebhookSecret": "opcional-para-validacao"
  },
  "Deployment": {
    "BaseUrl": "https://deploy.tutorcopiloto.com"
  }
}
```

### ngrok.yml

```yaml
version: 3
agent:
  authtoken: seu-authtoken-aqui

endpoints:
  - name: tutor-copiloto-aspnet
    url: https://tutor-copiloto.ngrok.app
    upstream:
      url: 5000
      protocol: http1
```

## � API Endpoints

### Webhooks

- `POST /api/webhook/github` - Webhook do GitHub
- `POST /api/webhook/deploy` - Deploy manual

### Deployments

- `GET /api/webhook/deployments` - Lista deployments
- `GET /api/webhook/deployments/{id}` - Detalhes de um deployment
- `GET /api/webhook/deployments/{id}/logs` - Logs de um deployment

### Ngrok Management

- `GET /api/ngrok/status` - Status do túnel
- `POST /api/ngrok/start` - Iniciar túnel
- `POST /api/ngrok/stop` - Parar túnel
- `GET /api/ngrok/webhook-url` - URL do webhook

## � Fluxo de Deployment

1. **Trigger**: Push para branch `main`/`master` ou deploy manual
2. **Validação**: Verificação de assinatura (se configurada)
3. **Criação**: Novo deployment é criado com status `Pending`
4. **Processamento**: 
   - Clone do repositório
   - Análise do código
   - Instalação de dependências
   - Build da aplicação
   - Deploy para ambiente
5. **Finalização**: Status atualizado para `Success` ou `Failed`

## 📝 Logs e Monitoramento

### Logs da Aplicação

Os logs são salvos em:
- Console: Para desenvolvimento
- Arquivo: `logs/tutor-copiloto-{data}.txt`

### Logs de Deployment

Cada deployment possui logs detalhados acessíveis via:
- Interface web: `/deployments`
- API: `/api/webhook/deployments/{id}/logs`

## � Deploy em Produção

### Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY publish/ .
EXPOSE 80
ENTRYPOINT ["dotnet", "TutorCopiloto.dll"]
```

### Variáveis de Ambiente

```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:80
POSTGRES_URL=sua-conexao-postgres
REDIS_URL=sua-conexao-redis
NGROK_AUTHTOKEN=seu-authtoken
GITHUB_WEBHOOK_SECRET=seu-secret
```

## 🛡️ Segurança

- **Validação de Webhook**: Secret do GitHub para validar origem
- **HTTPS**: Túneis ngrok sempre usam HTTPS
- **Rate Limiting**: Implementado nos endpoints de webhook
- **Logging**: Todas as operações são logadas

## 🔧 Desenvolvimento

### Estrutura do Projeto

```
dotnet-backend/
├── Controllers/
│   ├── WebhookController.cs      # Webhooks e deployments
│   └── NgrokController.cs        # Gerenciamento ngrok
├── Services/
│   ├── DeploymentService.cs      # Lógica de deployment
│   └── NgrokTunnelService.cs     # Gerenciamento de túneis
├── Pages/
│   └── Deployments.cshtml        # Interface web
└── Program.cs                    # Configuração da aplicação
```

### Executar em Desenvolvimento

```bash
cd dotnet-backend
dotnet restore
dotnet run
```

## 📞 Suporte

Para dúvidas ou problemas:

1. Verifique os logs da aplicação
2. Teste a conectividade do ngrok
3. Valide a configuração do webhook no GitHub
4. Consulte a documentação da API em `/swagger`

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo LICENSE para mais detalhes.
