#!/bin/bash

echo "🚀 Testando Sistema de Fallback de IA - Tutor Copiloto"
echo "=================================================="

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Função para testar endpoint
test_endpoint() {
    local url=$1
    local description=$2

    echo -e "\n${YELLOW}Testando: ${description}${NC}"
    echo "URL: $url"

    response=$(curl -s -w "\nHTTP_STATUS:%{http_code}" "$url")

    http_status=$(echo "$response" | grep "HTTP_STATUS:" | cut -d: -f2)
    body=$(echo "$response" | sed '/HTTP_STATUS:/d')

    if [ "$http_status" = "200" ]; then
        echo -e "${GREEN}✅ Sucesso (HTTP $http_status)${NC}"
        echo "Resposta: ${body:0:100}..."
    else
        echo -e "${RED}❌ Falha (HTTP $http_status)${NC}"
        echo "Erro: $body"
    fi
}

# Verificar se a aplicação está rodando
echo -e "\n${YELLOW}Verificando se a aplicação está rodando...${NC}"
if curl -s http://localhost:5000/health > /dev/null 2>&1; then
    echo -e "${GREEN}✅ Aplicação está rodando${NC}"
else
    echo -e "${RED}❌ Aplicação não está rodando. Execute: dotnet run${NC}"
    exit 1
fi

# Testar endpoints
test_endpoint "http://localhost:5000/api/aistatus/status" "Status dos Provedores de IA"
test_endpoint "http://localhost:5000/api/chat" "Chat com fallback (POST)" "POST"
test_endpoint "http://localhost:5000/swagger" "Documentação Swagger"

# Teste específico do chat (POST)
echo -e "\n${YELLOW}Testando Chat com mensagem de teste...${NC}"
chat_response=$(curl -s -X POST http://localhost:5000/api/chat \
    -H "Content-Type: application/json" \
    -d '{"message":"Olá, teste do sistema de fallback","userId":"test-script"}')

if echo "$chat_response" | grep -q "message"; then
    echo -e "${GREEN}✅ Chat funcionando com fallback${NC}"
    echo "Resposta: $(echo "$chat_response" | jq -r '.message' 2>/dev/null || echo "$chat_response")"
else
    echo -e "${RED}❌ Chat falhou${NC}"
    echo "Resposta: $chat_response"
fi

echo -e "\n${GREEN}🎉 Teste do Sistema de Fallback concluído!${NC}"
echo -e "${YELLOW}Para configurar chaves de API adicionais, edite appsettings.json:${NC}"
echo "  - OpenAI: AI:OpenAI:ApiKey"
echo "  - Anthropic: AI:Anthropic:ApiKey"
