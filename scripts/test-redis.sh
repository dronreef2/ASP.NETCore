#!/bin/bash

# Script para testar conectividade Redis Cloud
# Usage: ./scripts/test-redis.sh

set -e

echo "🔗 Testando conectividade Redis..."

# Carregar variáveis do .env
if [ -f .env ]; then
    # Carregar apenas linhas válidas (sem comentários e linhas vazias)
    set -a
    source <(grep -v '^#' .env | grep -v '^$' | sed 's/ #.*//')
    set +a
else
    echo "❌ Arquivo .env não encontrado"
    exit 1
fi

# Verificar se REDIS_URL está configurado
if [ -z "$REDIS_URL" ]; then
    echo "❌ REDIS_URL não configurado no .env"
    exit 1
fi

echo "📍 Redis URL: $REDIS_URL"

# Extrair componentes da URL para teste manual
if [[ $REDIS_URL =~ redis://([^:]+):([^@]+)@([^:]+):([0-9]+) ]]; then
    USER="${BASH_REMATCH[1]}"
    PASS="${BASH_REMATCH[2]}"
    HOST="${BASH_REMATCH[3]}"
    PORT="${BASH_REMATCH[4]}"
    
    echo "🔧 Componentes extraídos:"
    echo "   Host: $HOST"
    echo "   Port: $PORT"
    echo "   User: $USER"
    echo "   Pass: ${PASS:0:3}***"
    
    # Teste de conectividade
    echo ""
    echo "🏓 Testando ping..."
    
    if command -v redis-cli &> /dev/null; then
        if redis-cli -h "$HOST" -p "$PORT" -a "$PASS" ping 2>/dev/null; then
            echo "✅ Conexão Redis bem-sucedida!"
            
            # Teste básico de escrita/leitura
            echo ""
            echo "📝 Testando operações básicas..."
            
            TEST_KEY="test:$(date +%s)"
            TEST_VALUE="tutor-copiloto-test"
            
            redis-cli -h "$HOST" -p "$PORT" -a "$PASS" SET "$TEST_KEY" "$TEST_VALUE" > /dev/null
            RESULT=$(redis-cli -h "$HOST" -p "$PORT" -a "$PASS" GET "$TEST_KEY")
            redis-cli -h "$HOST" -p "$PORT" -a "$PASS" DEL "$TEST_KEY" > /dev/null
            
            if [ "$RESULT" = "$TEST_VALUE" ]; then
                echo "✅ Operações de escrita/leitura funcionando!"
            else
                echo "⚠️ Problema com operações de escrita/leitura"
            fi
            
            # Informações do servidor
            echo ""
            echo "ℹ️ Informações do servidor Redis:"
            redis-cli -h "$HOST" -p "$PORT" -a "$PASS" INFO server | grep -E "redis_version|os|arch" | head -3
            
        else
            echo "❌ Falha na conexão Redis"
            echo ""
            echo "🔍 Possíveis problemas:"
            echo "   • Verificar credenciais"
            echo "   • Verificar conectividade de rede"
            echo "   • Verificar se o serviço está ativo"
            exit 1
        fi
    else
        echo "⚠️ redis-cli não encontrado - instalando..."
        
        # Tentar instalar redis-cli
        if command -v apt-get &> /dev/null; then
            sudo apt-get update && sudo apt-get install -y redis-tools
        elif command -v brew &> /dev/null; then
            brew install redis
        else
            echo "❌ Não foi possível instalar redis-cli automaticamente"
            echo "   Instale manualmente: apt-get install redis-tools"
            exit 1
        fi
        
        # Tentar novamente
        if redis-cli -h "$HOST" -p "$PORT" -a "$PASS" ping; then
            echo "✅ Conexão Redis bem-sucedida!"
        else
            echo "❌ Falha na conexão Redis"
            exit 1
        fi
    fi
    
else
    echo "❌ Formato de REDIS_URL inválido"
    echo "   Formato esperado: redis://user:password@host:port"
    exit 1
fi

echo ""
echo "🎉 Teste de conectividade Redis concluído!"
