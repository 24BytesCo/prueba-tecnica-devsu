using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bancalite.Persitence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Bancalite.Application.Core;
using Bancalite.Application.Interface;

namespace Bancalite.Application.Clientes.GetCliente
{
    /// <summary>
    /// Query para obtener el detalle de un cliente por su Id.
    /// </summary>
    public class GetClienteQuery
    {
        /// <summary>
        /// Solicitud con el Id del cliente.
        /// </summary>
        /// <param name="Id">Identificador del cliente a consultar.</param>
        public record GetClienteQueryRequest(Guid Id) : IRequest<Result<ClienteDto>>;

        internal class Handler : IRequestHandler<GetClienteQueryRequest, Result<ClienteDto>>
        {
            private readonly BancaliteContext _context;
            private readonly IMapper _mapeador;
            private readonly IUserAccessor _userAccessor;

            public Handler(BancaliteContext context, IMapper mapeador, IUserAccessor userAccessor)
            {
                _context = context;
                _mapeador = mapeador;
                _userAccessor = userAccessor;
            }

            /// <summary>
            /// Ejecuta la consulta y devuelve el detalle del cliente.
            /// </summary>
            /// <param name="request">Request con el identificador del cliente.</param>
            /// <param name="cancellationToken">Token de cancelación.</param>
            /// <returns>Resultado con el DTO del cliente, o error si no existe.</returns>
            public async Task<Result<ClienteDto>> Handle(GetClienteQueryRequest request, CancellationToken cancellationToken)
            {
                var c = await _context.Clientes
                    .AsNoTracking()
                    .Include(x => x.Persona)
                        .ThenInclude(p => p.Genero)
                    .Include(x => x.Persona)
                        .ThenInclude(p => p.TipoDocumentoIdentidad)
                    .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

                if (c == null)
                {
                    return Result<ClienteDto>.Failure("Cliente no encontrado");
                }

                // Autorización: si no es Admin, sólo su propio cliente
                var identidad = _userAccessor.GetUsername();
                if (string.IsNullOrWhiteSpace(identidad))
                    return Result<ClienteDto>.Failure("Unauthorized");

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
                    if (c.AppUserId == null || c.AppUserId != userId)
                        return Result<ClienteDto>.Failure("Forbidden");
                }

                var dto = _mapeador.Map<ClienteDto>(c);
                // Agregar rol si existe vínculo con Identity
                dto.RolId = _context.UserRoles
                    .Where(ur => c.AppUserId != null && ur.UserId == c.AppUserId)
                    .Select(ur => (Guid?)ur.RoleId)
                    .FirstOrDefault();
                dto.RolNombre = (from ur in _context.UserRoles
                                 join r in _context.Roles on ur.RoleId equals r.Id
                                 where c.AppUserId != null && ur.UserId == c.AppUserId
                                 select r.Name).FirstOrDefault();

                return Result<ClienteDto>.Success(dto);
            }
        }
    }
}
