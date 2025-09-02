#!/bin/bash

# Script para inicializar cluster Kubernetes local
echo "🚀 Inicializando cluster Kubernetes local..."

# Verificar se Docker está rodando
if ! docker info &> /dev/null; then
    echo "❌ Docker não está rodando. Inicie o Docker primeiro."
    exit 1
fi

# Tentar Kind primeiro
if command -v kind &> /dev/null; then
    echo "📦 Usando Kind para criar cluster..."
    kind create cluster --name tutor-copiloto --config - <<KIND_CONFIG
kind: Cluster
apiVersion: kind.x-k8s.io/v1alpha4
nodes:
- role: control-plane
  kubeadmConfigPatches:
  - |
    kind: InitConfiguration
    nodeRegistration:
      kubeletExtraArgs:
        node-labels: "ingress-ready=true"
  extraPortMappings:
  - containerPort: 80
    hostPort: 80
    protocol: TCP
  - containerPort: 443
    hostPort: 443
    protocol: TCP
KIND_CONFIG
    
    echo "✅ Cluster Kind criado com sucesso!"
    echo "Para usar: kubectl cluster-info --context kind-tutor-copiloto"
    
elif command -v minikube &> /dev/null; then
    echo "📦 Usando Minikube para criar cluster..."
    minikube start --driver=docker --profile=tutor-copiloto
    
    echo "✅ Cluster Minikube criado com sucesso!"
    echo "Para usar: kubectl config use-context tutor-copiloto"
    
else
    echo "❌ Nem Kind nem Minikube estão instalados."
    echo "Instale um deles:"
    echo "  Kind: https://kind.sigs.k8s.io/"
    echo "  Minikube: https://minikube.sigs.k8s.io/"
    exit 1
fi

echo ""
echo "🎯 Próximos passos:"
echo "1. Configure os secrets: ./scripts/setup-secrets.sh"
echo "2. Deploy a aplicação: kubectl apply -f infra/deployments/kubernetes/"
echo "3. Verifique o status: kubectl get pods -n tutor-copiloto"
