#!/bin/bash

# Script para inicializar o servidor de deployment ASP.NET Core com ngrok

echo "🚀 Inicializando Tutor Copiloto - Servidor de Deployment"
echo "=================================================="

# Verifica se o ngrok está instalado
if ! command -v ngrok &> /dev/null; then
    echo "❌ ngrok não encontrado. Instalando..."
    
    # Detecta arquitetura
    ARCH=$(uname -m)
    if [[ "$ARCH" == "x86_64" ]]; then
        ARCH="amd64"
    elif [[ "$ARCH" == "aarch64" ]] || [[ "$ARCH" == "arm64" ]]; then
        ARCH="arm64"
    else
        echo "❌ Arquitetura não suportada: $ARCH"
        exit 1
    fi
    
    # Download e instalação do ngrok
    wget -q https://bin.equinox.io/c/bNyj1mQVY4c/ngrok-v3-stable-linux-${ARCH}.tgz
    tar xzf ngrok-v3-stable-linux-${ARCH}.tgz
    sudo mv ngrok /usr/local/bin/
    rm ngrok-v3-stable-linux-${ARCH}.tgz
    
    echo "✅ ngrok instalado com sucesso"
else
    echo "✅ ngrok já está instalado"
fi

# Verifica se o .NET está instalado
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK não encontrado. Por favor, instale o .NET 8.0 SDK"
    exit 1
else
    echo "✅ .NET SDK encontrado: $(dotnet --version)"
fi

# Navega para o diretório do backend
cd /workspaces/ASP.NETCore/dotnet-backend

# Restaura dependências
echo "📦 Restaurando dependências do projeto..."
dotnet restore

if [ $? -ne 0 ]; then
    echo "❌ Erro ao restaurar dependências"
    exit 1
fi

# Compila o projeto
echo "🏗️ Compilando projeto..."
dotnet build

if [ $? -ne 0 ]; then
    echo "❌ Erro ao compilar projeto"
    exit 1
fi

echo "✅ Projeto compilado com sucesso"

# Configurações de ambiente
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS="http://localhost:5000"

echo ""
echo "🌟 Configuração:"
echo "  - Ambiente: $ASPNETCORE_ENVIRONMENT"
echo "  - URL: $ASPNETCORE_URLS"
echo "  - Ngrok Config: /workspaces/ASP.NETCore/ngrok.yml"
echo ""

# Inicia a aplicação
echo "🚀 Iniciando aplicação ASP.NET Core..."
echo "📝 Logs serão salvos em: logs/"
echo ""
echo "🌐 Acesse:"
echo "  - Aplicação: http://localhost:5000"
echo "  - Swagger: http://localhost:5000/swagger"
echo "  - Deployments: http://localhost:5000/deployments"
echo "  - Health Check: http://localhost:5000/health"
echo ""
echo "📡 O ngrok será iniciado automaticamente e criará um túnel público"
echo "💡 Para parar: Ctrl+C"
echo ""

# Executa a aplicação
dotnet run
