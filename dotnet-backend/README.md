# 🎓 Tutor Copiloto - ASP.NET Core Backend

> Implementação ASP.NET Core da plataforma educacional Tutor Copiloto

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-green.svg)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15+-336791.svg)](https://www.postgresql.org/)
[![SignalR](https://img.shields.io/badge/SignalR-Tempo%20Real-orange.svg)](https://dotnet.microsoft.com/apps/aspnet/signalr)

## 🏗️ Estrutura do Projeto

```
dotnet-backend/
├── Controllers/
│   └── RelatorioController.cs      → API REST para relatórios
├── Services/
│   ├── IRelatorioService.cs        → Interface para DI
│   └── RelatorioService.cs         → Implementação do serviço
├── Hubs/
│   └── ChatHub.cs                  → Hub SignalR para tempo real
├── Pages/
│   ├── Index.cshtml                → Razor Page com i18n
│   └── Index.cshtml.cs             → PageModel com lógica
├── Models/
│   └── DomainModels.cs            → Modelos de domínio
├── Data/
│   └── TutorDbContext.cs          → Entity Framework Context
├── Resources/
│   ├── Index.pt-BR.resx           → Traduções português
│   └── Index.en.resx              → Traduções inglês
├── wwwroot/                       → Arquivos estáticos
├── Program.cs                     → Configuração de DI, SignalR, Swagger
└── appsettings.json              → Configurações da aplicação
```

## 🚀 Quick Start

### Pré-requisitos
- .NET 8.0 SDK
- PostgreSQL 15+
- Redis (local ou Redis Cloud)

### 1. Instalação e Configuração

```bash
# Navegar para o diretório ASP.NET Core
cd dotnet-backend

# Restaurar pacotes NuGet
dotnet restore

# Configurar variáveis de ambiente
export POSTGRES_URL="Host=localhost;Database=tutordb;Username=tutor;Password=copiloto123"
export REDIS_URL="redis://localhost:6379"
export JWT_SECRET="your-super-secret-jwt-key"

# Ou configurar no appsettings.json / appsettings.Development.json
```

### 2. Executar a Aplicação

```bash
# Desenvolvimento
dotnet run

# Ou com hot reload
dotnet watch run

# Produção
dotnet run --environment Production
```

### 3. Acessar Interfaces

- **🏠 Interface Web**: http://localhost:5000
- **📖 Swagger/API**: http://localhost:5000/swagger
- **🏥 Health Check**: http://localhost:5000/health
- **📡 SignalR Hub**: ws://localhost:5000/chathub

## 🔧 Funcionalidades Implementadas

### 📊 API REST (Controllers)
- **GET** `/api/relatorio/progresso/{userId}` - Relatório individual
- **GET** `/api/relatorio/turma/{turmaId}` - Relatório da turma  
- **GET** `/api/relatorio/ferramentas` - Uso de ferramentas
- **GET** `/api/relatorio/exportar/{id}` - Exportação (PDF/Excel/CSV)
- **GET** `/api/relatorio/lista` - Listar relatórios

### ⚡ SignalR (Tempo Real)
- **Chat Interativo** com suporte a grupos
- **Pair Programming** colaborativo
- **Notificações** em tempo real
- **Typing indicators** e status de conexão
- **Reconnection automática**

### 🌐 Razor Pages (Interface Web)
- **Página Principal** com chat integrado
- **Internacionalização** (pt-BR, en, es)
- **Estatísticas** atualizadas em tempo real
- **Interface responsiva** com Bootstrap
- **Integração SignalR** no frontend

### 🗄️ Entity Framework + PostgreSQL
- **Modelos de domínio** completos
- **Migrações automáticas** em desenvolvimento
- **Índices otimizados** para performance
- **Dados iniciais** para demonstração
- **Relacionamentos** configurados

### 🔐 Injeção de Dependência
```csharp
// Registro no Program.cs
builder.Services.AddScoped<IRelatorioService, RelatorioService>();
builder.Services.AddDbContext<TutorDbContext>();
builder.Services.AddSingleton<IConnectionMultiplexer>();

// Uso no Controller
public RelatorioController(
    IRelatorioService relatorioService,
    IStringLocalizer<RelatorioController> localizer)
{
    _relatorioService = relatorioService;
    _localizer = localizer;
}
```

### 🌍 Internacionalização (i18n)
```csharp
// Configuração no Program.cs
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "pt-BR", "en", "es" };
    options.DefaultRequestCulture = new RequestCulture("pt-BR");
    options.SupportedCultures = supportedCultures;
});

// Uso em Razor Pages
@inject IStringLocalizer<IndexModel> Localizer
<h1>@Localizer["TituloPlataforma"]</h1>
```

## 🔧 Configuração

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=tutordb;Username=tutor;Password=copiloto123",
    "Redis": "redis://localhost:6379"
  },
  "JWT": {
    "Secret": "your-super-secret-jwt-key",
    "ExpiryInMinutes": 60
  },
  "CORS": {
    "Origins": "http://localhost:3000,http://localhost:5173"
  }
}
```

### Variáveis de Ambiente
```bash
# Banco de dados
POSTGRES_URL=postgresql://user:pass@host:port/database
REDIS_URL=redis://user:pass@host:port

# Segurança
JWT_SECRET=your-super-secret-jwt-key-change-this
CORS_ORIGINS=http://localhost:3000,http://localhost:5173

# IA Integration
ANTHROPIC_API_KEY=sk-ant-api03-your-key-here
LLAMAINDEX_API_KEY=llx-your-key-here
```

## 🔄 SignalR Hub Methods

### Cliente → Servidor
```javascript
// Enviar mensagem
connection.invoke("SendMessage", {
    content: "Olá!",
    type: "TutorInteraction",
    sessionId: "session-123"
});

// Ingressar em grupo
connection.invoke("JoinGroup", "turma-informatica");

// Iniciar pair programming
connection.invoke("StartPairProgramming", "partner-user-id");
```

### Servidor → Cliente
```javascript
// Receber mensagem
connection.on("ReceiveMessage", (message) => {
    console.log("Nova mensagem:", message);
});

// Status de conexão
connection.on("UserConnected", (user) => {
    console.log("Usuário conectou:", user);
});

// Tutor digitando
connection.on("TutorTyping", () => {
    showTypingIndicator();
});
```

## 📊 Monitoramento

### Health Checks
```bash
# Verificar saúde da aplicação
curl http://localhost:5000/health

# Resposta esperada
{
  "status": "Healthy",
  "checks": {
    "database": "Healthy",
    "redis": "Healthy"
  }
}
```

### Logs Estruturados (Serilog)
```csharp
// Configuração no Program.cs
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/tutor-copiloto-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

// Uso nos serviços
_logger.LogInformation("Gerando relatório para usuário {UserId}", userId);
```

## 🧪 Desenvolvimento

### Executar com Debug
```bash
# Visual Studio Code
F5 (com launch.json configurado)

# Command line
dotnet run --configuration Debug
```

### Hot Reload
```bash
# Reinicialização automática
dotnet watch run

# Ou com HTTPS
dotnet watch run --urls "https://localhost:5001;http://localhost:5000"
```

### Migrações Entity Framework
```bash
# Criar migração
dotnet ef migrations add InitialCreate

# Aplicar migrações
dotnet ef database update

# Reverter migração
dotnet ef database update PreviousMigration
```

## 🚢 Deploy

### Docker (Futuro)
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TutorCopiloto.csproj", "."]
RUN dotnet restore "TutorCopiloto.csproj"
COPY . .
RUN dotnet build "TutorCopiloto.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TutorCopiloto.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TutorCopiloto.dll"]
```

### Kubernetes Integration
```yaml
# Configurar secrets
apiVersion: v1
kind: Secret
metadata:
  name: tutor-copiloto-secrets
data:
  postgres-url: <base64-encoded>
  redis-url: <base64-encoded>
  jwt-secret: <base64-encoded>
```

## 🤝 Integração com Node.js Backend

Este backend ASP.NET Core pode funcionar:

1. **Standalone** - Como alternativa completa ao backend Node.js
2. **Complementar** - Focado em relatórios e análises
3. **Híbrido** - Compartilhando banco PostgreSQL e Redis

### Compartilhamento de Dados
```csharp
// Mesmo esquema de banco do Node.js
public class Sessao
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public DateTime CriadoEm { get; set; }
    // Compatible com o schema Node.js
}
```

## 📚 Tecnologias Utilizadas

- **ASP.NET Core 8.0** - Framework web
- **Entity Framework Core** - ORM
- **PostgreSQL** - Banco principal com pgvector
- **Redis** - Cache e sessões
- **SignalR** - Comunicação tempo real
- **Swagger/OpenAPI** - Documentação API
- **Serilog** - Logging estruturado
- **JWT Bearer** - Autenticação
- **Bootstrap 5** - Interface responsiva

## 🔗 Links Úteis

- [ASP.NET Core Docs](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [SignalR Tutorial](https://docs.microsoft.com/en-us/aspnet/core/signalr/)
- [Swagger Integration](https://docs.microsoft.com/en-us/aspnet/core/tutorials/web-api-help-pages-using-swagger)

---

## 🎯 Próximos Passos

1. **Integração IA** - Conectar com Anthropic Claude
2. **Autenticação** - Sistema completo de usuários
3. **Containerização** - Docker e Kubernetes
4. **Testes** - Unit tests e integration tests
5. **Performance** - Caching avançado e otimizações

**Feito com ❤️ usando ASP.NET Core e C#**
