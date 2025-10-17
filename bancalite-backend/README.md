# Bancalite Backend

Backend de Bancalite construido con ASP.NET Core (net9), EF Core (PostgreSQL), Identity, MediatR (CQRS) y FluentValidation. Incluye autenticación JWT, recuperación de contraseña por correo y seeding de catálogos/roles/usuario admin en entorno de desarrollo.

## Stack

- .NET 9 (preview) + ASP.NET Core Web API
- MediatR (CQRS) + FluentValidation (pipeline automático)
- Entity Framework Core + Npgsql (PostgreSQL)
- ASP.NET Core Identity (usuarios/roles) + JWT
- Swashbuckle (Swagger/OpenAPI)

## Estructura de carpetas (backend)

- `src/Bancalite.Domain`: entidades del dominio (Cliente, Persona, Cuenta, Movimiento, catálogos…)
- `src/Bancalite.Persitence`: DbContext (EF Core), configuración de entidades, migraciones y seeding de catálogos
- `src/Bancalite.Infraestructure`: DI, servicios de seguridad (tokens, user accessor), email SMTP, seeding de roles/usuario admin
- `src/Bancalite.Application`: capa de aplicación (CQRS con MediatR), validaciones, contratos
- `src/Bancalite.WebApi`: API (Program, controllers, Swagger)

## Patrones y convenciones

- CQRS con MediatR: cada operación expone Request/Command/Query y Handler en `Application/*`
- Validación automática: `ValidationBehavior<TReq,TRes>` ejecuta todos los `IValidator<TReq>` antes del handler
- FluentValidation:
  - Validaciones de formato y FKs (existencia) en validadores
  - Reglas de unicidad y errores de concurrencia: preferentemente en handler + restricciones en DB
- Identity + JWT: autenticación por Bearer, roles `Admin` y `User`

## Requisitos

- .NET SDK 9 (preview)
- Docker (opcional para Postgres)
- PostgreSQL 14+ (si no usas Docker)

## Configuración (appsettings / variables)

`appsettings.Development.json` está ignorado por git. Usa `appsettings.Example.json` como plantilla.

### ConnectionStrings

- `ConnectionStrings:Default`: cadena completa de Postgres
- Alternativa: variables `DB_HOST/DB_PORT/DB_NAME/DB_USER/DB_PASSWORD` (se usa si no hay ConnectionStrings:Default)

### JWT

- `JWT:Key`: clave simétrica (mínimo 32 chars)
- `JWT:Issuer`, `JWT:Audience`, `JWT:ExpiresMinutes`

### SMTP (IEmailSender)

- `Smtp:Host`, `Smtp:Port`, `Smtp:EnableSsl`, `Smtp:Username`, `Smtp:Password`, `Smtp:SenderEmail`, `Smtp:SenderName`
- Para Ethereal en dev: `SenderEmail` debe ser igual a `Username`

### Usuario Admin (solo Development)

- `ADMIN_USERNAME`, `ADMIN_EMAIL`, `ADMIN_PASSWORD`

## Ejecución

### Desarrollo (con Postgres local)

1. Configura `bancalite-backend/src/Bancalite.WebApi/appsettings.Development.json` (basado en el Example)
2. Ejecuta API:

   ```bash
   DOTNET_ENVIRONMENT=Development dotnet run --project src/Bancalite.WebApi
   ```

3. Swagger: `http://localhost:<puerto>/swagger`

### Desarrollo (con Docker para Postgres)

1. Levanta Postgres con `docker-compose` en la raíz (ver README de raíz)
2. Ajusta `ConnectionStrings:Default` para apuntar a `localhost:5432`
3. Ejecuta la API como arriba

## Migraciones y base de datos

- En Development, `CatalogSeedHostedService` aplica migraciones al arrancar y siembra catálogos/roles/usuario admin
- Comandos útiles:

```bash
dotnet ef migrations add <Nombre> -p src/Bancalite.Persitence -s src/Bancalite.WebApi
dotnet ef database update -p src/Bancalite.Persitence -s src/Bancalite.WebApi
```

## Autenticación y autorización

- Login: `POST /api/auth/login` → retorna `Profile` con `Token`
- Bearer: usar `Authorize` en Swagger y enviar `Bearer <token>`
- Roles: `Admin` y `User`
- Creación de clientes: `POST /api/clientes` requiere rol `Admin`

## Cuentas (API)

Endpoints y reglas principales del módulo Cuentas:

- `GET /api/cuentas`
  - Lista paginada y filtrable por `clienteId` y `estado`.
  - Requiere rol `Admin`.
- `GET /api/cuentas/mias`
  - Devuelve las cuentas del usuario autenticado (según vínculo con `Cliente.AppUserId`).
  - Requiere autenticación.
