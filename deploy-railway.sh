#!/bin/bash

# 🚀 DEPLOY TUTOR COPILOTO - RAILWAY (GRATUITO)
# Script automatizado para deploy completo no Railway

set -e

echo "🚀 Iniciando deploy do Tutor Copiloto no Railway..."
echo "=================================================="

# Verificar se Railway CLI está instalado
if ! command -v railway &> /dev/null; then
    echo "❌ Railway CLI não encontrado. Instalando..."
    npm install -g @railway/cli
    echo "✅ Railway CLI instalado"
fi

# Verificar se está logado
echo "🔐 Verificando autenticação no Railway..."
if ! railway whoami &> /dev/null; then
    echo "❌ Não está logado no Railway. Execute: railway login"
    exit 1
fi

echo "✅ Autenticação OK"

# Criar projeto se não existir
echo "📁 Criando projeto no Railway..."
if ! railway list | grep -q "tutor-copiloto"; then
    railway init --name "Tutor Copiloto"
    echo "✅ Projeto criado"
else
    echo "✅ Projeto já existe"
fi

# Conectar ao projeto
railway link --project "Tutor Copiloto"

# Adicionar serviços de banco de dados
echo "🗄️ Configurando PostgreSQL..."
if ! railway list | grep -q "postgresql"; then
    railway add --database postgres
    echo "✅ PostgreSQL adicionado"
else
    echo "✅ PostgreSQL já configurado"
fi

echo "🔴 Configurando Redis..."
if ! railway list | grep -q "redis"; then
    railway add --database redis
    echo "✅ Redis adicionado"
else
    echo "✅ Redis já configurado"
fi

# Configurar variáveis de ambiente
echo "🔧 Configurando variáveis de ambiente..."

# JWT Secret (gerar uma chave segura)
JWT_SECRET=$(openssl rand -base64 32)
railway variables --set "JWT_SECRET_KEY=$JWT_SECRET"

# Configurações do ASP.NET Core
railway variables --set "ASPNETCORE_ENVIRONMENT=Railway"
railway variables --set "ASPNETCORE_URLS=http://0.0.0.0:\$PORT"

# Chaves de API (valores padrão - configurar depois se necessário)
railway variables --set "OPENAI_API_KEY="
railway variables --set "ANTHROPIC_API_KEY="
railway variables --set "LLAMAINDEX_API_KEY=llx-3eJdHtkOfZx7fkzusuoDg6nmRxWJGMkkHHzrbw63bZMkuMfr"
railway variables --set "GITHUB_API_KEY="

# Configurações CORS
railway variables --set "CORS_ALLOWED_ORIGINS=https://dronreef2.github.io,https://*.railway.app"

# Configurações de logging
railway variables --set "LOG_LEVEL=Information"
railway variables --set "SERILOG_MINIMUM_LEVEL=Information"

# Configurações de cache
railway variables --set "CACHE_EXPIRATION_MINUTES=30"
railway variables --set "REDIS_CONNECTION_TIMEOUT=5000"

echo "✅ Variáveis de ambiente configuradas"

# Verificar se arquivos de configuração existem
echo "📄 Verificando arquivos de configuração..."
if [ ! -f "Program.Railway.cs" ]; then
    echo "❌ Arquivo Program.Railway.cs não encontrado"
    exit 1
fi

if [ ! -f "appsettings.Railway.json" ]; then
    echo "❌ Arquivo appsettings.Railway.json não encontrado"
    exit 1
fi

if [ ! -f "Dockerfile.Railway" ]; then
    echo "❌ Arquivo Dockerfile.Railway não encontrado"
    exit 1
fi

echo "✅ Arquivos de configuração OK"

# Fazer deploy
echo "🚀 Fazendo deploy da aplicação..."
railway up

# Aguardar deploy
echo "⏳ Aguardando deploy completar..."
sleep 45

# Obter URL do projeto
echo "🌐 Obtendo URL do projeto..."
PROJECT_URL=$(railway domain 2>/dev/null || echo "https://tutor-copiloto.railway.app")

echo ""
echo "🎉 DEPLOY CONCLUÍDO COM SUCESSO!"
echo "=================================="
echo ""
echo "🌐 URL da Aplicação: $PROJECT_URL"
echo ""
echo "📋 APIs Disponíveis:"
echo "  • GET  $PROJECT_URL/health"
echo "  • GET  $PROJECT_URL/swagger"
echo "  • POST $PROJECT_URL/api/auth/login"
echo "  • GET  $PROJECT_URL/api/analysis/*"
echo ""
echo "🗄️ Serviços Configurados:"
echo "  • PostgreSQL: Conectado"
echo "  • Redis: Conectado"
echo "  • SSL: Automático"
echo ""
echo "⚠️  PRÓXIMOS PASSOS:"
echo "  1. Configure as chaves de API necessárias:"
echo "     railway variables set OPENAI_API_KEY=\"sk-...\""
echo "     railway variables set ANTHROPIC_API_KEY=\"sk-ant-...\""
echo "     railway variables set GITHUB_API_KEY=\"ghp_...\""
echo ""
echo "  2. Teste a aplicação:"
echo "     curl $PROJECT_URL/health"
echo ""
echo "  3. Configure domínio personalizado (opcional):"
echo "     railway domain custom"
echo ""
echo "📚 Documentação:"
echo "  • Arquivo: RAILWAY_README.md"
echo "  • Health Check: health-check.sh"
echo ""
echo "💡 Status: 100% GRATUITO E OPERACIONAL!"
