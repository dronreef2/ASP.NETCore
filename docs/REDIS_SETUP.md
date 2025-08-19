# Configuração Redis Cloud

Este projeto suporta tanto Redis local quanto Redis Cloud para cache e gerenciamento de sessões.

## Configuração para Desenvolvimento Local

Para desenvolvimento local, use uma instância Redis padrão:

```bash
# Docker
docker run -d --name redis -p 6379:6379 redis:7-alpine

# URL de conexão no .env
REDIS_URL=redis://localhost:6379
```

## Configuração para Redis Cloud

### 1. Obter Credenciais Redis Cloud

1. Acesse o [Redis Cloud Console](https://redis.io/cloud/)
2. Crie um banco de dados ou use existente
3. Obtenha as credenciais de conexão:
   - Host: `redis-xxxxx.xxxxx.sa-east-1-2.ec2.redns.redis-cloud.com`
   - Porta: `16189`
   - Usuário: `default`
   - Senha: Sua senha do Redis Cloud

### 2. Configurar URL de Conexão

No arquivo `.env`, configure a URL no formato:

```bash
REDIS_URL=redis://usuario:senha@host:porta
```

**Exemplo:**
```bash
REDIS_URL=redis://default:SuaSenhaAqui@redis-16189.crce207.sa-east-1-2.ec2.redns.redis-cloud.com:16189
```

### 3. Configuração no Kubernetes

Para produção no Kubernetes, crie um secret:

```bash
# Criar secret com credentials Redis
kubectl create secret generic redis-credentials \
  --from-literal=redis-url="redis://default:SuaSenhaAqui@redis-16189.crce207.sa-east-1-2.ec2.redns.redis-cloud.com:16189"
```

O deployment já está configurado para usar este secret:

```yaml
env:
  - name: REDIS_URL
    valueFrom:
      secretKeyRef:
        name: redis-credentials
        key: redis-url
```

## Implementação no Código

O projeto usa `ioredis` (Node.js), que suporta automaticamente URLs Redis:

```typescript
import Redis from 'ioredis';

// Conecta automaticamente usando process.env.REDIS_URL
const redis = new Redis(process.env.REDIS_URL);
```

## Funcionalidades Utilizadas

O Redis é usado para:
- **Cache de embeddings** do LlamaIndex
- **Sessões de usuário** e autenticação
- **Rate limiting** para APIs
- **Cache de respostas** do Claude
- **Métricas temporárias** antes de persistir

## Configurações Recomendadas

### Para Desenvolvimento
- Redis local via Docker
- TTL cache: 1 hora
- Max connections: 10

### Para Produção
- Redis Cloud com cluster
- TTL cache: 6 horas
- Max connections: 100
- SSL/TLS habilitado

## Troubleshooting

### Erro de Conexão
```bash
# Testar conectividade
redis-cli -h redis-16189.crce207.sa-east-1-2.ec2.redns.redis-cloud.com -p 16189 -a SuaSenha ping
```

### Verificar Logs
```bash
# Logs do serviço
kubectl logs deployment/tutor-copiloto-backend | grep redis

# Logs detalhados
NODE_ENV=development LOG_LEVEL=debug npm run dev
```

### Configurações de Rede
- Certifique-se que a porta 16189 está acessível
- Para Kubernetes, configure NetworkPolicy se necessário
- Verifique firewalls e security groups na AWS

## Migração de Dados

Para migrar de Redis local para Redis Cloud:

```bash
# Backup do Redis local
redis-cli --rdb backup.rdb

# Restaurar no Redis Cloud (usando redis-cli)
redis-cli -h redis-16189.crce207.sa-east-1-2.ec2.redns.redis-cloud.com -p 16189 -a SuaSenha --rdb backup.rdb
```

## Monitoramento

O projeto inclui métricas Redis via OpenTelemetry:
- Conexões ativas
- Hit rate do cache
- Latência de operações
- Uso de memória

Acesse o Grafana em `http://localhost:3001` para visualizar.
