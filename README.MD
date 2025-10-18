## Bancalite — Clean Architecture (Full‑Stack)

Proyecto de banca simplificada implementado con Clean Architecture:

- Backend (.NET 9)
  - Domain, Application (CQRS + MediatR), Infrastructure (EF Core + Identity + PDF), WebApi
- Frontend (Angular 18) + NgRx
- Infra (Docker / Compose) para DB + API + Front

## Documentación y artefactos

- Planificación y estimaciones — docs/plan/estimaciones.md
- ERDs: 
  - docs/db/movimientos/ERD-movimientos.drawio.png
  - docs/db/accounts/ERD-cuentas.drawio.png
  - docs/db/clients/ERD-persona-clientes..png
- Dominio: reglas y casos — docs/db/domain/movimientos-spec.md, docs/db/domain/casos-de-prueba.md
- Decision Log — docs/db/domain/decision-log.md
- Postman Collection — Bancalite.postman_collection.json

## Arquitectura (Clean Architecture)

- Domain: entidades y lógica básica (Cuentas, Movimientos, Cliente, Persona)
- Application: casos de uso (Commands/Queries), validaciones (FluentValidation), puertos
- Infrastructure: EF Core (PostgreSQL), Identity, rendering de PDF (QuestPDF), seeders
- WebApi: endpoints REST, swagger, manejo de errores (ProblemDetails)
- Frontend (Angular): módulos por feature, NgRx para auth y estado

## Getting Started — Docker (recomendado)

Prerrequisitos: Docker Desktop 4.x / Compose v2, puerto libre 8080 (API) y 8081 (Front).

1) Configura variables en `.env` (o usa `.env.example`):

```
DB_NAME=bancalite
DB_USER=admin
DB_PASSWORD=Admin1234
API_PORT=8080
WEB_PORT=8081
```

2) Levanta todo (DB + API + Front):

```
docker compose up -d --build
```

3) Verifica salud y acceso:

- API health: http://localhost:8080/health
- Swagger API: http://localhost:8080/swagger
- Frontend: http://localhost:8081

El Nginx del front hace proxy de `/api/*` → `api:8080`, por lo que `environment.apiBaseUrl='/api'` funciona sin CORS.

Semillas (dev): se crean catálogos, rol Admin y usuario base al iniciar (ver Infrastructure README para detalles).

## Ejecución local (sin Docker)

DB
```
docker compose up -d db
```

API
```
cd bancalite-backend/src/Bancalite.WebApi
dotnet run --urls http://localhost:8080
```

Front
```
cd bancalite-frontend
npm install
npm start
# http://localhost:4200 (proxy /api -> http://localhost:8080)
```

## CI/CD (GitHub Actions) + Imágenes Docker

Workflow: `.github/workflows/ci-docker.yml`

- En pull_request: ejecuta tests .NET y build de imagen (sin push)
- En push a `main`: build y push de imagen de la app a GHCR y Docker Hub

Imágenes publicadas (API):

- GHCR: `ghcr.io/<owner>/<repo>-app:latest` y tags (sha, semver, main)
- Docker Hub: `docker.io/${DOCKERHUB_USERNAME}/bancalite-app:latest`

Si publicas Front como imagen, puedes extender el workflow para construir `bancalite-frontend/Dockerfile` y publicar `bancalite-web`.

## Estructura del repo

- `bancalite-backend/` — solución .NET (Domain, Application, Infrastructure, WebApi)
- `bancalite-frontend/` — Angular 18 + NgRx
- `docs/` — ERDs, reglas de dominio, WBS, decisiones
- `docker-compose.yml` — DB + API + Front

## Enlaces rápidos

- Backend README: bancalite-backend/README.md
- Frontend README: bancalite-frontend/README-frontend.md
- Colección Postman: Bancalite.postman_collection.json

## Notas de producción

- Configurar variables de JWT en la API (`JWT__Key`, `JWT__Issuer`, `JWT__Audience`)
- Externalizar cadena de conexión `ConnectionStrings__Default`
- Activar logs y trazabilidad según entorno
