# Estimaciones WBS — PM-01 | Planificación y estimación

**Proyecto:** Banca Full-Stack (Cliente, Cuenta, Movimiento, Reportes)  
**Fecha:** 2025-10-15  
**Versión:** 1.2  
**Alcance base:** Según enunciado del ejercicio (API REST + Front web, CRUDs, reportes JSON/PDF, pruebas, Docker, Postman).  
**Modalidad:** 1 persona full‑stack, capacidad total de 80 horas (no se estiman días, solo horas).

---

## 1. Supuestos y lineamientos

- **Tecnologías:** .NET 8 + EF Core + SQL Server (o Postgres), Angular 16+ (sin UI kits prefabricados), Jest/RTL para FE, xUnit/NUnit para BE.  
- **Arquitectura:** Clean Architecture (capas: Domain, Application, Infrastructure, API) y principios SOLID.  
- **Pruebas mínimas:** 2–3 unit tests por endpoint/flujo crítico (crear movimiento y reporte).  
- **Infra:** Docker (API, DB, Front), `docker-compose` + README con pasos de ejecución.  
- **Reporte:** Endpoint de estado de cuenta por rango de fechas y cliente (JSON y PDF/base64).  
- **Reglas de dominio clave:**  
  - Débito negativo, crédito positivo.  
  - Sin sobregiro (bloquear si saldo insuficiente).  
  - Límite diario de retiro (tope 1000) con mensaje “Cupo diario excedido”.  
- **No funcionales** (alcance mínimo): logs, manejo de excepciones, validaciones DTO, semántica HTTP.

---

## 2. Equipo y capacidad

- **Equipo:** 1 persona full‑stack.  
- **Capacidad total:** 80 horas.  
- **Distribución orientativa:** BE 50%, FE 40%, DevOps/Docs/QA 10%.

> Nota: Las tareas incluyen micro‑buffers locales; no se agrega buffer global. El objetivo es completar el alcance base dentro de 80 h.

---

## 3. WBS priorizado para 1 persona (80 h)

### EPIC-0 — Setup & Base de Proyecto

| ID     | Tarea                                                                      | Est. (h) |
|--------|----------------------------------------------------------------------------|----------|
| `HU-Setup-01 &#124; T-S1.1` | Repo inicial + convenciones + esqueleto Clean Architecture | 3        |
| `HU-Setup-02 &#124; T-S1.2` | Config. EF Core + conexión + migración inicial             | 3        |
| `HU-Setup-03 &#124; T-S1.3` | Angular workspace + routing + layout base (sin framework CSS) | 2        |
| `HU-Auth-01 &#124; T-A1.1` | Autenticación — Estrategia JWT + cookies                    | 2        |
| **Subtotal** |                                                                        | **10**   |

---

### EPIC-A — Clientes

| ID      | Tarea                                                                     | Est. (h) |
|---------|---------------------------------------------------------------------------|----------|
| `HU-Cli-01 &#124; T-B1.2` | Modelo de BD — ERD + Migración                               | 2        |
| `HU-Cli-02 &#124; T-B1.3` | Persistencia — Repositorios/Servicios + Mapeos               | 3        |
| `HU-Cli-03 &#124; T-B1.4` | API — CRUD Clientes + Swagger                                | 3        |
| `HU-Cli-04 &#124; T-B1.5` | Pantallas de Interfaz — UI Clientes                          | 3        |
| `HU-Cli-05 &#124; T-B1.6` | Pruebas — Unit/FE + Postman (Clientes)                       | 1        |
| **Subtotal** |                                                                       | **12**   |

---

### EPIC-C — Cuentas

| ID      | Tarea                                                                     | Est. (h) |
|---------|---------------------------------------------------------------------------|----------|
| `HU-Cta-02 &#124; T-C1.1` | Entidades + reglas (estado activo, tipo de cuenta)           | 2        |
| `HU-Cta-03 &#124; T-C1.2` | Persistencia (repo/servicios) + mapeos                       | 3        |
| `HU-Cta-04 &#124; T-C1.4` | API — CRUD Cuentas                                           | 3        |
| `HU-Cta-01 &#124; T-C1.3` | Pantallas de Interfaz — UI Cuentas                           | 3        |
| `HU-Cta-05 &#124; T-C1.5` | Pruebas — Cuentas + Postman                                  | 1        |
| **Subtotal** |                                                                       | **12**   |

---

### EPIC-D — Movimientos

| ID      | Tarea                                                                     | Est. (h) |
|---------|---------------------------------------------------------------------------|----------|
| `HU-Mov-01 &#124; T-D1.1` | Modelo + reglas: signos, sin sobregiro, tope diario          | 4        |
| `HU-Mov-02 &#124; T-D1.2` | Persistencia + consultas auxiliares (saldo, acumulado día)   | 3        |
| `HU-Mov-03 &#124; T-D1.3` | Endpoints crear/listar por cuenta, validaciones y mensajes   | 5        |
| `HU-Mov-04 &#124; T-D1.4` | UI Angular: formulario de movimiento + listado con filtros   | 4        |
| `HU-Mov-05 &#124; T-D1.5` | Pruebas de dominio y endpoints clave                         | 2        |
| **Subtotal** |                                                                       | **18**   |

