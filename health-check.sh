#!/bin/bash

# Script de Health Check para Railway
# Este script verifica se todos os serviços estão funcionando

echo "=== TUTOR COPILOTO HEALTH CHECK ==="
echo "Data/Hora: $(date)"
echo "Ambiente: ${RAILWAY_ENVIRONMENT:-Production}"
echo ""

# Verificar se a aplicação está respondendo
echo "1. Verificando API principal..."
if curl -f -s http://localhost:8080/health > /dev/null 2>&1; then
    echo "✅ API está respondendo"
else
    echo "❌ API não está respondendo"
    exit 1
fi

# Verificar conexão com banco de dados
echo "2. Verificando conexão com banco de dados..."
if curl -f -s http://localhost:8080/api/health/database > /dev/null 2>&1; then
    echo "✅ Banco de dados conectado"
else
    echo "❌ Problema na conexão com banco de dados"
fi

# Verificar Redis
echo "3. Verificando Redis..."
if curl -f -s http://localhost:8080/api/health/redis > /dev/null 2>&1; then
    echo "✅ Redis funcionando"
else
    echo "❌ Redis não disponível"
fi

# Verificar serviços de IA
echo "4. Verificando serviços de IA..."
if curl -f -s http://localhost:8080/api/health/ai > /dev/null 2>&1; then
    echo "✅ Serviços de IA funcionando"
else
    echo "❌ Problema nos serviços de IA"
fi

# Verificar SignalR
echo "5. Verificando SignalR..."
if curl -f -s http://localhost:8080/api/health/signalr > /dev/null 2>&1; then
    echo "✅ SignalR funcionando"
else
    echo "❌ SignalR não disponível"
fi

# Verificar uso de memória
echo "6. Verificando uso de recursos..."
MEMORY_USAGE=$(ps aux --no-headers -o pmem -C dotnet | awk '{sum+=$1} END {print sum}')
if (( $(echo "$MEMORY_USAGE < 80" | bc -l) )); then
    echo "✅ Uso de memória OK: ${MEMORY_USAGE}%"
else
    echo "⚠️  Alto uso de memória: ${MEMORY_USAGE}%"
fi

# Verificar espaço em disco
DISK_USAGE=$(df / | tail -1 | awk '{print $5}' | sed 's/%//')
if [ "$DISK_USAGE" -lt 90 ]; then
    echo "✅ Espaço em disco OK: ${DISK_USAGE}%"
else
    echo "⚠️  Espaço em disco baixo: ${DISK_USAGE}%"
fi

echo ""
echo "=== HEALTH CHECK CONCLUÍDO ==="

# Verificar se há erros críticos
if curl -f -s http://localhost:8080/health > /dev/null 2>&1; then
    echo "✅ Status: SAUDÁVEL"
    exit 0
else
    echo "❌ Status: PROBLEMA CRÍTICO"
    exit 1
fi
