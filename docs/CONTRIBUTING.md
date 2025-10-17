# Guía de Contribución · GitFlow + Conventional Commits (ES)

Esta guía estandariza **ramas, commits, PRs** y versionado. Se basa en **GitFlow simplificado** y **Conventional Commits**, incluyendo **IDs de Historia/Tarea**.

---

## 1) Ramas base
- `main`: versiones **liberadas** y estables.
- `develop`: integración de trabajo listo para **QA**.

> **Prohibido** commitear directo a `main`.

---

## 2) Tipos de ramas (GitFlow simplificado)
- `feature/*`: nuevas historias/tareas.
- `release/*`: estabilización previa a release (bugfix menor, docs).
- `hotfix/*`: correcciones urgentes sobre `main`.

**Reglas generales**
- `feature/*` → PR requerido → `develop`.
- `release/*` → PR → `main` (**y** merge back a `develop`).
- `hotfix/*` → PR → `main` (**y** merge back a `develop`).
- Estrategia recomendada: **Squash & merge** (historial limpio).

---

## 3) Nomenclatura de ramas (con IDs)

**Formato (obligatorio)**  
```
<tipo>/<módulo>/<US_ID>-<Task_ID>-<slug-descriptivo>
```
- **Tipos válidos**: `feature`, `hotfix`, `release`, `chore`, `spike`.
- **módulo** (scope de negocio): `auth`, `clientes`, `cuentas`, `movimientos`, `reportes`, `admin`, `infra`, `db`, `docs`, `ci`, `pm`, etc.
- **US_ID**: identificador de historia (p. ej., `US-Auth-01`, `SET-01`).
- **Task_ID**: identificador de tarea (p. ej., `A1.1`, `D1.4`, `T-S0.2`).
- **slug-descriptivo**: descripción corta en **kebab-case**.

**Ejemplos**  
```
feature/auth/US-Auth-01-A1.1-estrategia-de-tokens
feature/movimientos/US-Mov-01-D1.4-endpoints-movimientos
hotfix/reportes/BUG-123-fix-totales-en-pdf
```

Comando (ejemplo):  
```
git checkout -b feature/auth/US-Auth-01-A1.1-estrategia-de-tokens
```

---

## 4) Mensajes de commit (Conventional Commits, en español)

**Formato (obligatorio)**  
```
<tipo>(<scope>): <resumen en imperativo> [<US_ID>][<Task_ID>]

<cuerpo opcional, qué y por qué>

Tarea: <enlaces o ids adicionales>
```
- **Tipos**: `feat`, `fix`, `docs`, `refactor`, `test`, `perf`, `build`, `ci`, `chore`, `style`, `revert`.
- **Scopes (sugeridos)**: `auth`, `clientes`, `cuentas`, `movimientos`, `reportes`, `admin`, `infra`, `db`, `docs`, `ci`.

**Reglas**
- Título ≤ **72** caracteres, en **imperativo** (“definir”, “agregar”, “corregir”).  
- Incluir **[US_ID][Task_ID]** siempre.  
- Commits **atómicos** (un cambio coherente por commit).

**Ejemplo**  
```
docs(auth): definir estrategia de tokens JWT (access/refresh) [US-Auth-01][A1.1]

- Access=15m, Refresh=14d en cookie HTTP-Only/Secure/SameSite
- Rotación y revocación de refresh; skew de reloj; lineamientos CORS
Tarea: ADR-001, Matriz de permisos
```

---

## 5) Pull Requests (PR)

**Título**  
```
<tipo>(<scope>): <US_ID> <Task_ID> — <resumen>
```
Ejemplo:  
```
docs(pm): SET-01 T-S0.2 — Guía GitFlow y Conventional Commits + plantillas
```

**Checklist del PR**
- [ ] Convenciones de **rama/commit/PR** cumplidas (IDs incluidos).
- [ ] Build/Tests/Linter **OK** (checks en verde).
- [ ] ≥ **1** aprobación de revisor.
- [ ] Sin secretos ni datos sensibles.
- [ ] Documentación actualizada (README/ADR/OpenAPI).

---

## 6) Issues y trazabilidad
- Vincular PR ↔ Issue: `Closes #123` / `Refs #123`.
- Mantener `US_ID` y `Task_ID` en **rama, commits y PR**.

---

## 7) Versionado Semántico (SemVer)
- `MAJOR.MINOR.PATCH` (tags: `v1.2.0`).
  - `feat` → MINOR (si no rompe compatibilidad).
  - `fix` → PATCH.
  - `BREAKING CHANGE` → MAJOR.

**Crear tag**  
```
git tag -a v1.2.0 -m "Release 1.2.0"
git push origin v1.2.0
```

---

## 8) (Opcional) Automatización
- **commitlint**: valida el formato (`<tipo>(<scope>): ... [US][Task]`).
- **husky**: hooks `pre-commit` / `pre-push` para lint/tests.
- **cz/commitizen** o **convco**: asistentes interactivos.

Ejemplo mínimo `commitlint.config.js` en la raíz del repo:
```js
/** @type {import('@commitlint/types').UserConfig} */
export default {
  extends: ['@commitlint/config-conventional'],
  rules: {
    'header-max-length': [2, 'always', 72],
    'type-enum': [2, 'always', [
      'feat','fix','docs','refactor','test','perf','build','ci','chore','style','revert'
    ]],
    'scope-case': [2, 'always', 'lower-case'],
    // Nota: Para exigir [US_ID][Task_ID] en el header, usar plugin o validación extra en CI.
  },
};
```

Plantilla de mensaje `.gitmessage.txt` (opcional):
```
<tipo>(<scope>): <resumen> [<US_ID>][<Task_ID>]

<cuerpo: qué y por qué>
Tarea: <referencias>
```
Configurar:
```
git config commit.template .gitmessage.txt
```

---

## 9) Protección de ramas (recomendado)
- `main` / `develop` **protegidas**: PR requerido, revisiones, checks CI.
- Bloquear `--force` sobre ramas protegidas.
- Reglas de aprobación: mínimo 1 revisor.
