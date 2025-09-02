# 🚀 Tutor Copiloto - Railway Deployment

Este guia explica como fazer o deploy completo do Tutor Copiloto no Railway para ter sua aplicação rodando 100% gratuitamente online.

## 📋 Pré-requisitos

- Conta no [Railway.app](https://railway.app) (gratuita)
- Chaves de API para os serviços de IA (OpenAI, Anthropic, etc.)
- Token do GitHub (opcional, para integração)

## 🏗️ Arquitetura no Railway

```
Railway Project
├── API Backend (ASP.NET Core)
├── PostgreSQL Database
├── Redis Cache
└── Custom Domain (opcional)
```

## 🚀 Deploy Automático

### 1. Preparar o Projeto

```bash
# 1. Clonar ou ter o projeto pronto
cd /workspaces/ASP.NETCore

# 2. Executar o script de deploy
chmod +x deploy-railway.sh
./deploy-railway.sh
```

### 2. Configurar Variáveis de Ambiente

No painel do Railway, configure estas variáveis:

#### Obrigatórias
```bash
JWT_SECRET_KEY=your-super-secret-jwt-key-here
OPENAI_API_KEY=your-openai-api-key
```

#### Opcionais
```bash
ANTHROPIC_API_KEY=your-anthropic-key
GITHUB_API_KEY=your-github-token
SMTP_USERNAME=your-email@gmail.com
SMTP_PASSWORD=your-app-password
```

## 🔧 Configuração Manual (Alternativa)

### 1. Criar Projeto no Railway

1. Acesse [Railway.app](https://railway.app)
2. Clique em "New Project"
3. Selecione "Deploy from GitHub repo"
4. Conecte seu repositório

### 2. Adicionar Serviços

1. **PostgreSQL**: Add → Database → PostgreSQL
2. **Redis**: Add → Database → Redis
3. **API**: Seu código ASP.NET Core será automaticamente detectado

### 3. Configurar Environment Variables

No painel da sua API service:

```
# JWT
JWT_SECRET_KEY=your-secret-key-here

# AI Services
OPENAI_API_KEY=sk-your-openai-key
ANTHROPIC_API_KEY=sk-ant-your-anthropic-key

# GitHub (opcional)
GITHUB_API_KEY=github_pat_your_token

# CORS
CORS_ALLOWED_ORIGINS=https://yourdomain.com
```

## 📊 Monitoramento

### Health Checks
- Endpoint: `https://your-app.railway.app/health`
- Verifica: API, Database, Redis, AI Services

### Logs
```bash
# Ver logs da aplicação
railway logs

# Ver logs de um serviço específico
railway logs --service api-service-name
```

### Métricas
Railway fornece métricas automáticas de:
- CPU Usage
- Memory Usage
- Network I/O
- Response Times

## 🔗 URLs dos Serviços

Após o deploy, você terá:

- **API**: `https://your-app.railway.app`
- **Swagger**: `https://your-app.railway.app/swagger`
- **Health Check**: `https://your-app.railway.app/health`

## 🌐 Custom Domain (Opcional)

1. Vá para Settings → Domains
2. Adicione seu domínio personalizado
3. Configure os registros DNS conforme instruído

## 💰 Custos

### Plano Gratuito (Railway)
- ✅ 512MB RAM
- ✅ PostgreSQL (até 512MB)
- ✅ Redis (até 100MB)
- ✅ 1GB bandwidth/mês
- ✅ Custom domain grátis

### Quando Pagar
- RAM adicional (>512MB)
- Storage adicional
- Bandwidth extra
- Múltiplas réplicas

## 🐛 Troubleshooting

### Problema: Build Falhando
```bash
# Ver logs detalhados
railway logs --build
```

### Problema: Database Connection
```bash
# Verificar variáveis de ambiente
railway variables

# Testar conexão
railway run dotnet ef database update
```

### Problema: Out of Memory
- Verifique se está usando o Dockerfile otimizado
- Considere aumentar para o plano Hobby ($5/mês)

## 🔄 Atualizações

### Deploy Automático
```bash
# Push para main/master dispara deploy automático
git add .
git commit -m "Update deployment"
git push origin main
```

### Rollback
```bash
# Ver deploys anteriores
railway deploys

# Fazer rollback
railway rollback <deploy-id>
```

## 📞 Suporte

- [Railway Docs](https://docs.railway.app/)
- [Discord Railway](https://discord.gg/railway)
- [GitHub Issues](https://github.com/railwayapp/cli/issues)

## 🎯 Próximos Passos

1. ✅ Deploy básico funcionando
2. 🔄 Configurar monitoring avançado
3. 🔄 Adicionar CDN para assets estáticos
4. 🔄 Configurar backup automático
5. 🔄 Otimizar performance

---

**🎉 Parabéns!** Sua aplicação Tutor Copiloto está rodando 100% gratuitamente no Railway!
