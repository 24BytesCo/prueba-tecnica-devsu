using AutoMapper;
using Bancalite.Application.Core;
using Bancalite.Application.Cuentas.CuentaList;
using Bancalite.Application.Interface;
using Bancalite.Persitence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bancalite.Application.Cuentas.MisCuentas
{
    /// <summary>
    /// Query para obtener las cuentas del usuario autenticado.
    /// </summary>
    public class MisCuentasQuery
    {
        /// <summary>
        /// Request sin parámetros; usa el usuario del contexto.
        /// </summary>
        public record MisCuentasQueryRequest() : IRequest<Result<IReadOnlyList<CuentaListItem>>>;

        internal class Handler : IRequestHandler<MisCuentasQueryRequest, Result<IReadOnlyList<CuentaListItem>>>
        {
            private readonly BancaliteContext _context;
            private readonly IUserAccessor _userAccessor;
            private readonly IMapper _mapeador;

            public Handler(BancaliteContext context, IUserAccessor userAccessor, IMapper mapeador)
            {
                _context = context;
                _userAccessor = userAccessor;
                _mapeador = mapeador;
            }

            public async Task<Result<IReadOnlyList<CuentaListItem>>> Handle(MisCuentasQueryRequest request, CancellationToken cancellationToken)
            {
                // Resolver identidad del usuario actual
                var identidad = _userAccessor.GetUsername();
                if (string.IsNullOrWhiteSpace(identidad))
                    return Result<IReadOnlyList<CuentaListItem>>.Failure("Unauthorized");

                Guid userId;
                if (!Guid.TryParse(identidad, out userId))
                {
                    var user = await _context.Users.AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Email == identidad || u.UserName == identidad, cancellationToken);
                    if (user == null)
                        return Result<IReadOnlyList<CuentaListItem>>.Failure("Unauthorized");
                    userId = user.Id;
                }

                // Obtener el cliente asociado al usuario
                var clienteId = await _context.Clientes.AsNoTracking()
                    .Where(c => c.AppUserId == userId)
                    .Select(c => c.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (clienteId == Guid.Empty)
                {
                    // No tiene cliente asociado => lista vacía
                    return Result<IReadOnlyList<CuentaListItem>>.Success(Array.Empty<CuentaListItem>());
                }

                // Consultar cuentas del cliente
                var cuentas = await _context.Cuentas
                    .AsNoTracking()
                    .Include(c => c.TipoCuenta)
                    .Include(c => c.Cliente).ThenInclude(cl => cl.Persona)
                    .Where(c => c.ClienteId == clienteId)
                    .OrderBy(c => c.NumeroCuenta)
                    .ToListAsync(cancellationToken);

                var items = _mapeador.Map<List<CuentaListItem>>(cuentas);
                return Result<IReadOnlyList<CuentaListItem>>.Success(items);
            }
        }
    }
}

