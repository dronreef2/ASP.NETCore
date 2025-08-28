# Tutor Copiloto - GitHub Chat MCP Integration

Esta é a integração do [GitHub Chat MCP](https://github.com/AsyncFuncAI/github-chat-mcp) com o sistema Tutor Copiloto.

## Visão Geral

Esta integração permite que o Tutor Copiloto utilize as capacidades avançadas de análise e consulta de repositórios GitHub fornecidas pelo GitHub Chat MCP, incluindo:

- ✅ Indexação automática de repositórios
- ✅ Consultas inteligentes sobre código
- ✅ Análise de arquitetura e tech stack
- ✅ Perguntas contextuais sobre repositórios
- ✅ Suporte a conversas multi-turn

## Pré-requisitos

- Python 3.12 ou superior
- [uv](https://astral.sh/uv) (gerenciador de pacotes Python)
- Chave da API do GitHub (opcional, mas recomendada)

## Instalação

1. **Instalar uv (se não tiver):**
   ```bash
   curl -LsSf https://astral.sh/uv/install.sh | sh
   ```

2. **Configurar variáveis de ambiente:**
   ```bash
   cp .env.example .env
   # Edite o arquivo .env com suas configurações
   ```

3. **Instalar dependências:**
   ```bash
   uv sync
   ```

## Uso

### Iniciar o servidor de integração

```bash
# Usando o script de inicialização
./start-integration.sh

# Ou diretamente com uv
uv run python -m tutor_copiloto_github.main
```

O servidor estará disponível em `http://localhost:8001`

### Endpoints da API

#### Indexar um repositório
```bash
curl -X POST http://localhost:8001/api/github/index \
  -H "Content-Type: application/json" \
  -d '{"repo_url": "https://github.com/microsoft/vscode"}'
```

#### Consultar um repositório
```bash
curl -X POST http://localhost:8001/api/github/query \
  -H "Content-Type: application/json" \
  -d '{
    "repo_url": "https://github.com/microsoft/vscode",
    "question": "Qual é a arquitetura principal deste projeto?"
  }'
```

#### Verificar status
```bash
curl http://localhost:8001/api/github/status
```

## Integração com o Tutor Copiloto

Esta integração se conecta automaticamente com o sistema principal do Tutor Copiloto através das seguintes formas:

1. **API REST**: O Tutor Copiloto pode chamar os endpoints desta API
2. **WebSocket/SignalR**: Comunicação em tempo real para atualizações
3. **Banco de dados compartilhado**: Sincronização de análises

## Arquitetura

```
┌─────────────────┐    HTTP     ┌──────────────────────┐
│  Tutor Copiloto │◄──────────►│ GitHub Chat MCP API  │
│   (.NET Core)   │             │     (Python)        │
└─────────────────┘             └──────────────────────┘
         │                               │
         │                               │
         ▼                               ▼
┌─────────────────┐             ┌──────────────────────┐
│   Database      │             │  GitHub Chat API    │
│  (PostgreSQL)   │◄──────────►│  (External Service) │
└─────────────────┘             └──────────────────────┘
```

## Desenvolvimento

### Executar testes
```bash
uv run pytest
```

### Formatação de código
```bash
uv run black src/
uv run isort src/
```

### Verificação de tipos
```bash
uv run mypy src/
```

## Configuração Avançada

### Usando com Claude Desktop

Para usar esta integração com o Claude Desktop, configure o MCP no `claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "tutor-copiloto-github": {
      "command": "uv",
      "args": [
        "--directory",
        "/path/to/python-integration",
        "run",
        "python",
        "-m",
        "tutor_copiloto_github.main"
      ],
      "env": {
        "GITHUB_API_KEY": "your_api_key_here"
      }
    }
  }
}
```

## Troubleshooting

### Problemas comuns

1. **Erro de conexão com GitHub API**
   - Verifique se a `GITHUB_API_KEY` está configurada
   - Confirme que a chave tem as permissões necessárias

2. **Erro de porta já em uso**
   - Mude a porta no arquivo `.env`: `MCP_SERVER_PORT=8002`

3. **Erro de dependências**
   - Execute `uv sync --reinstall` para reinstalar dependências

### Logs

Os logs são exibidos no console. Para aumentar a verbosidade:
```bash
LOG_LEVEL=DEBUG uv run python -m tutor_copiloto_github.main
```

## Contribuição

Para contribuir com esta integração:

1. Fork o repositório
2. Crie uma branch para sua feature
3. Faça commit das suas mudanças
4. Push para a branch
5. Abra um Pull Request

## Licença

Esta integração é parte do projeto Tutor Copiloto e segue a mesma licença.
