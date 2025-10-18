using AutoMapper;
using Bancalite.Application.Core;
using Bancalite.Persitence;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
        /// <param name="NumeroDocumento">Filtro por número de documento (prefijo).</param>
        /// <param name="Estado">Filtro por estado del cliente (true=activo, false=inactivo).</param>
        public record ClienteListQueryRequest(
            int Pagina = 1,
            int Tamano = 10,
            string? Nombres = null,
            string? NumeroDocumento = null,
            bool? Estado = null
        ) : IRequest<Result<Paged<ClienteListItem>>>;

        /// <summary>
        /// Manejador de la consulta de clientes.
        /// </summary>
        internal class Handler : IRequestHandler<ClienteListQueryRequest, Result<Paged<ClienteListItem>>>
        {
            private readonly BancaliteContext _context;
            private readonly IMapper _mapeador;

            public Handler(BancaliteContext context, IMapper mapeador)
            {
                _context = context;
                _mapeador = mapeador;
            }

            /// <summary>
            /// Ejecuta la consulta y retorna la lista de clientes mapeada a DTOs con paginación.
            /// </summary>
            /// <param name="request">Parámetros de paginación y filtros.</param>
            /// <param name="cancellationToken">Token de cancelación.</param>
            /// <returns>Resultado con elementos y metadatos de paginación.</returns>
            public async Task<Result<Paged<ClienteListItem>>> Handle(ClienteListQueryRequest request, CancellationToken cancellationToken)
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
                        // Coincidencia por prefijo para permitir búsqueda incremental (ej: "1061%")
                        query = query.Where(c => c.Persona.NumeroDocumento.StartsWith(ndoc));
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

                    // Proyección a DTO con AutoMapper (sin rol por ahora)
                    var entidades = await query.Skip(skip).Take(tamano)
                        .Include(c => c.Persona).ThenInclude(p => p.Genero)
                        .Include(c => c.Persona).ThenInclude(p => p.TipoDocumentoIdentidad)
                        .ToListAsync(cancellationToken);
                    var items = _mapeador.Map<List<ClienteListItem>>(entidades);

                    // Enriquecer con rol primer rol usando consulta única
                    var usuarios = entidades.Where(c => c.AppUserId != null).Select(c => c.AppUserId!.Value).Distinct().ToList();
                    if (usuarios.Count > 0)
                    {
                        var rolesPorUsuario = (from ur in _context.UserRoles
                                               join r in _context.Roles on ur.RoleId equals r.Id
                                               where usuarios.Contains(ur.UserId)
                                               select new { ur.UserId, ur.RoleId, r.Name })
                                              .AsEnumerable()
                                              .GroupBy(x => x.UserId)
                                              .ToDictionary(g => g.Key, g => g.First());

                        for (int i = 0; i < entidades.Count; i++)
                        {
                            var entidad = entidades[i];
                            var dto = items[i];
                            if (entidad.AppUserId != null && rolesPorUsuario.TryGetValue(entidad.AppUserId.Value, out var rol))
                            {
                                dto.RolId = rol.RoleId;
                                dto.RolNombre = rol.Name;
                            }
                        }
                    }

                    var paged = new Paged<ClienteListItem>
                    {
                        Items = items,
                        Total = total,
                        Pagina = pagina,
                        Tamano = tamano
                    };

                    return Result<Paged<ClienteListItem>>.Success(paged);
                }
                catch (Exception ex)
                {
                    // Error inesperado al consultar
                    return Result<Paged<ClienteListItem>>.Failure("No se pudo obtener el listado de clientes");
                }
            }
        }
    }
}




