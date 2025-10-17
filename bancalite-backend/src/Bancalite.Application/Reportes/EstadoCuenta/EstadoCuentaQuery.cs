using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bancalite.Application.Core;
using Bancalite.Application.Interface;
using Bancalite.Persitence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bancalite.Application.Reportes.EstadoCuenta
{
    /// <summary>
    /// Query para obtener el reporte de estado de cuenta (JSON/PDF comparten DTO).
    /// </summary>
    public class EstadoCuentaQuery
    {
        public record EstadoCuentaQueryRequest(EstadoCuentaRequest Request) : IRequest<Result<EstadoCuentaDto>>;

        internal class Handler : IRequestHandler<EstadoCuentaQueryRequest, Result<EstadoCuentaDto>>
        {
            private readonly BancaliteContext _context;
            private readonly IUserAccessor _userAccessor;

            public Handler(BancaliteContext context, IUserAccessor userAccessor)
            {
                _context = context;
                _userAccessor = userAccessor;
            }

            public async Task<Result<EstadoCuentaDto>> Handle(EstadoCuentaQueryRequest message, CancellationToken ct)
            {
                var req = message.Request;
                if (req.ClienteId == null && string.IsNullOrWhiteSpace(req.NumeroCuenta))
                    return Result<EstadoCuentaDto>.Failure("BadRequest: Debe indicar clienteId o numeroCuenta");
                if (req.Desde > req.Hasta)
                    return Result<EstadoCuentaDto>.Failure("BadRequest: Rango de fechas inválido");

                var identidad = _userAccessor.GetUsername();
                if (string.IsNullOrWhiteSpace(identidad)) return Result<EstadoCuentaDto>.Failure("Unauthorized");

                // Resolver cuentas destino y autorización
                List<Guid> cuentaIds = new();
                Guid? clienteId = null;
                string? clienteNombre = null;
                string? numeroCuenta = null;

                bool esAdmin = await (from ur in _context.UserRoles
                                       join r in _context.Roles on ur.RoleId equals r.Id
                                       join u in _context.Users on ur.UserId equals u.Id
                                       where (u.Email == identidad || u.UserName == identidad || u.Id.ToString() == identidad) && r.Name == "Admin"
                                       select ur).AnyAsync(ct);

                if (!string.IsNullOrWhiteSpace(req.NumeroCuenta))
                {
                    var cuenta = await _context.Cuentas.Include(c => c.Cliente).ThenInclude(cl => cl.Persona)
                        .FirstOrDefaultAsync(c => c.NumeroCuenta == req.NumeroCuenta!.Trim(), ct);
                    if (cuenta == null) return Result<EstadoCuentaDto>.Failure("No encontrado");
                    if (!esAdmin)
                    {
                        var userId = await _context.Users.AsNoTracking().Where(u => u.Email == identidad || u.UserName == identidad || u.Id.ToString() == identidad)
                            .Select(u => u.Id).FirstOrDefaultAsync(ct);
                        if (cuenta.Cliente.AppUserId == null || cuenta.Cliente.AppUserId != userId)
                            return Result<EstadoCuentaDto>.Failure("Forbidden");
                    }
                    cuentaIds.Add(cuenta.Id);
                    clienteId = cuenta.ClienteId;
                    clienteNombre = $"{cuenta.Cliente.Persona.Nombres} {cuenta.Cliente.Persona.Apellidos}";
                    numeroCuenta = cuenta.NumeroCuenta;
                }
                else if (req.ClienteId != null)
                {
                    var cliente = await _context.Clientes.Include(c => c.Persona).FirstOrDefaultAsync(c => c.Id == req.ClienteId, ct);
                    if (cliente == null) return Result<EstadoCuentaDto>.Failure("No encontrado");
                    if (!esAdmin)
                    {
                        var userId = await _context.Users.AsNoTracking().Where(u => u.Email == identidad || u.UserName == identidad || u.Id.ToString() == identidad)
                            .Select(u => u.Id).FirstOrDefaultAsync(ct);
                        if (cliente.AppUserId == null || cliente.AppUserId != userId)
                            return Result<EstadoCuentaDto>.Failure("Forbidden");
                    }
                    cuentaIds = await _context.Cuentas.AsNoTracking().Where(c => c.ClienteId == cliente.Id).Select(c => c.Id).ToListAsync(ct);
                    clienteId = cliente.Id;
                    clienteNombre = $"{cliente.Persona.Nombres} {cliente.Persona.Apellidos}";
                }

                // Si no hay cuentas, 404
                if (cuentaIds.Count == 0) return Result<EstadoCuentaDto>.Failure("No encontrado");

                var desde = req.Desde.Date;
                var hastaExcl = req.Hasta.Date.AddDays(1);

                var movs = await _context.Movimientos.AsNoTracking()
                    .Include(m => m.Tipo)
                    .Include(m => m.Cuenta)
                    .Where(m => cuentaIds.Contains(m.CuentaId) && m.Fecha >= desde && m.Fecha < hastaExcl)
                    .OrderBy(m => m.Fecha).ThenBy(m => m.Id)
                    .ToListAsync(ct);

                var dto = new EstadoCuentaDto
                {
                    ClienteId = clienteId,
                    ClienteNombre = clienteNombre,
                    NumeroCuenta = numeroCuenta,
                    Desde = req.Desde,
                    Hasta = req.Hasta
                };

                // Detalle
                foreach (var m in movs)
                {
                    dto.Movimientos.Add(new EstadoCuentaItemDto
                    {
                        Fecha = m.Fecha,
                        NumeroCuenta = m.Cuenta.NumeroCuenta,
                        TipoCodigo = m.Tipo.Codigo,
                        Monto = m.Monto,
                        SaldoPrevio = m.SaldoPrevio,
                        SaldoPosterior = m.SaldoPosterior,
                        Descripcion = m.Descripcion
                    });
                }

                // Totales y saldos
                dto.TotalCreditos = movs.Where(x => x.Tipo.Codigo == "CRE").Sum(x => x.Monto);
                dto.TotalDebitos = movs.Where(x => x.Tipo.Codigo == "DEB").Sum(x => x.Monto);

                if (movs.Count > 0)
                {
                    // Si hay varias cuentas, el saldo inicial suma el primer movimiento por cuenta
                    var primerosPorCuenta = movs.GroupBy(x => x.CuentaId).Select(g => g.First()).ToList();
                    var ultimosPorCuenta = movs.GroupBy(x => x.CuentaId).Select(g => g.Last()).ToList();
                    dto.SaldoInicial = primerosPorCuenta.Sum(x => x.SaldoPrevio);
                    dto.SaldoFinal = ultimosPorCuenta.Sum(x => x.SaldoPosterior);
                }
                else
                {
                    // Sin movimientos: usar saldos actuales como inicial/final para referencia
                    var cuentas = await _context.Cuentas.AsNoTracking().Where(c => cuentaIds.Contains(c.Id)).ToListAsync(ct);
                    dto.SaldoInicial = cuentas.Sum(c => c.SaldoActual);
                    dto.SaldoFinal = dto.SaldoInicial;
                }

                return Result<EstadoCuentaDto>.Success(dto);
            }
        }
    }
}