- `GET /api/cuentas/{id}`
  - Detalle de una cuenta (incluye tipo y titular).
  - Requiere autenticación.
- `POST /api/cuentas`
  - Abre una nueva cuenta. Si `NumeroCuenta` viene vacío, el sistema genera uno único (formato `####-####-####`).
  - Validaciones: `SaldoInicial >= 0`, `TipoCuentaId` y `ClienteId` deben existir, `NumeroCuenta` único si se envía.
  - Autorización: `Admin` puede crear para cualquier cliente. No-admin solo puede crear para su propio `ClienteId`.
- `PUT /api/cuentas/{id}` y `PATCH /api/cuentas/{id}`
  - Actualizan los datos de la cuenta (PUT: total, PATCH: parcial).
  - Validan unicidad de `NumeroCuenta` en cambios.
  - Requieren rol `Admin`.
- `PATCH /api/cuentas/{id}/estado`
  - Cambia el estado (Activa/Inactiva/Bloqueada).
  - Regla: para pasar a `Inactiva` el `SaldoActual` debe ser 0 (si no, 422).
  - Requiere rol `Admin`.
- `DELETE /api/cuentas/{id}` (soft-delete)
  - Inactiva una cuenta si `SaldoActual` es 0; si no, devuelve 422.
  - Requiere rol `Admin`.

## Movimientos (API)

Endpoints y reglas principales del módulo Movimientos:

- `POST /api/movimientos`
  - Registra un movimiento sobre una cuenta: `TipoCodigo = CRE` (crédito) o `DEB` (débito).
  - Idempotencia opcional mediante `IdempotencyKey` (misma clave → mismo resultado, sin duplicar).
  - Validaciones de dominio (respuesta 422 cuando corresponda):
    - Monto normalizado a 2 decimales con half-even (Bankers) y debe ser > 0.
    - Para débitos: sin sobregiro (mensaje “Saldo no disponible”).
    - Tope diario de retiros por cuenta (por defecto `1000.00`) (mensaje “Cupo diario Excedido”).
    - La cuenta debe estar Activa.
  - Seguridad: `Authorize`. Admin o propietario de la cuenta.
  - Respuesta: 201 (Created) con el DTO del movimiento en éxito; 4xx mapeados vía `Result`.

- `GET /api/movimientos?numeroCuenta=&desde=&hasta=`
  - Lista movimientos por número de cuenta y rango de fechas (UTC, [desde, hasta)).
  - Seguridad: `Authorize`. Admin o propietario de la cuenta.

### Configuración de Movimientos

En `appsettings.json` (opcional; valores por defecto indicados):

```json
{
  "Movimientos": {
    "TopeDiario": 1000,
    "RoundingMode": "ToEven"
  }
}
```

### Manejo de errores (WebApi)

Los handlers del Application layer devuelven `Result<T>`; el WebApi mapea a HTTP:

- 200: `IsSuccess = true`.
- 400 (BadRequest): errores de validación (FluentValidation) o reglas marcadas como `BadRequest`.
- 401 (Unauthorized): identidad ausente o inválida.
- 403 (Forbidden): identidad sin permisos (no-admin/no-propietario).
- 404 (Not Found): recurso inexistente.
- 409 (Conflict): duplicados emitidos explícitamente desde handler (en Cuentas, la unicidad de número validada en FluentValidation responde 400).
- 422 (Unprocessable Entity): violaciones de reglas de negocio (p. ej., inactivar/eliminar con saldo ≠ 0).

## Clientes (create/list)

## Recuperación de contraseña

- `POST /api/auth/forgot-password?email=&redirectBaseUrl=`
  - Devuelve 401 si el usuario no existe
  - En Development, devuelve `{ sent, link, token }` para pruebas
  - Envía correo SMTP con enlace de reseteo (o muestra en Ethereal)
- `POST /api/auth/reset-password?email=&token=&newPassword=`

## Clientes (create/list)

- Create (CQRS): crea `Persona` y `Cliente`, crea/vincula `AppUser` (Identity) con `Email/Password`, asigna rol (`IdRol` o `User`), copia `PasswordHash` al `Cliente`
- List: retorna datos de persona, estado, nombres/códigos de catálogos y rol

## Estado actual vs ejercicio

- CRUD Clientes: implementado (con seguridad Admin para crear y propietario/admin para consultar/actualizar/borrar).
- CRUD Cuentas: implementado con reglas y roles anteriores.
- Movimientos: implementado (créditos/débitos, tope diario, sobregiro, idempotencia, listado por fechas).
- Reporte de estado de cuenta (JSON/PDF base64): pendiente.

## Testing

## Buenas prácticas del repo

