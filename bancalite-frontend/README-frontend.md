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
- Estado global con NgRx preparado (slices mínimos, sin lógica).
- Interceptores y guards están como stubs para completar en siguientes historias.

