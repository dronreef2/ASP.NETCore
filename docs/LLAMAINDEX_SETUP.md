# 🔐 Configuração Segura de API Keys

## ⚠️ IMPORTANTE - Chave LlamaIndex

**Chave fornecida**: `llx-3eJdHtkOfZx7fkzusuoDg6nmRxWJGMkkHHzrbw63bZMkuMfr`

Esta chave foi armazenada aqui **temporariamente** para referência. **MOVA IMEDIATAMENTE** para um local seguro seguindo as instruções abaixo.

## 🛡️ Armazenamento Seguro

### 1. Desenvolvimento Local
```bash
# Copie a chave para seu .env local (NÃO commitado)
cp .env.example .env
nano .env

# Substitua a linha:
LLAMAINDEX_API_KEY=llx-3eJdHtkOfZx7fkzusuoDg6nmRxWJGMkkHHzrbw63bZMkuMfr
```

### 2. Produção - Kubernetes Secrets
```bash
# Crie o secret no cluster
kubectl create secret generic tutor-copiloto-secrets \
  --from-literal=llamaindex-api-key="llx-3eJdHtkOfZx7fkzusuoDg6nmRxWJGMkkHHzrbw63bZMkuMfr" \
  --namespace=tutor-copiloto

# Ou via YAML (base64 encoded)
echo -n "llx-3eJdHtkOfZx7fkzusuoDg6nmRxWJGMkkHHzrbw63bZMkuMfr" | base64
# Resultado: bGx4LTNlSmRIdGtPZlp4N2ZrenVzdW9EZzZubVJ4V0pHTWtrSEh6cmJ3NjNiWk1rdU1mcg==
```

### 3. CI/CD - GitHub Actions
```yaml
# .github/workflows/deploy.yml
env:
  LLAMAINDEX_API_KEY: ${{ secrets.LLAMAINDEX_API_KEY }}
```

### 4. Outros Ambientes
```bash
# Heroku
heroku config:set LLAMAINDEX_API_KEY=llx-3eJdHtkOfZx7fkzusuoDg6nmRxWJGMkkHHzrbw63bZMkuMfr

# AWS Systems Manager
aws ssm put-parameter \
  --name "/tutor-copiloto/llamaindex-api-key" \
  --value "llx-3eJdHtkOfZx7fkzusuoDg6nmRxWJGMkkHHzrbw63bZMkuMfr" \
  --type "SecureString"

# Azure Key Vault
az keyvault secret set \
  --vault-name "tutor-copiloto-kv" \
  --name "llamaindex-api-key" \
  --value "llx-3eJdHtkOfZx7fkzusuoDg6nmRxWJGMkkHHzrbw63bZMkuMfr"
```

## 🔄 Integração com LlamaIndex

### Configuração no Orquestrador
```typescript
// backend/orchestrator/src/services/llamaindex.ts
import { LlamaIndex } from 'llamaindex';

export class LlamaIndexService {
  private client: LlamaIndex;

  constructor() {
    this.client = new LlamaIndex({
      apiKey: process.env.LLAMAINDEX_API_KEY,
    });
  }

  async createEmbeddings(text: string): Promise<number[]> {
    const response = await this.client.embeddings.create({
      input: text,
      model: 'text-embedding-ada-002',
    });
    return response.data[0].embedding;
  }

  async queryIndex(query: string, topK: number = 5): Promise<any[]> {
    const results = await this.client.query({
      query,
      topK,
      includeMetadata: true,
    });
    return results;
  }
}
```

### Configuração Docker/Kubernetes
```yaml
# Adicionar ao deployment.yaml
env:
- name: LLAMAINDEX_API_KEY
  valueFrom:
    secretKeyRef:
      name: tutor-copiloto-secrets
      key: llamaindex-api-key
```

## ✅ Checklist de Segurança

- [ ] Chave movida do arquivo de documentação para .env local
- [ ] .env adicionado ao .gitignore (já está)
- [ ] Secret criado no Kubernetes para produção
- [ ] Variável configurada no CI/CD
- [ ] Documentação atualizada para equipe
- [ ] Chave testada em ambiente de desenvolvimento
- [ ] Chave testada em ambiente de produção

## 🚨 Se a Chave for Comprometida

1. **Revogue imediatamente** a chave no dashboard da LlamaIndex
2. **Gere uma nova chave** na plataforma
3. **Atualize todos os ambientes** com a nova chave
4. **Rotacione secrets** no Kubernetes/Cloud
5. **Audite logs** para uso não autorizado

## 📚 Documentação LlamaIndex

- [Dashboard](https://cloud.llamaindex.ai/)
- [API Documentation](https://docs.llamaindex.ai/)
- [SDKs](https://github.com/run-llama/llama_index)

---

**⚠️ LEMBRE-SE: Após configurar em todos os ambientes, DELETE este arquivo ou remova a chave desta documentação!**
