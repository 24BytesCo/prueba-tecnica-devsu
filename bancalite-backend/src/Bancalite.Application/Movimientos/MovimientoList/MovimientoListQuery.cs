using Bancalite.Application.Core;
using Bancalite.Application.Interface;
using Bancalite.Persitence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bancalite.Application.Movimientos.MovimientoList
{
    /// <summary>
    /// Query para listar movimientos por cuenta y rango de fechas.
    /// </summary>
    public class MovimientoListQuery
    {
        /// <summary>
        /// Parámetros de consulta.
        /// </summary>
        public record MovimientoListQueryRequest(string NumeroCuenta, DateTime? Desde = null, DateTime? Hasta = null)
            : IRequest<Result<IReadOnlyList<Item>>>;

        /// <summary>
        /// Item liviano de movimiento.
        /// </summary>
        public class Item
        {
            public Guid MovimientoId { get; set; }
            public DateTime Fecha { get; set; }
            public string TipoCodigo { get; set; } = string.Empty;
            public decimal Monto { get; set; }
            public decimal SaldoPrevio { get; set; }
            public decimal SaldoPosterior { get; set; }
            public string? Descripcion { get; set; }
        }

        internal class Handler : IRequestHandler<MovimientoListQueryRequest, Result<IReadOnlyList<Item>>>
        {
            private readonly BancaliteContext _context;
            private readonly IUserAccessor _userAccessor;
            public Handler(BancaliteContext context, IUserAccessor userAccessor)
            {
                _context = context;
                _userAccessor = userAccessor;
            }

            public async Task<Result<IReadOnlyList<Item>>> Handle(MovimientoListQueryRequest request, CancellationToken ct)
            {
                // Usuario actual
                var identidad = _userAccessor.GetUsername();
                if (string.IsNullOrWhiteSpace(identidad)) return Result<IReadOnlyList<Item>>.Failure("Unauthorized");

                var numero = request.NumeroCuenta.Trim();
                var cuenta = await _context.Cuentas.Include(c => c.Cliente).FirstOrDefaultAsync(c => c.NumeroCuenta == numero, ct);
                if (cuenta == null) return Result<IReadOnlyList<Item>>.Failure("No encontrado");

                // Admin o propietario
                var esAdmin = await (from ur in _context.UserRoles
                                     join r in _context.Roles on ur.RoleId equals r.Id
                                     join u in _context.Users on ur.UserId equals u.Id
                                     where (u.Email == identidad || u.UserName == identidad || u.Id.ToString() == identidad) && r.Name == "Admin"
                                     select ur).AnyAsync(ct);
                if (!esAdmin)
                {
                    var userId = await _context.Users.AsNoTracking()
                        .Where(u => u.Email == identidad || u.UserName == identidad || u.Id.ToString() == identidad)
                        .Select(u => u.Id).FirstOrDefaultAsync(ct);
                    if (cuenta.Cliente.AppUserId == null || cuenta.Cliente.AppUserId != userId)
                        return Result<IReadOnlyList<Item>>.Failure("Forbidden");
                }

                // Rango de fechas (UTC) — si no se envía, todo el histórico
                var desde = request.Desde?.Date;
                var hastaExcl = request.Hasta?.Date.AddDays(1);

                var query = _context.Movimientos.AsNoTracking()
                    .Include(m => m.Tipo)
                    .Where(m => m.CuentaId == cuenta.Id)
                    .AsQueryable();

                if (desde.HasValue) query = query.Where(m => m.Fecha >= desde.Value);
                if (hastaExcl.HasValue) query = query.Where(m => m.Fecha < hastaExcl.Value);

                var items = await query
                    .OrderBy(m => m.Fecha).ThenBy(m => m.Id)
                    .Select(m => new Item
                    {
                        MovimientoId = m.Id,
                        Fecha = m.Fecha,
                        TipoCodigo = m.Tipo.Codigo,
                        Monto = m.Monto,
                        SaldoPrevio = m.SaldoPrevio,
                        SaldoPosterior = m.SaldoPosterior,
                        Descripcion = m.Descripcion
                    })
                    .ToListAsync(ct);

                return Result<IReadOnlyList<Item>>.Success(items);
            }
        }
    }
}

