# Multi-stage build para otimização
FROM node:18-alpine AS base

# Stage 1: Instalar dependências
FROM base AS deps
WORKDIR /app
COPY package*.json ./
COPY turbo.json ./
COPY shared/ ./shared/
RUN npm ci --only=production

# Stage 2: Build da aplicação
FROM base AS builder
WORKDIR /app
COPY . .
COPY --from=deps /app/node_modules ./node_modules
RUN npm run build

# Stage 3: Orquestrador
FROM base AS orchestrator
WORKDIR /app

# Criar usuário não-root para segurança
RUN addgroup --system --gid 1001 nodejs
RUN adduser --system --uid 1001 tutor-copiloto

# Copiar arquivos de produção
COPY --from=builder --chown=tutor-copiloto:nodejs /app/backend/orchestrator/dist ./dist
COPY --from=builder --chown=tutor-copiloto:nodejs /app/backend/orchestrator/package.json ./
COPY --from=deps --chown=tutor-copiloto:nodejs /app/node_modules ./node_modules

# Instalar dumb-init para signal handling
RUN apk add --no-cache dumb-init

# Configurar usuário e permissions
USER tutor-copiloto

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD node -e "require('http').get('http://localhost:8080/health', (res) => { process.exit(res.statusCode === 200 ? 0 : 1) })"

# Expor porta
EXPOSE 8080

# Comando de execução com signal handling
ENTRYPOINT ["dumb-init", "--"]
CMD ["node", "dist/index.js"]

# Labels para metadata
LABEL maintainer="tutor-copiloto@example.com"
LABEL version="1.0.0"
LABEL description="Tutor Copiloto Orchestrator Service"
