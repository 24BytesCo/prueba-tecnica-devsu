# Bancalite Frontend (Angular 18)

Esqueleto base para el frontend que consumirá el backend de Bancalite. Incluye NgModules, lazy‑loading, ESLint + Prettier, Jest y NgRx preparados.

## Requisitos
- Node 18+
- Angular CLI 18 (`npm i -g @angular/cli`)

## Instalación
```bash
cd bancalite-frontend
npm install
```

## Desarrollo
```bash
npm start
# Navega a http://localhost:4200 (proxy /api -> http://localhost:8080)
```

## Lint y formato
```bash
npm run lint
npm run lint:fix
npm run format
npm run format:check
```

## Pruebas
```bash
npm test
npm run test:coverage  # umbral global 80%
```

## Build producción
```bash
npm run build
```

## Estructura
- `src/app/core`: servicios singleton, interceptores, guards (cross‑cutting)
- `src/app/shared`: componentes/pipes/directivas reutilizables
- `src/app/layout`: Header, Sidebar, Footer y layout protegido
- `src/app/features`: módulos por dominio (auth, dashboard, clientes, cuentas, movimientos, reportes) con rutas lazy
- `src/environments`: configuración por entorno

## Notas
- Sin frameworks de UI (solo SCSS propio).
- Estado global con NgRx para autenticación (acciones/efectos/reducer/selectores básicos).
- Interceptores y guards están como stubs para completar en siguientes historias.

## Consumo de API
- Contrato de éxito: ApiResult<T> `{ isSuccess, datos, error }` → los servicios mapean a `datos`.
- Contrato de error: ProblemDetails `{ title, status, detail }` → interceptor muestra `detail` con SweetAlert2 y maneja 401 redirigiendo al login.

## Catálogos (para formularios)
- Clientes usa los endpoints de catálogo del backend para poblar selects:
  - `GET /api/catalogos/generos`
  - `GET /api/catalogos/tipos-documento`
  - Servicio: `CatalogosService` (core/services).

## Estilos (mini librería)
- Clases globales en `src/styles.scss`:
  - Botones: `.btn`, `.btn-primary`, `.btn-ghost`
  - Inputs: `.input`, `.select`
  - Formularios: `.form`, `.form-grid`, `.form-field`, `.error`, `.hint`
  - Tabla liviana: `.table`, `.thead`, `.rowt`

## Buscador de clientes (nombres/documento)
- Componente: `src/app/features/clientes/pages/clientes-list-page.component.ts`.
- Servicio: `src/app/core/services/clientes.service.ts`.
- Comportamiento: si el término es numérico (solo dígitos) se envía `numeroDocumento=<término>`, si no, `nombres=<término>`.
  - Evita combinar ambos filtros para no forzar un AND que vacíe resultados.
  - En backend, `nombres` usa contiene; `numeroDocumento` usa prefijo (StartsWith).

## Loader global y toasts de éxito
- Loader overlay global que cubre la app mientras hay peticiones HTTP en curso.
  - Servicio: `src/app/core/services/loader.service.ts`.
  - Componente: `src/app/shared/components/loader/loader.component.ts` (insertado en el layout protegido).
- Interceptor: `src/app/core/interceptors/loader.interceptor.ts`.
  - Muestra loader para todas las requests.
  - Muestra toast SweetAlert2 (top‑end) solo en respuestas exitosas de `POST/PUT/PATCH/DELETE`.
  - Los `GET` no generan toast. Los errores se tratan en `ErrorInterceptor`.

## Edición del Número de Documento
- En creación se puede editar; en edición queda inhabilitado (solo lectura).
  - Página standalone: `src/app/features/clientes/pages/clientes-form-page.component.ts`.
  - Modal desde lista: `src/app/features/clientes/pages/clientes-list-page.component.ts` (deshabilita al editar y habilita al crear).
  - Se usa `getRawValue()` al guardar para incluir campos deshabilitados.

## Proxy en desarrollo
- `environment.apiBaseUrl = '/api'` y `proxy.conf.json` redirige a `http://localhost:8080`.
- Ver: `angular.json` (`serve.proxyConfig`) y `bancalite-frontend/proxy.conf.json`.
- ## Movimientos (UI)
- Ruta: `/movimientos`.
- Búsqueda por número de cuenta, titular y cédula con sugerencias (autocomplete). Al seleccionar, se fija el número y se consulta.
- Filtros: fechas (onChange dispara búsqueda), tipo CRE/DEB (filtro local) y texto en descripción.
- Registrar movimiento: `/movimientos/nuevo` con buscador de cuenta + select de tipo (catálogo), monto, descripción e idempotency key.
- Post‑creación redirige a `/movimientos?numeroCuenta=...`.

- ## Reportes (UI)
- Ruta: `/reportes`.
- Modo "Por cuenta" (autocomplete por número/titular/cédula) y "Por cliente" (autocomplete por nombre/documento).
- Fechas con cambio inmediato; render de resumen y tabla.
- Exportaciones:
  - Botón "Descargar JSON": GET `/api/reportes/json` como archivo.
  - Botón "Descargar PDF (Base64)": obtiene base64 y descarga.

## Docker (Nginx)
El frontend cuenta con un `Dockerfile` multi-stage y una configuraci�n de Nginx para servir la SPA y proxyear `/api` hacia el backend.

```bash
docker build -t bancalite-web -f bancalite-frontend/Dockerfile .
docker run --rm -p 8081:80 bancalite-web
# http://localhost:8081
```

Con `docker-compose.yml` en la ra�z puedes levantar DB + API + Front con un solo comando:

```bash
docker compose up -d --build
```
