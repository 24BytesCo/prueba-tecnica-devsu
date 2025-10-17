using System;
using System.Threading;
using System.Threading.Tasks;
using Bancalite.Application.Core;
using Bancalite.Persitence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Bancalite.Application.Interface;

namespace Bancalite.Application.Clientes.ClienteDelete
{
    /// <summary>
    /// Comando para realizar soft-delete del cliente (Estado = false).
    /// </summary>
    public class ClienteDeleteCommand
    {
        /// <summary>
        /// Solicitud con el Id del cliente a eliminar lógicamente.
        /// </summary>
        /// <param name="Id">Identificador del cliente a desactivar.</param>
        public record ClienteDeleteCommandRequest(Guid Id) : IRequest<Result<bool>>;

        internal class Handler : IRequestHandler<ClienteDeleteCommandRequest, Result<bool>>
        {
            private readonly BancaliteContext _context;
            private readonly IUserAccessor _userAccessor;

            public Handler(BancaliteContext context, IUserAccessor userAccessor)
            {
                _context = context;
                _userAccessor = userAccessor;
            }

            /// <summary>
            /// Marca el cliente como inactivo (soft-delete).
            /// </summary>
            /// <param name="request">Id del cliente a desactivar.</param>
            /// <param name="cancellationToken">Token de cancelación.</param>
            /// <returns>Resultado indicando éxito o error.</returns>
            public async Task<Result<bool>> Handle(ClienteDeleteCommandRequest request, CancellationToken cancellationToken)
            {
                var cliente = await _context.Clientes.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
                if (cliente == null)
                {
                    return Result<bool>.Failure("Cliente no encontrado");
                }

                // Autorización: si no es Admin, sólo su propio cliente
                var identidad = _userAccessor.GetUsername();
                if (string.IsNullOrWhiteSpace(identidad)) return Result<bool>.Failure("Unauthorized");
                var esAdmin = await (from ur in _context.UserRoles
                                     join r in _context.Roles on ur.RoleId equals r.Id
                                     join u in _context.Users on ur.UserId equals u.Id
                                     where (u.Email == identidad || u.UserName == identidad || u.Id.ToString() == identidad) && r.Name == "Admin"
                                     select ur).AnyAsync(cancellationToken);
                if (!esAdmin)
                {
                    var userId = await _context.Users.AsNoTracking()
                        .Where(u => u.Email == identidad || u.UserName == identidad || u.Id.ToString() == identidad)
                        .Select(u => u.Id).FirstOrDefaultAsync(cancellationToken);
                    if (cliente.AppUserId == null || cliente.AppUserId != userId)
                        return Result<bool>.Failure("Forbidden");
                }

                if (!cliente.Estado)
                {
                    // Ya estaba inactivo
                    return Result<bool>.Success(true);
                }

                // Inhabilitar cliente y todas sus cuentas asociadas
                cliente.Estado = false;
                cliente.UpdatedAt = DateTime.UtcNow;
                var cuentas = await _context.Cuentas.Where(c => c.ClienteId == cliente.Id).ToListAsync(cancellationToken);
                foreach (var cta in cuentas)
                {
                    if (cta.Estado != Domain.EstadoCuenta.Inactiva)
                    {
                        cta.Desactivar();
                    }
                }
                await _context.SaveChangesAsync(cancellationToken);

                return Result<bool>.Success(true);
            }
        }
    }
}
