# Stage 1: build Angular app
FROM node:20-alpine AS build
WORKDIR /app

# Install deps
COPY bancalite-frontend/package*.json ./
RUN npm ci

# Copy sources and build
COPY bancalite-frontend/ .
# Disable analytics / CI friendly
ENV NG_CLI_ANALYTICS=ci
RUN npm run build -- --configuration production

# Stage 2: serve with Nginx
FROM nginx:1.25-alpine AS runtime

# Copy nginx config with SPA fallback and API proxy
COPY bancalite-frontend/nginx/default.conf /etc/nginx/conf.d/default.conf

# Copy build artifacts
COPY --from=build /app/dist/bancalite-frontend /usr/share/nginx/html

EXPOSE 80

HEALTHCHECK --interval=10s --timeout=5s --retries=5 CMD wget -qO- http://localhost/ || exit 1

CMD ["nginx","-g","daemon off;"]

