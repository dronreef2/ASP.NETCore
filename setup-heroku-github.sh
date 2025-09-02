#!/bin/bash

# Script de configuração do Deploy Automático GitHub Actions + Heroku
# Autor: GitHub Copilot
# Data: $(date)

set -e

echo "🚀 Configuração do Deploy Automático - Tutor Copiloto"
echo "======================================================"

# Verificar se heroku CLI está instalado
if ! command -v heroku &> /dev/null; then
    echo "❌ Heroku CLI não encontrado. Instale em: https://devcenter.heroku.com/articles/heroku-cli"
    exit 1
fi

# Verificar se está logado no Heroku
if ! heroku auth:whoami &> /dev/null; then
    echo "🔐 Faça login no Heroku:"
    heroku login
fi

# Solicitar nome do app
read -p "📝 Digite o nome do seu app no Heroku (ou pressione Enter para gerar automaticamente): " APP_NAME

if [ -z "$APP_NAME" ]; then
    APP_NAME="tutor-copiloto-$(date +%s)"
    echo "🔄 Nome gerado automaticamente: $APP_NAME"
fi

# Criar app no Heroku
echo "🏗️  Criando app no Heroku: $APP_NAME"
heroku create "$APP_NAME"

# Configurar stack para container
echo "🐳 Configurando stack para Docker..."
heroku stack:set container -a "$APP_NAME"

# Adicionar PostgreSQL
read -p "🗄️  Adicionar PostgreSQL? (y/n): " ADD_POSTGRES
if [[ $ADD_POSTGRES =~ ^[Yy]$ ]]; then
    echo "📦 Adicionando PostgreSQL..."
    heroku addons:create heroku-postgresql:hobby-dev -a "$APP_NAME"
fi

# Obter API Key
echo "🔑 Obtendo API Key do Heroku..."
HEROKU_API_KEY=$(heroku auth:token)

echo ""
echo "✅ Configuração do Heroku concluída!"
echo "====================================="
echo "📋 App Name: $APP_NAME"
echo "🔗 URL: https://$APP_NAME.herokuapp.com"
echo ""
echo "📝 Agora configure os secrets no GitHub:"
echo "=========================================="
echo "1. Acesse: https://github.com/$(git config --get remote.origin.url | sed 's/.*github.com[:/]\([^.]*\).*/\1/')/settings/secrets/actions/new"
echo "2. Adicione os seguintes secrets:"
echo "   - HEROKU_API_KEY: $HEROKU_API_KEY"
echo "   - HEROKU_APP_NAME: $APP_NAME"
echo ""
echo "🎉 Pronto! O próximo push na branch main fará o deploy automático!"
echo ""
echo "📊 Para verificar o deploy:"
echo "heroku logs --tail -a $APP_NAME"
