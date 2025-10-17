## 2025-10-15 – Redondeo y tope diario
- **Redondeo**: se adopta **half-even (bancario)** en `decimal(18,2)` para minimizar sesgos en acumulaciones y coincidir con motores SQL/EF por defecto. Se aplica **antes** de evaluar reglas.
- **Tope diario**: parámetro configurable por cuenta, con **valor por defecto 1000.00**. La verificación suma débitos del **día contable** (zona `America/Bogota`).
- **Signos**: se guarda el `ValorFirmado` en movimiento (créditos `+`, débitos `-`) y el `SaldoPosterior` calculado.
- **Idempotencia**: se usa `IdempotencyKey` única por cuenta; en reintento se retorna el mismo resultado.
- **Códigos HTTP**: `422` para reglas de dominio, `409` para conflicto de idempotencia, `404` para cuenta inexistente/inactiva.