---

### EPIC-E — Reporte de estado de cuenta

| ID      | Tarea                                                                     | Est. (h) |
|---------|---------------------------------------------------------------------------|----------|
| `HU-Rep-01 &#124; T-E1.1` | Endpoint reporte (cliente + rango fechas): totales débitos/créditos/saldos | 4        |
| `HU-Rep-02 &#124; T-E1.2` | Generación PDF mínima (server‑side, p. ej. QuestPDF) + salida base64     | 5        |
| `HU-Rep-03 &#124; T-E1.3` | UI Angular: filtros, tarjetas totales, tabla                             | 3        |
| `HU-Rep-04 &#124; T-E1.4` | Botón Exportar PDF y verificación de descarga                            | 2        |
| **Subtotal** |                                                                       | **14**   |

---

### EPIC-Q — Calidad y validaciones cruzadas

| ID      | Tarea                                                                     | Est. (h) |
|---------|---------------------------------------------------------------------------|----------|
| `HU-QA-01 &#124; T-Q1.1` | Colección Postman completa (CRUDs y reporte)                  | 3        |
| `HU-QA-02 &#124; T-Q1.2` | Tests unitarios adicionales BE (dominio + controller)         | 2        |
| `HU-QA-03 &#124; T-Q1.3` | Tests unitarios FE (componentes tabla/filtros)                | 1        |
| **Subtotal** |                                                                       | **6**    |

---

### EPIC-DPL — Despliegue y documentación

| ID      | Tarea                                                                     | Est. (h) |
|---------|---------------------------------------------------------------------------|----------|
| `HU-Dpl-01 &#124; T-P1.1` | Ajustes finales Docker/compose y perfiles                  | 3        |
| `HU-Dpl-02 &#124; T-P1.2` | README detallado (local, Docker, seeds, colección)         | 3        |
| `HU-Dpl-03 &#124; T-P1.3` | Script `BaseDatos.sql` final + seeds de ejemplo            | 2        |
| **Subtotal** |                                                                       | **8**    |

---

## 4. Resumen de horas

| Épica   | Horas |
|---------|-------|
| EPIC-0  | 10    |
| EPIC-A  | 12    |
| EPIC-C  | 12    |
| EPIC-D  | 18    |
| EPIC-E  | 14    |
| EPIC-Q  | 6     |
| EPIC-DPL| 8     |
| **Total** | **80** |


---

## 5. Entregables

- API .NET 8 con controladores `/clientes`, `/cuentas`, `/movimientos`, `/reportes`.  
- Capa de dominio con invariantes (sobregiro, tope diario, signos).  
- Angular: CRUDs, pantalla de reportes con filtros, totales y exportación PDF.  
- Colección Postman (`/docs/postman/collection.json`).  
- Pruebas unitarias BE/FE mínimas.  
- Dockerfiles + `docker-compose.yml`.  
- `BaseDatos.sql` + seeds (casos del enunciado).  
- README de ejecución local y Docker.

---

## 6. Riesgos y mitigación

| Riesgo                                   | Impacto | Mitigación |
|------------------------------------------|---------|------------|
| Complejidad PDF (fuentes/tablas)         | Medio   | Usar librería probada (QuestPDF/iText); plantilla simple. |
| Reglas de límite diario (acumulado)      | Medio   | Tests de dominio; consulta sumatoria del día + índice por fecha. |
| Sin framework de UI en Angular           | Bajo    | Componentes propios simples; tabla nativa mejorada. |
| Tiempos de Docker en entorno local       | Bajo    | Caché de layers; imágenes slim. |

---

## 7. Criterios de aceptación (claves)

- **Movimientos:** Bloquea sobregiro y excedente de tope diario con mensajes exactos.  
- **Reporte:** Devuelve JSON correcto y PDF/base64 equivalentes (mismos totales).  
- **Front:** Búsqueda en tablas y mensajes de validación visibles.  
- **Docker:** `docker-compose up` levanta DB + API + Front; README usable.  
- **Pruebas:** Unit tests en BE (dominio + controller) y FE (componentes críticos).

---

## 8. Trazabilidad con tus HUs

- **HU-Rep-01** (Reportes): cubierto por `EPIC-E` (HU-Rep-01..HU-Rep-04).  
- **HU-Dpl-01** (Docker/Compose): cubierto por `EPIC-DPL`.  
- **Modelo BD Movimientos (T-D1.2):** `EPIC-D` (HU-Mov-01..HU-Mov-05).

---

## 9. Nota de estimación

Estas horas contemplan una implementación mínima pero robusta en Clean Architecture, con validaciones, manejo de errores y documentación.
