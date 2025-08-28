#!/bin/bash
# Script de inicialização da integração GitHub Chat MCP

set -e

echo "🚀 Iniciando Tutor Copiloto - GitHub Chat MCP Integration..."

# Verificar se o Python está instalado
if ! command -v python3 &> /dev/null; then
    echo "❌ Python 3 não encontrado. Instale o Python 3.12 ou superior."
    exit 1
fi

# Verificar se o uv está instalado
if ! command -v uv &> /dev/null; then
    echo "📦 Instalando uv (gerenciador de pacotes Python)..."
    curl -LsSf https://astral.sh/uv/install.sh | sh
    export PATH="$HOME/.cargo/bin:$PATH"
fi

# Instalar dependências
echo "📦 Instalando dependências..."
cd /workspaces/ASP.NETCore/python-integration
uv sync

# Verificar se a chave da API do GitHub está configurada
if [ -z "$GITHUB_API_KEY" ]; then
    echo "⚠️  GITHUB_API_KEY não configurada. Algumas funcionalidades podem não funcionar."
    echo "   Configure a variável de ambiente GITHUB_API_KEY com sua chave da API GitHub."
fi

# Iniciar o servidor
echo "🌐 Iniciando servidor FastAPI na porta 8001..."
uv run python -m tutor_copiloto_github.main
