# 🚀 Deployment Guide - Tutor Copiloto

## Opções de Deployment

### 1. Desenvolvimento Local (Recomendado) 🏠

Para desenvolvimento rápido sem complexidade de Kubernetes:

```bash
# Iniciar todos os serviços localmente
./scripts/start-local-dev.sh

# Ou manualmente:
docker-compose up -d
cd src/Web/API && dotnet run
```

**Serviços disponíveis:**
- 🌐 API: http://localhost:5000
- ��️ PostgreSQL: localhost:5432
- 🔄 Redis: localhost:6379
- 📊 PgAdmin: http://localhost:5050
- 🔍 Redis Commander: http://localhost:8081

### 2. Kubernetes Local 🐳

Para testar deployment em Kubernetes local:

```bash
# Inicializar cluster local
./scripts/init-local-cluster.sh

# Configurar secrets
./scripts/setup-secrets.sh

# Deploy aplicação
kubectl apply -f infra/deployments/kubernetes/

# Verificar status
kubectl get pods -n tutor-copiloto
```

### 3. Kubernetes em Nuvem ☁️

Para deployment em produção:

1. Configure seu cluster Kubernetes (AKS, EKS, GKE)
2. Execute: `./scripts/setup-secrets.sh`
3. Deploy: `kubectl apply -f infra/deployments/kubernetes/`
4. Configure ingress conforme necessário

## 🔧 Troubleshooting

### Erro: "deploy cluster proxy not ready"

Este erro ocorre quando:
- Não há cluster Kubernetes rodando
- Scripts tentam verificar conectividade com cluster inexistente

**Soluções:**
1. Use desenvolvimento local: `./scripts/start-local-dev.sh`
2. Configure cluster local: `./scripts/init-local-cluster.sh`
3. Ignore verificações de cluster para desenvolvimento

### Verificar Status dos Serviços

```bash
# Docker Compose
docker-compose ps

# Kubernetes
kubectl get pods -n tutor-copiloto
kubectl get services -n tutor-copiloto

# Aplicação
curl http://localhost:5000/health
```

### Logs de Debug

```bash
# Docker Compose
docker-compose logs -f

# Kubernetes
kubectl logs -n tutor-copiloto deployment/orchestrator

# Aplicação
tail -f src/Web/API/logs/*.txt
```

## 📋 Pré-requisitos

- Docker e Docker Compose
- .NET 8.0 SDK
- kubectl (opcional, para Kubernetes)
- kind ou minikube (opcional, para cluster local)

## 🎯 Recomendações

- **Desenvolvimento**: Use Docker Compose local
- **Testes**: Use cluster local com Kind/Minikube
- **Produção**: Use Kubernetes gerenciado (AKS/EKS/GKE)
