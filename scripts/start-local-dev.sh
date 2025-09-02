#!/bin/bash

# Script para iniciar desenvolvimento local sem Kubernetes
echo "🚀 Iniciando desenvolvimento local com Docker Compose..."

# Verificar se Docker está rodando
if ! docker info &> /dev/null; then
    echo "❌ Docker não está rodando. Inicie o Docker primeiro."
    exit 1
fi

# Iniciar serviços com Docker Compose
echo "📦 Iniciando serviços..."
docker-compose up -d

# Aguardar serviços ficarem prontos
echo "⏳ Aguardando serviços ficarem prontos..."
sleep 10

# Verificar status dos serviços
echo "📊 Status dos serviços:"
docker-compose ps

# Iniciar aplicação ASP.NET Core
echo "🏃 Iniciando aplicação ASP.NET Core..."
cd src/Web/API
dotnet run --urls=http://localhost:5000 &

echo ""
echo "✅ Desenvolvimento local iniciado!"
echo "🌐 API disponível em: http://localhost:5000"
echo "📊 PgAdmin em: http://localhost:5050"
echo "🔍 Redis Commander em: http://localhost:8081"
echo ""
echo "Para parar: docker-compose down"
echo "Para logs: docker-compose logs -f"
