using Bancalite.Application.Config;
using Bancalite.Application.Core;
using Bancalite.Application.Interface;
using Bancalite.Domain;
using Bancalite.Persitence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Bancalite.Application.Movimientos.MovimientoCreate
{
    /// <summary>
    /// Comando para registrar créditos y débitos en una cuenta con validaciones exhaustivas.
    /// </summary>
    public class MovimientoCreateCommand
    {
        /// <summary>
        /// Solicitud para crear un movimiento.
        /// </summary>
        public record MovimientoCreateCommandRequest(MovimientoCreateRequest Request) : IRequest<Result<MovimientoDto>>;

        /// <summary>
        /// DTO de respuesta del movimiento creado.
        /// </summary>
        public class MovimientoDto
        {
            public Guid MovimientoId { get; set; }
            public Guid CuentaId { get; set; }
            public string NumeroCuenta { get; set; } = string.Empty;
            public string TipoCodigo { get; set; } = string.Empty;
            public decimal Monto { get; set; }
            public decimal SaldoPrevio { get; set; }
            public decimal SaldoPosterior { get; set; }
            public DateTime Fecha { get; set; }
        }

        internal class Handler : IRequestHandler<MovimientoCreateCommandRequest, Result<MovimientoDto>>
        {
            private readonly BancaliteContext _context;
            private readonly IUserAccessor _userAccessor;
            private readonly MovimientosOptions _options;

            public Handler(BancaliteContext context, IUserAccessor userAccessor, IOptions<MovimientosOptions> options)
            {
                _context = context;
                _userAccessor = userAccessor;
                _options = options.Value;
            }

            public async Task<Result<MovimientoDto>> Handle(MovimientoCreateCommandRequest message, CancellationToken ct)
            {
                // Usuario actual
                var identidad = _userAccessor.GetUsername();
                if (string.IsNullOrWhiteSpace(identidad))
                    return Result<MovimientoDto>.Failure("Unauthorized");

                // Normalizar datos de request
                var numero = message.Request.NumeroCuenta.Trim(); // número de cuenta
                var tipoCodigo = message.Request.TipoCodigo.Trim().ToUpperInvariant(); // CRE/DEB

                // Buscar cuenta y propietario
                var cuenta = await _context.Cuentas
                    .Include(c => c.Cliente)
                    .FirstOrDefaultAsync(c => c.NumeroCuenta == numero, ct);
                if (cuenta == null)
                    return Result<MovimientoDto>.Failure("No encontrado");

                // Reglas de autorización: Admin o propietario de la cuenta
                var esAdmin = await (from ur in _context.UserRoles
                                     join r in _context.Roles on ur.RoleId equals r.Id
                                     join u in _context.Users on ur.UserId equals u.Id
                                     where (u.Email == identidad || u.UserName == identidad || u.Id.ToString() == identidad) && r.Name == "Admin"
                                     select ur).AnyAsync(ct);
                if (!esAdmin)
                {
                    var userId = await _context.Users.AsNoTracking()
                        .Where(u => u.Email == identidad || u.UserName == identidad || u.Id.ToString() == identidad)
                        .Select(u => u.Id)
                        .FirstOrDefaultAsync(ct);
                    if (cuenta.Cliente.AppUserId == null || cuenta.Cliente.AppUserId != userId)
                        return Result<MovimientoDto>.Failure("Forbidden");
                }

                // Cuenta debe estar Activa
                if (!string.Equals(cuenta.Estado.ToString(), "Activa", StringComparison.OrdinalIgnoreCase))
                    return Result<MovimientoDto>.Failure("No encontrado"); // ocultar existencia

                // Validar tipo de movimiento por código (DEB/CRE)
                var tipo = await _context.TiposMovimiento.AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Codigo == tipoCodigo, ct);
                if (tipo == null || !tipo.Activo)
                    return Result<MovimientoDto>.Failure("BadRequest: Tipo de movimiento inválido");

                // Normalización monetaria (2 decimales, half-even)
                var montoNorm = Math.Round(message.Request.Monto, 2, _options.RoundingMode);
                if (montoNorm <= 0)
                    return Result<MovimientoDto>.Failure("Unprocessable: Monto inválido");

                // Idempotencia: si trae clave y existe registro previo, devolver el mismo resultado
                if (!string.IsNullOrWhiteSpace(message.Request.IdempotencyKey))
                {
                    var prev = await _context.Movimientos.AsNoTracking()
                        .Include(m => m.Cuenta)
                        .Include(m => m.Tipo)
                        .FirstOrDefaultAsync(m => m.CuentaId == cuenta.Id && m.IdempotencyKey == message.Request.IdempotencyKey, ct);
                    if (prev != null)
                    {
                        return Result<MovimientoDto>.Success(new MovimientoDto
                        {
                            MovimientoId = prev.Id,
                            CuentaId = prev.CuentaId,
                            NumeroCuenta = prev.Cuenta.NumeroCuenta,
                            TipoCodigo = prev.Tipo.Codigo,
                            Monto = prev.Monto,
                            SaldoPrevio = prev.SaldoPrevio,
                            SaldoPosterior = prev.SaldoPosterior,
                            Fecha = prev.Fecha
                        });
                    }
                }

                // Transacción para consistencia (solo en proveedores relacionales)
                Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? tx = null;
                if (_context.Database.IsRelational())
                {
                    tx = await _context.Database.BeginTransactionAsync(ct);
                }

                // Recalcular saldos dentro de la transacción
                await _context.Entry(cuenta).ReloadAsync(ct); // refrescar fila

                var saldoPrevio = cuenta.SaldoActual; // snapshot

                // Validación de tope diario en débitos
                if (tipoCodigo == "DEB")
                {
                    // Ventana del día (UTC)
                    var hoy = DateTime.UtcNow.Date;
                    var manana = hoy.AddDays(1);

                    var tipoDeb = await _context.TiposMovimiento.AsNoTracking().FirstOrDefaultAsync(t => t.Codigo == "DEB", ct);
                    var totalDebitosDia = await _context.Movimientos.AsNoTracking()
                        .Where(m => m.CuentaId == cuenta.Id && m.TipoId == tipoDeb!.Id && m.Fecha >= hoy && m.Fecha < manana)
                        .SumAsync(m => m.Monto, ct);

                    if (totalDebitosDia + montoNorm > _options.TopeDiario)
                        return Result<MovimientoDto>.Failure("Unprocessable: Cupo diario Excedido");

                    // Sin sobregiro
                    if (saldoPrevio - montoNorm < 0)
                        return Result<MovimientoDto>.Failure("Unprocessable: Saldo no disponible");
                }

                // Efecto en saldo
                var valorFirmado = tipoCodigo == "DEB" ? -montoNorm : montoNorm; // signo
                var saldoPosterior = saldoPrevio + valorFirmado; // nuevo saldo

                // Persistir movimiento y saldo
                var movimiento = new Movimiento
                {
                    Id = Guid.NewGuid(),
                    CuentaId = cuenta.Id,
                    TipoId = tipo.Id,
                    Monto = montoNorm,
                    SaldoPrevio = saldoPrevio,
                    SaldoPosterior = saldoPosterior,
                    Fecha = DateTime.UtcNow,
                    IdempotencyKey = message.Request.IdempotencyKey,
                    Descripcion = string.IsNullOrWhiteSpace(message.Request.Descripcion) ? null : message.Request.Descripcion!.Trim(),
                    CreatedBy = identidad
                };

                // Actualizar saldo
                cuenta.SaldoActual = saldoPosterior;

                await _context.Movimientos.AddAsync(movimiento, ct);
                await _context.SaveChangesAsync(ct);
                if (tx != null)
                {
                    await tx.CommitAsync(ct);
                }

                // DTO de salida
                var dto = new MovimientoDto
                {
                    MovimientoId = movimiento.Id,
                    CuentaId = movimiento.CuentaId,
                    NumeroCuenta = cuenta.NumeroCuenta,
                    TipoCodigo = tipo.Codigo,
                    Monto = movimiento.Monto,
                    SaldoPrevio = movimiento.SaldoPrevio,
                    SaldoPosterior = movimiento.SaldoPosterior,
                    Fecha = movimiento.Fecha
                };
                return Result<MovimientoDto>.Success(dto);
            }
        }
    }
}
