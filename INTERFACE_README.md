# 🚀 Tutor Copiloto - Interface Integrada

## Visão Geral

A nova interface integrada do Tutor Copiloto oferece uma experiência centralizada para acessar todas as funcionalidades de IA do sistema. Com uma navegação intuitiva e design moderno, você pode facilmente alternar entre diferentes recursos.

## Funcionalidades Disponíveis

### 📊 Dashboard Principal
- **Status do Sistema**: Monitoramento em tempo real dos serviços (.NET, Node.js, Banco de dados, IA)
- **Estatísticas Gerais**: Resumo das atividades (chats, análises, buscas, usuários ativos)
- **Ações Rápidas**: Acesso direto às funcionalidades principais
- **Atividades Recentes**: Log das últimas ações realizadas

### 💬 Chat Inteligente
- **Modelos Suportados**: Claude (Anthropic), GPT (OpenAI), Codestral
- **Interface Moderna**: Chat em tempo real com formatação rica
- **Histórico**: Mantém o contexto da conversa
- **Seleção de Modelo**: Alterne entre diferentes modelos de IA

### 🧠 Análise Inteligente com Semantic Kernel
- **Tipos de Análise**:
  - 📄 Análise de Documento
  - 💻 Análise de Código
  - 😊 Análise de Sentimento
  - 📋 Resumo Inteligente
  - 🏷️ Extração de Palavras-chave
- **Upload de Arquivos**: Suporte para .txt, .md, .js, .py, .cs, etc.
- **Resultados Estruturados**: Apresentação clara dos resultados

### 🔍 Busca Semântica com LlamaIndex
- **Busca Inteligente**: Busca por similaridade semântica
- **Explicações**: Geração de explicações detalhadas
- **Histórico de Buscas**: Rastreamento das consultas realizadas
- **Buscas Rápidas**: Exemplos prontos para teste

### 📋 Sistema de Relatórios
- **Geração Automática**: Relatórios de uso, performance, erros, IA e usuários
- **Download**: Exportação em formato acessível
- **Histórico**: Gerenciamento de relatórios anteriores
- **Visualização**: Preview do conteúdo antes do download

### ⚙️ Configurações do Sistema
- **Aparência**: Tema (claro/escuro) e idioma
- **Comportamento**: Notificações, salvamento automático, telemetria
- **Performance**: Configuração de timeouts e limites de arquivo
- **Informações do Sistema**: Versões e status dos componentes
- **Import/Export**: Backup e restauração de configurações

## Como Usar

### 1. **Navegação**
- Use a barra lateral esquerda para navegar entre as seções
- Cada seção possui uma interface dedicada e intuitiva
- O status do sistema é mostrado em tempo real

### 2. **Chat Inteligente**
- Selecione o modelo de IA desejado
- Digite sua mensagem e pressione Enter ou clique em enviar
- O histórico da conversa é mantido automaticamente

### 3. **Análise Inteligente**
- Escolha o tipo de análise desejado
- Cole o texto ou faça upload de um arquivo
- Clique em "Iniciar Análise" para processar
- Visualize os resultados estruturados

### 4. **Busca Semântica**
- Digite sua consulta de busca
- Escolha entre busca semântica ou explicação
- Use as buscas rápidas para exemplos
- O histórico mantém suas consultas anteriores

### 5. **Relatórios**
- Selecione o tipo de relatório desejado
- Clique em "Gerar Relatório" para criar um novo
- Visualize o histórico de relatórios gerados
- Baixe os relatórios em formato texto

### 6. **Configurações**
- Personalize a aparência e comportamento
- Configure limites de performance
- Importe/exporte suas configurações
- Visualize informações técnicas do sistema

## Integração com Backends

### Backend .NET (porta 5000/7284)
- **Semantic Kernel**: Análises inteligentes e processamento de IA
- **Entity Framework**: Gerenciamento de dados
- **SignalR**: Comunicação em tempo real
- **Autenticação**: Controle de acesso

### Backend Node.js (porta 8080)
- **LlamaIndex**: Busca semântica e RAG
- **Express/Fastify**: API REST
- **OpenAI Integration**: Modelos de linguagem
- **Document Processing**: Indexação e busca

## Tecnologias Utilizadas

### Frontend
- **React 18**: Framework JavaScript moderno
- **Vite**: Build tool rápido e otimizado
- **CSS-in-JS**: Estilização moderna e responsiva
- **ES6+**: JavaScript moderno com módulos

### Backend
- **.NET 8**: Framework robusto para aplicações empresariais
- **Node.js**: Runtime JavaScript server-side
- **PostgreSQL**: Banco de dados relacional
- **Redis**: Cache e armazenamento em memória

### IA e ML
- **Semantic Kernel**: Framework para integração de IA
- **LlamaIndex**: Busca semântica e RAG
- **OpenAI API**: Modelos de linguagem avançados
- **Anthropic Claude**: IA conversacional

## Desenvolvimento

### Pré-requisitos
- Node.js 18+
- .NET 8 SDK
- PostgreSQL
- Redis (opcional)

### Instalação
```bash
# Frontend
cd web-spa
npm install
npm start

# Backend .NET
cd dotnet-backend
dotnet run

# Backend Node.js
cd backend/orchestrator
npm install
npm start
```

### Build
```bash
# Frontend
cd web-spa
npm run build

# Backend .NET
cd dotnet-backend
dotnet publish -c Release
```

## Próximos Passos

- [ ] Integração com mais modelos de IA
- [ ] Sistema de autenticação avançado
- [ ] Dashboard com gráficos interativos
- [ ] Exportação de relatórios em PDF
- [ ] Modo offline para funcionalidades básicas
- [ ] Integração com APIs externas
- [ ] Sistema de plugins extensível

## Suporte

Para dúvidas ou problemas, consulte:
- 📚 Documentação técnica
- 🐛 Issues no repositório
- 💬 Chat de suporte
- 📧 Email: suporte@tutorcopiloto.com

---

**Tutor Copiloto** - Transformando o aprendizado com Inteligência Artificial 🤖📚
