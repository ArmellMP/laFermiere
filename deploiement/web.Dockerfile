# Image du site : l'application React compilée, servie par Caddy (https automatique).
# Construite depuis la racine du dépôt (voir docker-compose.yml).

# 1. Compilation de l'application (tests de types compris)
FROM node:22-alpine AS construction
WORKDIR /front
COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci
COPY frontend/ ./
RUN npm run build

# 2. Caddy + les fichiers compilés
FROM caddy:2-alpine
COPY deploiement/Caddyfile /etc/caddy/Caddyfile
COPY --from=construction /front/dist /srv
