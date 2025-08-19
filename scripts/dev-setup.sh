#!/bin/bash

# Script de desenvolvimento local para Tutor Copiloto
# Uso: ./scripts/dev-setup.sh

set -e

echo "🎓 Configurando ambiente de desenvolvimento do Tutor Copiloto..."

# Verificar Node.js
if ! command -v node &> /dev/null; then
    echo "❌ Node.js não encontrado. Instale o Node.js 18+ primeiro."
    exit 1
fi

NODE_VERSION=$(node --version | sed 's/v//')
REQUIRED_VERSION="18.0.0"

if ! printf '%s\n%s\n' "$REQUIRED_VERSION" "$NODE_VERSION" | sort -V -C; then
    echo "❌ Node.js versão $NODE_VERSION encontrada. Requer versão 18+"
    exit 1
fi

echo "✅ Node.js $NODE_VERSION OK"

# Verificar Docker
if ! command -v docker &> /dev/null; then
    echo "❌ Docker não encontrado. Instale o Docker primeiro."
    exit 1
fi

echo "✅ Docker OK"

# Verificar se Docker está rodando
if ! docker info &> /dev/null; then
    echo "❌ Docker não está rodando. Inicie o Docker primeiro."
    exit 1
fi

echo "✅ Docker rodando OK"

# Instalar dependências
echo ""
echo "📦 Instalando dependências..."
npm install

# Configurar .env se não existir
if [[ ! -f .env ]]; then
    echo ""
    echo "📝 Criando arquivo .env..."
    cp .env.example .env
    
    echo ""
    echo "⚠️ IMPORTANTE: Configure suas API keys no arquivo .env:"
    echo "   - ANTHROPIC_API_KEY"
    echo "   - LLAMAINDEX_API_KEY (chave fornecida: llx-3eJdHtkOfZx7fkzusuoDg6nmRxWJGMkkHHzrbw63bZMkuMfr)"
    echo ""
    
    # Configurar automaticamente a chave LlamaIndex
    if command -v sed &> /dev/null; then
        sed -i.bak 's/LLAMAINDEX_API_KEY=llx-your-llamaindex-key-here/LLAMAINDEX_API_KEY=llx-3eJdHtkOfZx7fkzusuoDg6nmRxWJGMkkHHzrbw63bZMkuMfr/' .env
        echo "✅ Chave LlamaIndex configurada automaticamente no .env"
    fi
    
    read -p "Pressione Enter para continuar após configurar as API keys..."
else
    echo "✅ Arquivo .env já existe"
fi

# Iniciar serviços de desenvolvimento
echo ""
echo "🚀 Iniciando serviços de desenvolvimento..."

# Parar serviços se já estiverem rodando
docker-compose down -v 2>/dev/null || true

# Iniciar serviços
docker-compose up -d

# Aguardar serviços iniciarem
echo "⏳ Aguardando serviços iniciarem..."
sleep 10

# Verificar saúde dos serviços
echo ""
echo "🔍 Verificando saúde dos serviços..."

services=("postgres:5432" "redis:6379")
for service in "${services[@]}"; do
    service_name=$(echo $service | cut -d: -f1)
    port=$(echo $service | cut -d: -f2)
    
    if nc -z localhost $port 2>/dev/null; then
        echo "✅ $service_name OK"
    else
        echo "❌ $service_name não está respondendo na porta $port"
    fi
done

# Build inicial
echo ""
echo "🔧 Build inicial..."
npm run build

echo ""
echo "🎉 Ambiente de desenvolvimento configurado com sucesso!"
echo ""
echo "Para iniciar o desenvolvimento:"
echo "  1. Terminal 1: npm run dev (todos os serviços)"
echo "  2. Ou separadamente:"
echo "     - Backend: cd backend/orchestrator && npm run dev"
echo "     - Web SPA: cd clients/web-spa && npm run dev"
echo "     - VS Code Ext: cd clients/vscode-ext && npm run watch"
echo ""
echo "URLs úteis:"
echo "  - Web Interface: http://localhost:3000"
echo "  - API Backend: http://localhost:8080"
echo "  - API Docs: http://localhost:8080/docs"
echo "  - Grafana: http://localhost:3001 (admin/admin123)"
echo "  - Prometheus: http://localhost:9090"
echo ""
echo "Para parar os serviços: docker-compose down"
