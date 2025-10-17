using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bancalite.Persitence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Bancalite.Application.Clientes.ClienteList
{
    /// <summary>
    /// Query para obtener el listado de clientes.
    /// </summary>
    public class ClienteListQuery
    {
        /// <summary>
        /// Solicitud para listar clientes (sin filtros por ahora).
        /// </summary>
        public record ClienteListQueryRequest() : IRequest<Bancalite.Application.Core.Result<IReadOnlyList<ClienteListItem>>>;

        /// <summary>
        /// Manejador de la consulta de clientes.
        /// </summary>
        internal class Handler : IRequestHandler<ClienteListQueryRequest, Bancalite.Application.Core.Result<IReadOnlyList<ClienteListItem>>> 
        {
            private readonly BancaliteContext _context;

            public Handler(BancaliteContext context)
            {
                _context = context;
            }

            /// <summary>
            /// Ejecuta la consulta y retorna la lista de clientes mapeada a DTOs.
            /// </summary>
            public async Task<Bancalite.Application.Core.Result<IReadOnlyList<ClienteListItem>>> Handle(ClienteListQueryRequest request, CancellationToken cancellationToken)
            {
                try
                {
                    // Consulta incluye persona para proyectar datos
                    var query = _context.Clientes
                        .AsNoTracking()
                        .Include(c => c.Persona)
                        .ThenInclude(p => p.Genero)
                        .Include(c => c.Persona)
                        .ThenInclude(p => p.TipoDocumentoIdentidad)
                        .Select(c => new ClienteListItem
                        {
                            ClienteId = c.Id,
                            PersonaId = c.PersonaId,
                            Nombres = c.Persona.Nombres,
                            Apellidos = c.Persona.Apellidos,
                            Edad = c.Persona.Edad,
                            GeneroId = c.Persona.GeneroId,
                            GeneroNombre = c.Persona.Genero.Nombre,
                            GeneroCodigo = c.Persona.Genero.Codigo,
                            TipoDocumentoIdentidadId = c.Persona.TipoDocumentoIdentidadId,
                            TipoDocumentoIdentidadNombre = c.Persona.TipoDocumentoIdentidad.Nombre,
                            TipoDocumentoIdentidadCodigo = c.Persona.TipoDocumentoIdentidad.Codigo,
                            NumeroDocumento = c.Persona.NumeroDocumento,
                            Direccion = c.Persona.Direccion,
                            Telefono = c.Persona.Telefono,
                            Email = c.Persona.Email,
                            Estado = c.Estado,
                            // Rol (si hay AppUser vinculado, tomamos el primer rol asignado)
                            RolId = _context.UserRoles
                                .Where(ur => c.AppUserId != null && ur.UserId == c.AppUserId)
                                .Select(ur => (Guid?)ur.RoleId)
                                .FirstOrDefault(),
                            RolNombre = (from ur in _context.UserRoles
                                         join r in _context.Roles on ur.RoleId equals r.Id
                                         where c.AppUserId != null && ur.UserId == c.AppUserId
                                         select r.Name).FirstOrDefault()
                        });

                    // Ejecutar y retornar
                    var data = await query.ToListAsync(cancellationToken);
                    IReadOnlyList<ClienteListItem> ro = data; return Bancalite.Application.Core.Result<IReadOnlyList<ClienteListItem>>.Success(ro);
                }
                catch (Exception ex)
                {
                    // Error inesperado al consultar
                    return Bancalite.Application.Core.Result<IReadOnlyList<ClienteListItem>>.Failure("No se pudo obtener el listado de clientes");
                }
            }
        }
    }
}




