#!/bin/bash

echo "🩺 Testando health check da aplicação..."

# Verificar se dotnet está disponível
if ! command -v dotnet &> /dev/null; then
    echo "❌ dotnet não encontrado. Adicionando ao PATH..."
    export PATH="$PATH:/home/codespace/.dotnet"
fi

# Verificar se a aplicação está rodando
if pgrep -f "dotnet.*TutorCopiloto" > /dev/null; then
    echo "✅ Aplicação já está rodando"
else
    echo "🚀 Iniciando aplicação..."
    cd src/Web/API
    dotnet run --urls=http://localhost:8080 &
    sleep 10
fi

# Testar health check
echo "🔍 Testando endpoint /health..."
if curl -f -s http://localhost:8080/health > /dev/null; then
    echo "✅ Health check passou!"
    curl -s http://localhost:8080/health
else
    echo "❌ Health check falhou!"
    echo "Verificando se a aplicação está respondendo..."
    curl -s http://localhost:8080/ || echo "Aplicação não está respondendo"
fi

echo ""
echo "📊 Status da aplicação:"
ps aux | grep dotnet | grep -v grep || echo "Nenhum processo dotnet encontrado"
