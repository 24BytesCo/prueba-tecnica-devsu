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
    /// Query para obtener el listado de clientes con paginación y filtros básicos.
    /// </summary>
    public class ClienteListQuery
    {
        /// <summary>
        /// Filtros y paginación para listar clientes.
        /// </summary>
        /// <param name="Pagina">Número de página (1-based).</param>
        /// <param name="Tamano">Tamaño de página (cantidad de registros).</param>
        /// <param name="Nombres">Filtro por nombre/apellido (contiene) en Persona.</param>
        /// <param name="NumeroDocumento">Filtro por número de documento (exacto).</param>
        /// <param name="Estado">Filtro por estado del cliente (true=activo, false=inactivo).</param>
        public record ClienteListQueryRequest(
            int Pagina = 1,
            int Tamano = 10,
            string? Nombres = null,
            string? NumeroDocumento = null,
            bool? Estado = null
        ) : IRequest<Bancalite.Application.Core.Result<Bancalite.Application.Core.Paged<ClienteListItem>>>;

        /// <summary>
        /// Manejador de la consulta de clientes.
        /// </summary>
        internal class Handler : IRequestHandler<ClienteListQueryRequest, Bancalite.Application.Core.Result<Bancalite.Application.Core.Paged<ClienteListItem>>> 
        {
            private readonly BancaliteContext _context;

            public Handler(BancaliteContext context)
            {
                _context = context;
            }

            /// <summary>
            /// Ejecuta la consulta y retorna la lista de clientes mapeada a DTOs con paginación.
            /// </summary>
            /// <param name="request">Parámetros de paginación y filtros.</param>
            /// <param name="cancellationToken">Token de cancelación.</param>
            /// <returns>Resultado con elementos y metadatos de paginación.</returns>
            public async Task<Bancalite.Application.Core.Result<Bancalite.Application.Core.Paged<ClienteListItem>>> Handle(ClienteListQueryRequest request, CancellationToken cancellationToken)
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
                        .AsQueryable();

                    // Filtros básicos
                    if (!string.IsNullOrWhiteSpace(request.Nombres))
                    {
                        var nombre = request.Nombres.Trim().ToLower();
                        query = query.Where(c => (c.Persona.Nombres + " " + c.Persona.Apellidos).ToLower().Contains(nombre));
                    }

                    if (!string.IsNullOrWhiteSpace(request.NumeroDocumento))
                    {
                        var ndoc = request.NumeroDocumento.Trim();
                        query = query.Where(c => c.Persona.NumeroDocumento == ndoc);
                    }

                    if (request.Estado.HasValue)
                    {
                        query = query.Where(c => c.Estado == request.Estado.Value);
                    }

                    // Total antes de paginar
                    var total = await query.CountAsync(cancellationToken);

                    // Ordenar por Apellidos, Nombres de manera estable
                    query = query
                        .OrderBy(c => c.Persona.Apellidos)
                        .ThenBy(c => c.Persona.Nombres);

                    // Paginación (1-based)
                    var pagina = request.Pagina <= 0 ? 1 : request.Pagina;
                    var tamano = request.Tamano <= 0 ? 10 : request.Tamano;
                    var skip = (pagina - 1) * tamano;

                    var items = await query
                        .Skip(skip)
                        .Take(tamano)
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
                        })
                        .ToListAsync(cancellationToken);

                    var paged = new Bancalite.Application.Core.Paged<ClienteListItem>
                    {
                        Items = items,
                        Total = total,
                        Pagina = pagina,
                        Tamano = tamano
                    };

                    return Bancalite.Application.Core.Result<Bancalite.Application.Core.Paged<ClienteListItem>>.Success(paged);
                }
                catch (Exception ex)
                {
                    // Error inesperado al consultar
                    return Bancalite.Application.Core.Result<Bancalite.Application.Core.Paged<ClienteListItem>>.Failure("No se pudo obtener el listado de clientes");
                }
            }
        }
    }
}




