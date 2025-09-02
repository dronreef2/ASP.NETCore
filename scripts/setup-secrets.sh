#!/bin/bash

# Script para configuração segura dos secrets do Kubernetes
# Uso: ./scripts/setup-secrets.sh

set -e

echo "🔐 Configurando secrets do Tutor Copiloto no Kubernetes..."

# Verificar se kubectl está instalado
if ! command -v kubectl &> /dev/null; then
    echo "❌ kubectl não encontrado. Instale o kubectl primeiro."
    exit 1
fi

# Verificar se está conectado a um cluster
if ! kubectl cluster-info &> /dev/null; then
    echo "⚠️  Não conectado a um cluster Kubernetes."
    echo "Para configurar secrets, primeiro conecte-se a um cluster válido:"
    echo "  - Para Minikube: minikube start"
    echo "  - Para Kind: kind create cluster"
    echo "  - Para AKS/EKS/GKE: configure kubectl"
    echo ""
    echo "Ou execute o deployment local com Docker Compose:"
    echo "  docker-compose up -d"
    exit 1
fi

# Criar namespace se não existir
echo "📁 Criando namespace tutor-copiloto..."
kubectl create namespace tutor-copiloto --dry-run=client -o yaml | kubectl apply -f -

# Função para ler input seguro
read_secret() {
    local prompt="$1"
    local var_name="$2"
    echo -n "$prompt"
    read -s value
    echo
    eval "$var_name='$value'"
}

# Coletar API keys
echo ""
echo "Por favor, forneça as seguintes API keys:"
echo ""

read_secret "Anthropic API Key (sk-ant-...): " ANTHROPIC_KEY
read_secret "LlamaIndex API Key (llx-...): " LLAMAINDEX_KEY

echo ""
echo "Configurações de banco (opcional - pressione Enter para usar padrões):"
echo ""

echo -n "PostgreSQL URL [postgresql://tutor:copiloto123@localhost:5432/tutordb]: "
read POSTGRES_URL
if [[ -z "$POSTGRES_URL" ]]; then
    POSTGRES_URL="postgresql://tutor:copiloto123@localhost:5432/tutordb"
fi

echo -n "Redis URL [redis://localhost:6379]: "
read REDIS_URL
if [[ -z "$REDIS_URL" ]]; then
    REDIS_URL="redis://localhost:6379"
fi

echo ""
echo -n "JWT Secret (deixe vazio para gerar automaticamente): "
read JWT_SECRET
if [[ -z "$JWT_SECRET" ]]; then
    JWT_SECRET=$(openssl rand -base64 32)
    echo "✅ JWT Secret gerado automaticamente"
fi

# Validar se as chaves foram fornecidas
if [[ -z "$ANTHROPIC_KEY" ]]; then
    echo "❌ Anthropic API Key é obrigatória"
    exit 1
fi

if [[ -z "$LLAMAINDEX_KEY" ]]; then
    echo "❌ LlamaIndex API Key é obrigatória"
    exit 1
fi

# Validar formato das chaves
if [[ ! "$ANTHROPIC_KEY" =~ ^sk-ant- ]]; then
    echo "⚠️ Aviso: Anthropic API Key não parece estar no formato correto (deve começar com sk-ant-)"
fi

if [[ ! "$LLAMAINDEX_KEY" =~ ^llx- ]]; then
    echo "⚠️ Aviso: LlamaIndex API Key não parece estar no formato correto (deve começar com llx-)"
fi

# Criar secret
echo ""
echo "🔧 Criando secrets no Kubernetes..."

kubectl create secret generic tutor-copiloto-secrets \
  --from-literal=anthropic-api-key="$ANTHROPIC_KEY" \
  --from-literal=llamaindex-api-key="$LLAMAINDEX_KEY" \
  --from-literal=postgres-url="$POSTGRES_URL" \
  --from-literal=redis-url="$REDIS_URL" \
  --from-literal=jwt-secret="$JWT_SECRET" \
  --namespace=tutor-copiloto \
  --dry-run=client -o yaml | kubectl apply -f -

# Verificar se o secret foi criado
if kubectl get secret tutor-copiloto-secrets -n tutor-copiloto &> /dev/null; then
    echo "✅ Secret criado com sucesso!"
    echo ""
    echo "📋 Secrets configurados:"
    echo "   • anthropic-api-key"
    echo "   • llamaindex-api-key" 
    echo "   • postgres-url"
    echo "   • redis-url"
    echo "   • jwt-secret"
else
    echo "❌ Falha ao criar secret"
    exit 1
fi

# Limpar variáveis da memória
unset ANTHROPIC_KEY
unset LLAMAINDEX_KEY
unset POSTGRES_URL
unset REDIS_URL
unset JWT_SECRET

echo ""
echo "🎉 Configuração de secrets concluída!"
echo ""
echo "Próximos passos:"
echo "1. Deploy da aplicação: kubectl apply -f infra/deployments/kubernetes/"
echo "2. Verificar pods: kubectl get pods -n tutor-copiloto"
echo "3. Verificar logs: kubectl logs -n tutor-copiloto -l app=orchestrator"
echo ""
echo "Para ver os secrets criados (sem valores):"
echo "kubectl get secrets -n tutor-copiloto"
