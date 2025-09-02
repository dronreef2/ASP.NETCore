# Deploy Automático no GitHub Pages via GitHub Actions

Este repositório está configurado para fazer deploy automático no **GitHub Pages** sempre que houver push na branch `main`.

## 🚀 Como Configurar

### 1. Habilitar GitHub Pages no Repositório

1. Acesse **Settings** do seu repositório
2. Role para baixo até **Pages**
3. Em **Source**, selecione **GitHub Actions**

### 2. Deploy Automático

Após habilitar o GitHub Pages, todo push na branch `main` irá:

1. ✅ **Build** da aplicação .NET
2. ✅ **Testes** automatizados
3. ✅ **Geração** de página HTML estática
4. ✅ **Deploy** automático no GitHub Pages

### 3. Acessar a Aplicação

Após o deploy, sua aplicação estará disponível em:
```
https://dronreef2.github.io/ASP.NETCore
```

## 📋 Workflow do GitHub Actions

O workflow `.github/workflows/github-pages-deploy.yml` executa:

- **Build** da aplicação ASP.NET Core
- **Testes** automatizados
- **Geração** de página HTML com informações da API
- **Deploy** no GitHub Pages

## 🔧 Configuração Manual (Alternativa)

Se preferir deploy manual:

```bash
# Build da aplicação
dotnet build src/Web/API/TutorCopiloto.csproj --configuration Release

# Testes
dotnet test --no-build --configuration Release

# Publish
dotnet publish src/Web/API/TutorCopiloto.csproj --configuration Release --output ./publish
```

## 📊 Monitoramento

- **GitHub Actions**: Acompanhe os deploys em **Actions** tab
- **GitHub Pages**: Status em **Settings > Pages**
- **URL da Aplicação**: https://dronreef2.github.io/ASP.NETCore

---

**🎉 Pronto! Seu deploy automático no GitHub Pages está configurado!**
