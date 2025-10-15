
> **HU-Mov-01 | T-D1.1 – Reglas de dominio para Movimientos**

## 1. Propósito
Establecer reglas funcionales y de dominio para el registro de **créditos** y **débitos** en cuentas, garantizando saldos correctos, ausencia de sobregiro, cumplimiento de tope diario y consistencia de redondeo. Sirve como base de implementación y de pruebas automatizadas.

## 2. Definiciones y entidades
- **Cuenta**: `{ NumeroCuenta, TipoCuenta, SaldoActual, Estado, TopeDiario }`.
  - `TopeDiario`: valor configurable (por defecto **1000.00**).
- **Movimiento**: `{ Id, Fecha, Tipo (CR/DB), MontoOriginal, MontoNormalizado, ValorFirmado, SaldoAnterior, SaldoPosterior, Usuario, IdempotencyKey? }`.
  - `MontoOriginal`: valor recibido (string/decimal).
  - `MontoNormalizado`: `decimal(18, 2)` tras validación + redondeo.
  - `ValorFirmado`: `+MontoNormalizado` si **CR** (crédito); `-MontoNormalizado` si **DB** (débito).
- **Día contable**: intervalo **00:00–23:59:59** en la zona horaria del sistema (sugerida: `America/Bogota`).

## 3. Reglas de negocio
### 3.1 Signos y efecto en saldo
- **Crédito (CR)**: incrementa saldo. `SaldoPosterior = SaldoAnterior + MontoNormalizado`.
- **Débito (DB)**: decrementa saldo. `SaldoPosterior = SaldoAnterior - MontoNormalizado`.
- **Persistencia del valor**: almacenar el `ValorFirmado` en el movimiento y el `SaldoPosterior` resultante.

### 3.2 Restricciones
1. **Montos válidos**: `MontoOriginal > 0`. Rechazar `<= 0`.
2. **Precisión monetaria**: normalizar a `decimal(18,2)` antes de cálculos y verificaciones.
3. **Sin sobregiro** (solo para DB): `SaldoAnterior - MontoNormalizado >= 0`. Si no se cumple → error `Saldo no disponible`.
4. **Límite diario por cuenta** (solo para DB):
   - `TotalDebitosDia = Σ MontoNormalizado(debitos del día)`.
   - Regla: `TotalDebitosDia + MontoNormalizado <= TopeDiario`.
   - Violación → error `Cupo diario Excedido`.
5. **Cuenta activa**: no permitir movimientos si `Estado = false`.

### 3.3 Reglas de redondeo
- **Tipo**: `decimal(18,2)` con **redondeo bancario (half-even)**.
- **Momento**: aplicar redondeo **antes** de evaluar sobregiro y tope diario.
- **Consistencia**: la misma estrategia debe usarse en **API**, **dominio** y **persistencia**.

### 3.4 Idempotencia y duplicados (lineamientos)
- **Idempotency-Key** opcional (header o campo). Si viene definido:
  - Imponer unicidad `{NumeroCuenta, IdempotencyKey}`.
  - Si se repite la operación con la misma clave → devolver **200/201** con el **mismo resultado** (sin efecto secundario adicional).
- Heurística adicional para detectar duplicados sin clave (opcional): ventana corta por `{NumeroCuenta, Tipo, MontoNormalizado, hash(metadata), ±5s}`.

### 3.5 Concurrencia y consistencia
- Ejecutar en **transacción** con **aislamiento** suficiente (recomendado `SERIALIZABLE` o `REPEATABLE READ` + **update por comparación**).
- Actualización atómica sugerida:
  - `UPDATE Cuenta SET SaldoActual = SaldoActual + @ValorFirmado WHERE NumeroCuenta = @N AND SaldoActual + @ValorFirmado >= 0`.
  - Verificar filas afectadas = 1; si 0 → violación de sobregiro.
- Ordenamiento por `FechaCreacion` + `Id` asegura consistencia ante múltiples movimientos en el mismo segundo.

### 3.6 Auditoría mínima
Registrar en el movimiento: `{ Fecha, Usuario, Tipo, SaldoAnterior, SaldoPosterior, Origen/IP?, IdempotencyKey? }`.

### 3.7 Mensajes de error (texto y semántica)
- **Saldo no disponible**: intento de débito con saldo insuficiente.
- **Cupo diario Excedido**: violación del tope diario de débitos.
- **Monto inválido**: `MontoOriginal <= 0` o formato/precisión incorrecta.


## 4. Parámetros configurables
- `Movimientos:TopeDiario` → `decimal`, **default 1000.00**.
- `Movimientos:RoundingMode` → `Bankers` (half-even).
- `Movimientos:Timezone` → `America/Bogota`.
- `Movimientos:EnableIdempotency` → `bool`.
- `Movimientos:DuplicateWindowSeconds` → `int` (opcional, p.ej. 5).

## 5. Ejemplos y casos límite (tablas)
### 5.1 Débito exacto al saldo
| Caso | SaldoAnterior | Tipo | Monto | TotalDebitosDia | Tope | Esperado |
|---|---:|:--:|---:|---:|---:|---|
| DB exacto | 540.00 | DB | 540.00 | 0.00 | 1000.00 | **OK**, SaldoPosterior = 0.00 |

### 5.2 Tope diario alcanzado
| Caso | SaldoAnterior | Tipo | Monto | TotalDebitosDia | Tope | Esperado |
|---|---:|:--:|---:|---:|---:|---|
| Límite | 2000.00 | DB | 300.00 | 800.00 | 1000.00 | **Error**: *Cupo diario Excedido* |

### 5.3 Múltiples movimientos mismo segundo
| Orden | SaldoAnterior | Tipo | Monto | TotalDebitosDia (antes) | Resultado |
|---:|---:|:--:|---:|---:|---|
| 1 | 1000.00 | DB | 200.00 | 0.00 | OK → saldo 800.00 |
| 2 | 800.00 | DB | 900.00 | 200.00 | Error: Saldo no disponible |

### 5.4 Crédito seguido de débito
| Paso | SaldoAnterior | Tipo | Monto | Esperado |
|---|---:|:--:|---:|---|
| 1 | 100.00 | CR | 600.00 | OK → saldo 700.00 |
| 2 | 700.00 | DB | 575.00 | OK → saldo 125.00 |

### 5.5 Idempotencia
| Caso | Clave | Tipo | Monto | Efecto |
|---|---|:--:|---:|---|
| Reintento | `abc-123` | DB | 50.00 | Devuelve el **mismo** movimiento sin duplicar |

## 6. Errores y códigos sugeridos
- `422 Unprocessable Entity` para violaciones de reglas de dominio.
- `409 Conflict` si `IdempotencyKey` duplica con datos inconsistentes.
- `404 Not Found` si la cuenta no existe o está inactiva.

## 7. Métricas y observabilidad
- Contadores de **rechazos por regla** (sobregiro, tope, monto inválido, cuenta inactiva).
- `histogram` de latencia y tamaño de lote.

## 8. Seguridad
- Requiere **autenticación** de operador; registrar `Usuario`.
- Validar permisos sobre la cuenta.

---
