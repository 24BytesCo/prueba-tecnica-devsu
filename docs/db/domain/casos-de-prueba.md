
> **Derivados de "movimientos-spec.md".**

## Conjunto mínimo (QA)
| ID | Precondiciones | Entrada | Esperado |
|---|---|---|---|
| CT-001 | Cuenta activa saldo 700.00 | CR 600.00 | 201, SaldoPosterior 1300.00 |
| CT-002 | Cuenta activa saldo 1300.00 | DB 575.00 | 201, SaldoPosterior 725.00 |
| CT-003 | Saldo 540.00, TotalDebitosDia 0 | DB 540.00 | 201, SaldoPosterior 0.00 |
| CT-004 | Saldo 0.00 | DB 10.00 | 422, `Saldo no disponible` |
| CT-005 | TopeDiario 1000.00, TotalDebitosDia 900.00 | DB 150.00 | 422, `Cupo diario Excedido` |
| CT-006 | MontoOriginal 0 | DB 0 | 422, `Monto inválido` |
| CT-007 | Cuenta inactiva | CR 10.00 | 404 |
| CT-008 | Reintento con IdemKey | DB 50.00 + key `abc` | 201/200, sin duplicado |
| CT-009 | Concurrencia 2 débitos simultáneos 800.00 y 300.00 sobre saldo 1000.00 | DB | Uno **201** (saldo 200.00) y otro **422** por sobregiro |
