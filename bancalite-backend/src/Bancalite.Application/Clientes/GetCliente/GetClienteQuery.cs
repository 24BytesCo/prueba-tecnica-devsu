using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bancalite.Persitence;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
        public record GetClienteQueryRequest(Guid Id) : IRequest<Bancalite.Application.Core.Result<ClienteDto>>;

        internal class Handler : IRequestHandler<GetClienteQueryRequest, Bancalite.Application.Core.Result<ClienteDto>>
        {
            private readonly BancaliteContext _context;

            public Handler(BancaliteContext context)
            {
                _context = context;
            }

            /// <summary>
            /// Ejecuta la consulta y devuelve el detalle del cliente.
            /// </summary>
            /// <param name="request">Request con el identificador del cliente.</param>
            /// <param name="cancellationToken">Token de cancelación.</param>
            /// <returns>Resultado con el DTO del cliente, o error si no existe.</returns>
            public async Task<Bancalite.Application.Core.Result<ClienteDto>> Handle(GetClienteQueryRequest request, CancellationToken cancellationToken)
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
                    return Bancalite.Application.Core.Result<ClienteDto>.Failure("Cliente no encontrado");
                }

                var dto = new ClienteDto
                {
                    ClienteId = c.Id,
                    PersonaId = c.PersonaId,
                    Nombres = c.Persona.Nombres,
                    Apellidos = c.Persona.Apellidos,
                    Edad = c.Persona.Edad,
                    GeneroId = c.Persona.GeneroId,
                    GeneroNombre = c.Persona.Genero.Nombre,
                    TipoDocumentoIdentidadId = c.Persona.TipoDocumentoIdentidadId,
                    TipoDocumentoIdentidadNombre = c.Persona.TipoDocumentoIdentidad.Nombre,
                    NumeroDocumento = c.Persona.NumeroDocumento,
                    Direccion = c.Persona.Direccion,
                    Telefono = c.Persona.Telefono,
                    Email = c.Persona.Email,
                    Estado = c.Estado,
                    RolId = _context.UserRoles
                        .Where(ur => c.AppUserId != null && ur.UserId == c.AppUserId)
                        .Select(ur => (Guid?)ur.RoleId)
                        .FirstOrDefault(),
                    RolNombre = (from ur in _context.UserRoles
                                 join r in _context.Roles on ur.RoleId equals r.Id
                                 where c.AppUserId != null && ur.UserId == c.AppUserId
                                 select r.Name).FirstOrDefault()
                };

                return Bancalite.Application.Core.Result<ClienteDto>.Success(dto);
            }
        }
    }
}