- No commitear secretos: `appsettings.Development.json`, `.env`, credenciales, claves
- Mantener validaciones en validadores; reglas con acceso a datos sensibles en handlers
- Evitar lógica en controllers: delegar en Application (MediatR)
- Usar migraciones para cambios de esquema; no modificar DB a mano
- Nombres y mensajes cortos y claros.

## Testing

- Ejecutar: `dotnet test`
- Pruebas de integración incluidas:
  - Auth: login, me, logout, refresh.
  - Clientes: listar, crear, get por id, put/patch/delete (incluyendo seguridad propietario/admin).
  - Cuentas: listar (solo admin), crear (admin/propietario), detalle, mis cuentas, unicidad de número, put/patch (solo admin), cambio de estado y delete con regla de saldo.
  - Movimientos: 
    - Créditos/débitos válidos → 201 y saldoPosterior correcto.
    - Débito sin saldo suficiente → 422 “Saldo no disponible”.
    - Tope diario excedido → 422 “Cupo diario Excedido”.
    - Débito exacto deja saldo en 0.00.
    - Saldo 0 y débito → 422.
    - Idempotencia con misma clave → mismo MovimientoId y mismo resultado.
    - Cuenta inactiva → 404 en intento de movimiento.
    - Dos débitos consecutivos (800 y 300) sobre saldo 1000 → uno OK y el otro 422.
  - En pruebas se usa autenticación de test con cabecera `X-Test-Email` para simular identidad:
    - Sin cabecera → Admin por defecto.
    - Con cabecera → Usuario con ese email (sin rol Admin), útil para validar casos de propietario/no-admin.

## Reportes (API)

Endpoints del reporte de estado de cuenta (JSON y PDF):

- `GET /api/reportes?clienteId|numeroCuenta&desde&hasta`
  - Devuelve JSON con:
    - `desde`, `hasta`, `clienteId/clienteNombre` y/o `numeroCuenta`.
    - Totales: `totalCreditos`, `totalDebitos`, `saldoInicial`, `saldoFinal`.
    - Detalle: lista de movimientos `{ fecha, numeroCuenta, tipoCodigo, monto, saldoPrevio, saldoPosterior, descripcion }`.
  - Reglas y seguridad:
    - Debe indicarse `clienteId` o `numeroCuenta` (400 si faltan ambos; 400 si rango inválido).
    - Admin u operador con permiso; clientes solo ven sus datos.
    - 404 si no hay datos/entidades encontradas.

- `GET /api/reportes/pdf?clienteId|numeroCuenta&desde&hasta`
  - Renderiza el mismo dataset en PDF (paridad con JSON garantizada al compartir DTO).
  - Encabezados de respuesta: `Content-Type: application/pdf`, `Content-Disposition: attachment; filename="reporte-estado-cuenta-<timestamp>.pdf"`.
  - Paginación en PDF: cabecera por página + tabla de movimientos; se parte automáticamente el detalle para listas largas.
  - Plantilla simple con fuente estándar (Helvetica).

Notas de cálculo
- Los totales se agregan sobre el rango `[desde, hasta]` en UTC.
- `saldoInicial/saldoFinal` se calculan a partir de `SaldoPrevio/SaldoPosterior` del primer/último movimiento por cuenta. Si no hay movimientos, se usa el `SaldoActual` como referencia.

Errores y códigos (JSON/PDF)
- 400: validación de filtros/rango.
- 401/403: autenticación/autorización.
- 404: cliente/cuenta inexistente o sin datos.

## Migraciones

- Comandos generales:

```bash
dotnet ef migrations add <Nombre> -p src/Bancalite.Persitence -s src/Bancalite.WebApi
dotnet ef database update -p src/Bancalite.Persitence -s src/Bancalite.WebApi
```

- Contexto usado: `BancaliteContext` (único). Para dirigir la salida a la carpeta actual de migraciones:

```bash
dotnet ef migrations add AddMovimientoIdempotencyKey \
  -p bancalite-backend/src/Bancalite.Persitence \
  -s bancalite-backend/src/Bancalite.WebApi \
  --context BancaliteContext -o Migrations/Bancalite

dotnet ef database update \
  -p bancalite-backend/src/Bancalite.Persitence \
  -s bancalite-backend/src/Bancalite.WebApi \
  --context BancaliteContext
```

- Si la base ya tenía tablas (por ejemplo Identity) pero el historial de migraciones está vacío, puede ser necesario hacer un “baseline” del primer snapshot y luego aplicar las migraciones nuevas. En desarrollo, como alternativa rápida, recrear la base de datos evita inconsistencias.

## Solución de problemas

- Build con archivos bloqueados: matar proceso `Bancalite.WebApi` (Windows: `taskkill /IM Bancalite.WebApi.exe /F`)
- Error tokens de Identity: asegurar `.AddDefaultTokenProviders()` en DI
- Ethereal no entrega a Gmail/Outlook: revisar mensajes en su UI
