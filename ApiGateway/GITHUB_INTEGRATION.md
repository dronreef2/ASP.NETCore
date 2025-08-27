# GitHub API Integration via Gateway

Este documento explica como usar o gateway YARP para consultar informações do GitHub e obter diagnósticos do sistema.

## 🚀 Configuração do GitHub

### 1. Obter Token de Acesso

1. Acesse [GitHub Settings > Developer settings > Personal access tokens](https://github.com/settings/tokens)
2. Clique em "Generate new token (classic)"
3. Selecione as permissões necessárias:
   - `repo` - Acesso completo aos repositórios
   - `read:user` - Ler informações do perfil
   - `read:org` - Ler informações da organização (se aplicável)
4. Copie o token gerado

### 2. Configurar Token no Gateway

#### Opção A: Via appsettings.GitHub.json
```json
{
  "GitHub": {
    "Token": "YOUR_GITHUB_TOKEN_HERE"
  }
}
```

#### Opção B: Via variável de ambiente
```bash
export GITHUB_TOKEN=your_token_here
```

#### Opção C: Via header na requisição
```http
Authorization: Bearer YOUR_GITHUB_TOKEN_HERE
```

## 📋 Rotas Disponíveis

### Repositórios

#### Listar repositórios do usuário autenticado
```
GET /github/user/repos
```

#### Informações de um repositório específico
```
GET /github/repos/{owner}/{repo}
```
**Exemplo:**
```
GET /github/repos/dronreef2/ASP.NETCore
```

#### Listar issues
```
GET /github/repos/{owner}/{repo}/issues
```
**Parâmetros de query:**
- `state` - `open`, `closed`, `all` (padrão: `open`)
- `labels` - Filtrar por labels
- `per_page` - Itens por página (máximo: 100)
- `page` - Página

#### Listar Pull Requests
```
GET /github/repos/{owner}/{repo}/pulls
```
**Parâmetros similares aos de issues**

#### Commits recentes
```
GET /github/repos/{owner}/{repo}/commits
```
**Parâmetros:**
- `sha` - Branch ou commit específico
- `path` - Filtrar por caminho
- `author` - Filtrar por autor
- `since` - Commits desde data (ISO 8601)
- `until` - Commits até data (ISO 8601)

#### Branches
```
GET /github/repos/{owner}/{repo}/branches
```

#### Conteúdo de arquivos
```
GET /github/repos/{owner}/{repo}/contents/{path}
```
**Exemplo:**
```
GET /github/repos/dronreef2/ASP.NETCore/contents/README.md
```

### Diagnósticos e Monitoramento

#### Status geral do sistema
```
GET /diagnostics/status
```

#### Métricas de performance
```
GET /metrics/performance
```

#### Logs do sistema
```
GET /diagnostics/logs?level=info&limit=100
```
**Parâmetros:**
- `level` - `debug`, `info`, `warn`, `error`
- `limit` - Número máximo de entradas
- `since` - Data inicial (ISO 8601)

#### Health check detalhado
```
GET /status/detailed
```

#### Estatísticas do repositório
```
GET /diagnostics/repo-stats
```

#### Relatório de atividade
```
GET /diagnostics/activity-report?period=7d
```
**Parâmetros:**
- `period` - Período (`1d`, `7d`, `30d`, `90d`)

## 🧪 Exemplos de Uso

### JavaScript/Node.js
```javascript
const axios = require('axios');

const gatewayUrl = 'http://localhost:8080';
const githubToken = 'YOUR_GITHUB_TOKEN_HERE';

// Consultar repositório
const repoInfo = await axios.get(`${gatewayUrl}/github/repos/dronreef2/ASP.NETCore`, {
  headers: {
    'Authorization': `Bearer ${githubToken}`,
    'Accept': 'application/vnd.github.v3+json',
    'User-Agent': 'MyApp/1.0'
  }
});

// Listar issues
const issues = await axios.get(`${gatewayUrl}/github/repos/dronreef2/ASP.NETCore/issues`, {
  headers: {
    'Authorization': `Bearer ${githubToken}`,
    'Accept': 'application/vnd.github.v3+json',
    'User-Agent': 'MyApp/1.0'
  }
});
```

### Python
```python
import requests

gateway_url = 'http://localhost:8080'
github_token = 'YOUR_GITHUB_TOKEN_HERE'
headers = {
    'Authorization': f'Bearer {github_token}',
    'Accept': 'application/vnd.github.v3+json',
    'User-Agent': 'MyApp/1.0'
}

# Consultar repositório
response = requests.get(f'{gateway_url}/github/repos/dronreef2/ASP.NETCore', headers=headers)
repo_data = response.json()

# Listar commits recentes
response = requests.get(f'{gateway_url}/github/repos/dronreef2/ASP.NETCore/commits?per_page=5', headers=headers)
commits = response.json()
```

### C#
```csharp
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

var client = new HttpClient();
client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "YOUR_GITHUB_TOKEN_HERE");
client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("MyApp", "1.0"));

var gatewayUrl = "http://localhost:8080";

// Consultar repositório
var response = await client.GetAsync($"{gatewayUrl}/github/repos/dronreef2/ASP.NETCore");
var repoData = await response.Content.ReadFromJsonAsync<JsonElement>();

// Listar issues
var issuesResponse = await client.GetAsync($"{gatewayUrl}/github/repos/dronreef2/ASP.NETCore/issues");
var issues = await issuesResponse.Content.ReadFromJsonAsync<JsonElement[]>();
```

## ⚠️ Considerações de Segurança

1. **Nunca commite tokens no código** - Use variáveis de ambiente
2. **Configure rate limiting** no gateway para evitar abuso
3. **Valide permissões** antes de permitir acesso às rotas
4. **Monitore uso** através dos logs de diagnóstico
5. **Use HTTPS** em produção

## 🔧 Configuração Avançada

### Rate Limiting
Configure limites de taxa no cluster do GitHub:

```json
{
  "Clusters": {
    "githubCluster": {
      "RateLimiterPolicy": "FixedWindow",
      "RateLimiterOptions": {
        "Window": "00:01:00",
        "PermitLimit": 100
      }
    }
  }
}
```

### Autenticação Personalizada
Para implementar autenticação customizada, adicione transforms:

```json
{
  "Transforms": [
    {
      "RequestHeader": "X-API-Key",
      "Set": "{header}",
      "When": "RequestHeaderMissing"
    }
  ]
}
```

## 📊 Monitoramento

### Logs do Gateway
O gateway registra todas as requisições. Para visualizar:
```bash
# Logs em tempo real
dotnet run --environment Development

# Ou consultar via API
GET /diagnostics/logs?level=info&limit=50
```

### Métricas
```bash
# Métricas de performance
GET /metrics/performance

# Estatísticas de uso
GET /diagnostics/repo-stats
```

## 🚨 Troubleshooting

### Erro 401 Unauthorized
- Verifique se o token do GitHub é válido
- Confirme se o token tem as permissões necessárias
- Certifique-se de que o header `Authorization` está sendo enviado

### Erro 403 Forbidden
- O repositório pode ser privado - verifique as permissões do token
- Você pode ter excedido o rate limit do GitHub

### Erro 404 Not Found
- Verifique se o owner/repo estão corretos
- O repositório pode não existir ou estar privado

### Timeout
- Aumente o timeout no cluster se necessário
- Verifique conectividade com api.github.com

## 📚 Referências

- [GitHub REST API Documentation](https://docs.github.com/en/rest)
- [YARP Reverse Proxy Documentation](https://microsoft.github.io/reverse-proxy/)
- [Rate Limiting](https://docs.github.com/en/rest/overview/resources-in-the-rest-api#rate-limiting)</content>
<parameter name="filePath">/workspaces/ASP.NETCore/ApiGateway/GITHUB_INTEGRATION.md
