# Tutor Copiloto - Documentação de Deploy

## Visão Geral

O Tutor Copiloto é uma plataforma educacional que combina IA avançada com ferramentas de desenvolvimento para criar uma experiência de aprendizado interativa e personalizada.

## Arquitetura

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   VS Code Ext   │    │    Web SPA      │    │   Mobile App    │
│   (TypeScript)  │    │    (React)      │    │   (Flutter)     │
└─────────┬───────┘    └─────────┬───────┘    └─────────┬───────┘
          │                      │                      │
          └──────────┬───────────┴──────────────────────┘
                     │
         ┌───────────▼────────────┐
         │    Load Balancer       │
         │    (Nginx Ingress)     │
         └───────────┬────────────┘
                     │
         ┌───────────▼────────────┐
         │    Orquestrador BFF    │
         │    (Node.js/Fastify)   │
         │    - Streaming Chat    │
         │    - Tool Coordination │
         │    - Prompt Assembly   │
         └───────────┬────────────┘
                     │
    ┌────────────────┼────────────────┐
    │                │                │
┌───▼────┐    ┌─────▼─────┐    ┌─────▼─────┐
│Anthropic│    │    RAG    │    │   Tools   │
│ Claude  │    │ (pgvector)│    │Container  │
│   API   │    │Embeddings │    │Execution  │
└─────────┘    └───────────┘    └───────────┘
```

## Pré-requisitos

### Desenvolvimento Local
- Node.js 18+
- Docker & Docker Compose
- VS Code (para extensão)
- Git

### Produção
- Kubernetes cluster (v1.25+)
- Nginx Ingress Controller
- Cert-Manager (para SSL)
- Prometheus & Grafana (observabilidade)
- PostgreSQL com pgvector

## Configuração de Desenvolvimento

### 1. Clone e Setup
```bash
git clone https://github.com/org/tutor-copiloto.git
cd tutor-copiloto

# Instalar dependências
npm install

# Setup ambiente
cp .env.example .env
# Editar .env com suas chaves de API
```

### 2. Configuração de Ambiente
```env
# .env
ANTHROPIC_API_KEY=sk-ant-xxxx
POSTGRES_URL=postgresql://user:pass@localhost:5432/tutordb
REDIS_URL=redis://localhost:6379
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317
LOG_LEVEL=debug
VSCODE_DEV_MODE=true
```

### 3. Desenvolvimento Local
```bash
# Terminal 1: Backend services
docker-compose up -d postgres redis otel-collector

# Terminal 2: Orquestrador
cd backend/orchestrator
npm run dev

# Terminal 3: Web SPA
cd clients/web-spa
npm run dev

# Terminal 4: VS Code Extension
cd clients/vscode-ext
npm run watch
# Pressionar F5 no VS Code para debug da extensão
```

## Deploy em Produção

### 1. Build e Push das Imagens
```bash
# Build todas as imagens
docker build -t tutor-copiloto/orchestrator:1.0.0 -f backend/orchestrator/Dockerfile .
docker build -t tutor-copiloto/web-spa:1.0.0 -f clients/web-spa/Dockerfile .

# Push para registry (substituir por seu registry)
docker push tutor-copiloto/orchestrator:1.0.0
docker push tutor-copiloto/web-spa:1.0.0
```

### 2. Configuração de Secrets
```bash
# Criar namespace
kubectl create namespace tutor-copiloto

# Configurar secrets
kubectl create secret generic tutor-copiloto-secrets \
  --from-literal=anthropic-api-key="sk-ant-your-key" \
  --namespace=tutor-copiloto

# SSL Certificate (se usando cert-manager)
kubectl apply -f infra/deployments/kubernetes/certificates.yaml
```

### 3. Deploy da Aplicação
```bash
# Deploy principal
kubectl apply -f infra/deployments/kubernetes/deployment.yaml

# Configurar ingress
kubectl apply -f infra/deployments/kubernetes/ingress.yaml

# Verificar status
kubectl get pods -n tutor-copiloto
kubectl get services -n tutor-copiloto
kubectl get ingress -n tutor-copiloto
```

### 4. Configuração de Monitoramento
```bash
# Deploy Prometheus & Grafana
kubectl apply -f infra/observability/prometheus.yaml
kubectl apply -f infra/observability/grafana.yaml

