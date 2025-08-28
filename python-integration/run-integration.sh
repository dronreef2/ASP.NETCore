#!/bin/bash
# Script completo para executar a integração GitHub Chat MCP

set -e

echo "🚀 Tutor Copiloto - GitHub Chat MCP Integration Setup"
echo "=================================================="

# Verificar se estamos no diretório correto
if [ ! -f "pyproject.toml" ]; then
    echo "❌ Execute este script do diretório python-integration"
    exit 1
fi

# Verificar se o uv está instalado
if ! command -v uv &> /dev/null; then
    echo "📦 Instalando uv (gerenciador de pacotes Python)..."
    curl -LsSf https://astral.sh/uv/install.sh | sh
    export PATH="$HOME/.cargo/bin:$PATH"
fi

# Instalar dependências
echo "📦 Instalando dependências Python..."
uv sync

# Verificar se o .env existe
if [ ! -f ".env" ]; then
    echo "📝 Criando arquivo .env a partir do exemplo..."
    cp .env.example .env
    echo "⚠️  Edite o arquivo .env com suas configurações antes de continuar"
    echo "   Principalmente a GITHUB_API_KEY se disponível"
fi

# Verificar se o servidor .NET está rodando
echo "🔍 Verificando se o servidor .NET está rodando..."
if curl -s http://localhost:5000/health > /dev/null; then
    echo "✅ Servidor .NET está rodando"
else
    echo "⚠️  Servidor .NET não está rodando. Inicie-o com:"
    echo "   cd ../dotnet-backend && dotnet run"
fi

# Iniciar o servidor de integração Python
echo "🌐 Iniciando servidor de integração Python..."
echo "📍 API disponível em: http://localhost:8001"
echo "📖 Documentação: http://localhost:8001/docs"
echo ""
echo "Para testar a integração:"
echo "curl http://localhost:8001/api/github/status"
echo ""
echo "Para testar com o .NET:"
echo "curl http://localhost:5000/api/github-integration/status"
echo ""

# Executar o servidor
uv run python -m tutor_copiloto_github.main
