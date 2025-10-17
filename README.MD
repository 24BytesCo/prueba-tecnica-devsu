.

## Documentos del Proyecto

- Estimaciones WBS — 80 h: WBS priorizado por épicas y tareas — docs/plan/estimaciones.md
- ERD Movimientos: Diagrama entidad–relación de movimientos — docs/db/movimientos/ERD-movimientos.drawio.png
- ERD Cuentas: Diagrama entidad–relación de cuentas — docs/db/accounts/ERD-cuentas.drawio.png
- ERD Personas/Clientes: Diagrama entidad–relación de clientes — docs/db/clients/ERD-persona-clientes..png
- Especificación de Movimientos: Reglas y flujos de negocio — docs/db/domain/movimientos-spec.md
- Casos de Prueba (Dominio): Escenarios y validaciones de reglas — docs/db/domain/casos-de-prueba.md
- Decision Log: Registro de decisiones de diseño — docs/db/domain/decision-log.md
- WBS (Excel): Matriz WBS editable — docs/plan/WBS.xlsx
- WBS & Roadmap 2025 (TSV): Vista exportada en TSV — docs/plan/Banca Full-Stack WBS & Roadmap 2025 - View 1.tsv

## Docker / Desarrollo Rápido

Este repo incluye `docker-compose.yml` para levantar PostgreSQL. La API se ejecuta con `dotnet run` desde `bancalite-backend`.

### Variables de entorno recomendadas

Configura antes de levantar los servicios (puedes usar un `.env` en la raíz):

```
DB_HOST=localhost
DB_PORT=5432
DB_NAME=bancalite
DB_USER=admin
DB_PASSWORD=Admin1234
```

### Levantar Postgres

```bash
docker compose up -d db
```

La base queda disponible en `localhost:5432`.

### Ejecutar la API

En otra terminal:

```bash
cd bancalite-backend
DOTNET_ENVIRONMENT=Development dotnet run --project src/Bancalite.WebApi
```

- Swagger: `http://localhost:<puerto>/swagger`
- En Development se aplican migraciones y se siembran catálogos/roles/usuario admin

### (Opcional) Incluir la API en docker-compose

Puedes añadir un servicio `api` al `docker-compose.yml` (no incluido por defecto):

```yaml
  api:
    build: ./bancalite-backend
    environment:
      - ConnectionStrings__Default=Host=db;Port=5432;Database=${DB_NAME:-bancalite};Username=${DB_USER};Password=${DB_PASSWORD};Pooling=true
      - DOTNET_ENVIRONMENT=Development
    depends_on:
      db:
        condition: service_healthy
    ports:
      - "5208:8080"
```

Nota: para producción, define `JWT:Key`, `JWT:Issuer`, `JWT:Audience` y `Smtp:*` mediante variables de entorno o secretos seguros (no commitear).