# Importar dashboards
kubectl apply -f infra/observability/dashboards/
```

## Configurações de Segurança

### 1. RBAC e Políticas de Rede
```yaml
# Já incluído nos manifestos K8s
- ServiceAccount com permissões mínimas
- NetworkPolicy restritiva
- PodSecurityPolicy
- Secrets gerenciados via K8s
```

### 2. Configurações de Container
```yaml
securityContext:
  runAsNonRoot: true
  runAsUser: 1001
  allowPrivilegeEscalation: false
  readOnlyRootFilesystem: true
  capabilities:
    drop: ["ALL"]
```

### 3. Configurações de Ingress
```yaml
# Rate limiting
nginx.ingress.kubernetes.io/rate-limit: "100"
nginx.ingress.kubernetes.io/rate-limit-window: "1m"

# Security headers automáticos
# CORS configurado
# SSL obrigatório
```

## Monitoramento e Observabilidade

### Métricas Importantes
- **Request Rate**: Requisições por segundo
- **Latency**: P95/P99 de tempo de resposta
- **Error Rate**: Taxa de erro por endpoint
- **Token Usage**: Consumo de tokens da Anthropic
- **Cost Tracking**: Custo por usuário/sessão
- **Tool Execution**: Sucesso/falha das ferramentas

### Dashboards Grafana
1. **Overview**: Métricas gerais da aplicação
2. **Performance**: Latência e throughput
3. **Costs**: Análise de custos por modelo/usuário
4. **Educational**: Métricas pedagógicas
5. **Infrastructure**: Recursos K8s

### Alertas Configurados
- Alta latência (P95 > 5s)
- Taxa de erro > 5%
- Custo diário > threshold
- Falha de ferramentas > 10%
- Pods não responsivos

## Estratégias de Scaling

### Horizontal Pod Autoscaler (HPA)
```yaml
# Configurado para escalar baseado em:
- CPU > 70%
- Memory > 80%
- Custom metrics (requests/second)
```

### Vertical Pod Autoscaler (VPA)
```yaml
# Recomendações automáticas de recursos
updatePolicy:
  updateMode: "Auto"
```

### Cluster Autoscaler
```yaml
# Escala nós conforme demanda
# Configurado nos node groups
```

## Backup e Disaster Recovery

### Dados para Backup
- Conversas e histórico (PostgreSQL)
- Configurações (ConfigMaps/Secrets)
- Métricas históricas (Prometheus)

### Estratégia de Backup
```bash
# PostgreSQL
kubectl exec -n tutor-copiloto postgres-0 -- pg_dump tutordb > backup.sql

# Configurações
kubectl get configmaps,secrets -n tutor-copiloto -o yaml > config-backup.yaml

# Volumes persistentes
velero backup create tutor-copiloto-backup --include-namespaces tutor-copiloto
```

## Troubleshooting

### Logs
```bash
# Logs do orquestrador
kubectl logs -n tutor-copiloto -l app=orchestrator -f

# Logs com contexto estruturado
kubectl logs -n tutor-copiloto orchestrator-xxx --previous
```

### Debugging
```bash
# Conectar ao pod para debug
kubectl exec -it -n tutor-copiloto orchestrator-xxx -- sh

# Port forward para teste local
kubectl port-forward -n tutor-copiloto svc/orchestrator-service 8080:80
```

### Problemas Comuns

1. **Anthropic API Rate Limit**
   - Verificar configuração de rate limiting
   - Implementar circuit breaker
   - Usar modelo mais barato para requests simples

2. **High Memory Usage**
   - Verificar cache do Node.js
   - Ajustar limites de recursos
   - Monitoring de memory leaks

3. **Tool Execution Timeouts**
   - Verificar limites de container
   - Logs dos containers de ferramenta
   - Rede entre pods

## Roadmap de Melhorias

### Sprint 6+ (Futuro)
- [ ] Integração com mais LLMs (OpenAI, Gemini)
- [ ] Plugin system para ferramentas custom
- [ ] Analytics avançados de aprendizado
- [ ] Modo offline para VS Code extension
- [ ] Mobile app (Flutter)
- [ ] Integração com LMS (Moodle, Canvas)
- [ ] Multi-tenant architecture
- [ ] Advanced RBAC with fine-grained permissions

---

**Maintainers**: Time de DevOps e Educação  
**Last Updated**: 19/08/2025  
**Version**: 1.0.0
