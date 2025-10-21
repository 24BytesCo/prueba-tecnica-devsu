# Bancalite — Clean Architecture (Full-Stack)

Proyecto de banca simplificada implementado con **Clean Architecture**.

- **Backend (.NET 9):** Domain, Application (CQRS + MediatR), Infrastructure (EF Core + Identity + PDF) y WebApi.
- **Frontend (Angular 18):** NgRx para la gestión de estado.
- **Infraestructura (Docker / Compose):** Contenedores para la base de datos, API y Frontend.

---

## 🚀 Guía de Despliegue Rápido (Recomendado)

Este método es el más rápido para levantar la aplicación, ya que utiliza las imágenes públicas pre-construidas desde Docker Hub. No necesitas clonar el repositorio.

### Requisitos

- Docker Desktop o Docker Engine con Docker Compose v2.
- Conexión a internet para descargar las imágenes.
- Puertos `5432`, `8080` y `4200` libres (o puedes cambiarlos en `.env`).

### 1. Crear una carpeta de trabajo

```bash
mkdir bancalite-stack
cd bancalite-stack
```

### 2. Crear el archivo .env

Crea un archivo llamado `.env` y pega el siguiente contenido. Recuerda cambiar los valores sensibles por unos seguros para producción.

```env
# Puertos de la aplicación
API_PORT=8080
WEB_PORT=4200

# Base de Datos (PostgreSQL)
DB_USER=admin
DB_PASSWORD=Admin1234
DB_NAME=bancalite
DB_PORT=5432

# Configuración JWT (JSON Web Token)
# IMPORTANTE: usa una clave MUY larga y segura en producción.
JWT__Key=super_larga_clave_secreta_para_fines_de_demo_cámbiala
JWT__Issuer=Bancalite.WebApi
JWT__Audience=Bancalite.Client
JWT__ExpiresMinutes=60

# Credenciales del Administrador por Defecto
ADMIN_USERNAME=admin
ADMIN_EMAIL=admin@example.com
ADMIN_PASSWORD=CambiaEstaClave123!

# Entorno de la Aplicación (Development | Production)
ASPNETCORE_ENVIRONMENT=Development
```

Nota importante:
- Se recomienda dejar `ASPNETCORE_ENVIRONMENT=Development` para que la migración de base de datos se ejecute automáticamente al iniciar la API.
- Si necesitas usar `Production`, aplica manualmente el SQL: `docs/db/bancalite_prod_data.sql`.

Configuración del backend:
- La API lee su configuración por defecto desde `appsettings.Development.json` (por ejemplo: `ConnectionStrings`, `JWT`, `SMTP`).
- En producción, replica/ajusta esa configuración en `appsettings.Production.json` o usa variables de entorno equivalentes en el `docker-compose.yml`.

### 3. Crear el archivo docker-compose.yml

Crea un archivo llamado `docker-compose.yml` con este contenido:

```yaml
services:
  db:
    image: postgres:14.3
    restart: always
    environment:
      POSTGRES_USER: ${DB_USER}
      POSTGRES_PASSWORD: ${DB_PASSWORD}
      POSTGRES_DB: ${DB_NAME}
    ports:
      - "${DB_PORT}:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${DB_USER} -d ${DB_NAME}"]
      interval: 10s
      timeout: 5s
      retries: 5

  api:
    image: 24bytes/bancalite-api:latest
    restart: always
    ports:
      - "${API_PORT}:8080"
    depends_on:
      db:
        condition: service_healthy
    environment:
      - ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT}
      - ConnectionStrings__Default=Server=db;Port=5432;User Id=${DB_USER};Password=${DB_PASSWORD};Database=${DB_NAME}
      - JWT__Key=${JWT__Key}
      - JWT__Issuer=${JWT__Issuer}
      - JWT__Audience=${JWT__Audience}
      - JWT__ExpiresMinutes=${JWT__ExpiresMinutes}
      - ADMIN_USERNAME=${ADMIN_USERNAME}
      - ADMIN_EMAIL=${ADMIN_EMAIL}
      - ADMIN_PASSWORD=${ADMIN_PASSWORD}

  web:
    image: 24bytes/bancalite-web:latest
    restart: always
    ports:
      - "${WEB_PORT}:80"
    depends_on:
      - api

volumes:
  postgres_data:
```

### 4. Descargar y levantar el stack

```bash
# Descarga las imágenes más recientes
docker compose pull

# Levanta todos los servicios en segundo plano
docker compose up -d
```

#### Solo para producción: inicializar base de datos


Si configuraste `ASPNETCORE_ENVIRONMENT=Production`, la API no aplica migraciones automáticamente. Con los contenedores arriba, ejecuta el SQL que replica la estructura y carga datos iniciales:

**Nota:** El sql debes descargarlo del repositorio en la ubicación `docs/db/bancalite_prod_data.sql` y ponerlo en la raiz de la carpeta local `bancalite-stack`

```bash

# Copiando el archivo al contenedor y ejecutándolo
docker compose cp bancalite_prod_data.sql db:/tmp/bancalite_prod_data.sql
docker compose exec db sh -lc 'PGPASSWORD="$POSTGRES_PASSWORD" psql -h localhost -U "$POSTGRES_USER" -d "$POSTGRES_DB" -f /tmp/bancalite_prod_data.sql'
```


### 5. Acceder y administrar

- Web (Frontend): http://localhost:4200
- API (Backend): http://localhost:8080/swagger
- Para detener los servicios: `docker compose down`
- Para detener y eliminar los datos de la DB: `docker compose down -v`

## 💻 Despliegue para Desarrollo (Construyendo las Imágenes)

Usa este método si clonaste el repositorio y quieres construir las imágenes de Docker localmente a partir del código fuente.

### Clona el repositorio:

```bash
git clone https://github.com/tu-usuario/bancalite-app.git
cd bancalite-app
```

### Configura las variables de entorno:

Copia el archivo de ejemplo y renómbralo a `.env`.

```bash
cp .env.example .env
```

### Levanta los servicios:

Este comando construirá las imágenes y levantará los contenedores.

```bash
docker compose up -d --build
```

### Verifica que todo funcione:

- API Health Check: http://localhost:8080/health
- Frontend: http://localhost:8081

## 🏛️ Arquitectura (Clean Architecture)

- Domain: Contiene las entidades y la lógica de negocio principal.
- Application: Define los casos de uso (Commands/Queries), validaciones y puertos.
- Infrastructure: Implementa detalles externos como la base de datos (EF Core), autenticación (Identity) y generación de PDFs (QuestPDF).
- WebApi: Expone los endpoints REST e integra Swagger.
- Frontend (Angular): Organizado en módulos por funcionalidad, utilizando NgRx para el estado.

## 🔄 CI/CD con GitHub Actions

El repositorio incluye un workflow (`.github/workflows/ci-docker.yml`) que automatiza:

- En Pull Request: Ejecución de tests de .NET y construcción de imágenes.
- En Push a main: Publicación de imágenes en GHCR y Docker Hub.

## 📂 Estructura del Repositorio

- `bancalite-backend/`: Solución .NET.
- `bancalite-frontend/`: Aplicación Angular 18 + NgRx.
- `docs/`: Documentación, diagramas ERD y decisiones de arquitectura.
- `docker-compose.yml`: Orquestador principal de servicios.

## 📚 Documentación y Artefactos

- Planificación y estimaciones: `docs/plan/estimaciones.md`
- Diagramas ERD: `docs/db/`
- Reglas de dominio y casos de prueba: `docs/db/domain/movimientos-spec.md`
- Colección de Postman: `Bancalite.postman_collection.json`
