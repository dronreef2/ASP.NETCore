# 🚀 DEPLOY RÁPIDO - TUTOR COPILOTO NO RAILWAY

## ⚡ EXECUÇÃO RÁPIDA (5 minutos)

### 1. Preparar Ambiente
```bash
cd /workspaces/ASP.NETCore
chmod +x deploy-railway.sh
```

### 2. Login no Railway
```bash
railway login
```

### 3. Executar Deploy
```bash
./deploy-railway.sh
```

### 4. Configurar APIs (Opcional)
```bash
# OpenAI (recomendado)
railway variables set OPENAI_API_KEY="sk-your-key-here"

# Anthropic (opcional)
railway variables set ANTHROPIC_API_KEY="sk-ant-your-key-here"

# GitHub (opcional)
railway variables set GITHUB_API_KEY="ghp_your-token-here"
```

## ✅ RESULTADO ESPERADO

Após executar o script, você terá:

- 🌐 **URL**: `https://tutor-copiloto.railway.app`
- 🗄️ **PostgreSQL**: Conectado automaticamente
- 🔴 **Redis**: Conectado automaticamente
- 🔒 **SSL**: Configurado automaticamente
- 📊 **Monitoramento**: Ativo

## 🧪 TESTAR APLICAÇÃO

```bash
# Health Check
curl https://tutor-copiloto.railway.app/health

# Swagger UI
open https://tutor-copiloto.railway.app/swagger
```

## 📋 ARQUIVOS CRIADOS

- ✅ `Program.Railway.cs` - Configuração otimizada para Railway
- ✅ `appsettings.Railway.json` - Configurações específicas
- ✅ `Dockerfile.Railway` - Build otimizado
- ✅ `railway.toml` - Configurações do Railway
- ✅ `health-check.sh` - Script de verificação
- ✅ `RAILWAY_README.md` - Documentação completa
- ✅ `.env.example` - Exemplo de variáveis

## 💰 CUSTOS

- ✅ **100% GRATUITO** no plano Railway Starter
- ✅ 512MB RAM suficiente
- ✅ PostgreSQL + Redis incluídos
- ✅ Domínio personalizado grátis

## 🆘 PROBLEMAS COMUNS

### Erro: "Railway CLI not found"
```bash
npm install -g @railway/cli
```

### Erro: "Not logged in"
```bash
railway login
```

### Erro: "Build failed"
```bash
railway logs --build
```

## 📞 SUPORTE

- 📖 [Documentação Completa](RAILWAY_README.md)
- 🌐 [Railway Docs](https://docs.railway.app)
- 💬 [Discord Railway](https://discord.gg/railway)

---

**🎯 PRONTO!** Execute `./deploy-railway.sh` e tenha sua aplicação rodando em minutos!
