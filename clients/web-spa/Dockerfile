# Multi-stage build para React SPA
FROM node:18-alpine AS base

# Stage 1: Build da aplicação React
FROM base AS builder
WORKDIR /app

# Copiar dependências
COPY package*.json ./
COPY clients/web-spa/package.json ./clients/web-spa/
COPY shared/schemas/package.json ./shared/schemas/
RUN npm ci

# Copiar código fonte
COPY clients/web-spa/ ./clients/web-spa/
COPY shared/schemas/ ./shared/schemas/
COPY turbo.json ./

# Build da aplicação
RUN npm run build

# Stage 2: Nginx para servir arquivos estáticos
FROM nginx:alpine AS production

# Instalar dumb-init
RUN apk add --no-cache dumb-init

# Copiar arquivos buildados
COPY --from=builder /app/clients/web-spa/dist /usr/share/nginx/html

# Configuração customizada do Nginx
COPY clients/web-spa/nginx.conf /etc/nginx/conf.d/default.conf

# Criar usuário não-root
RUN addgroup --system --gid 1001 nginx-user
RUN adduser --system --uid 1001 --ingroup nginx-user nginx-user

# Ajustar permissões
RUN chown -R nginx-user:nginx-user /usr/share/nginx/html
RUN chown -R nginx-user:nginx-user /var/cache/nginx
RUN chown -R nginx-user:nginx-user /var/log/nginx
RUN chown -R nginx-user:nginx-user /etc/nginx/conf.d

# Configurar nginx para rodar como usuário não-root
RUN sed -i '/user nginx;/d' /etc/nginx/nginx.conf
RUN sed -i 's/listen       80;/listen       8080;/' /etc/nginx/conf.d/default.conf

# Mudar para usuário não-root
USER nginx-user

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:8080/ || exit 1

# Expor porta
EXPOSE 8080

# Comando de execução
ENTRYPOINT ["dumb-init", "--"]
CMD ["nginx", "-g", "daemon off;"]

# Labels
LABEL maintainer="tutor-copiloto@example.com"
LABEL version="1.0.0"
LABEL description="Tutor Copiloto Web SPA"
