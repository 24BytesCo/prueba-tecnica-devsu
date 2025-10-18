using Bancalite.Application.Core;
using Bancalite.Persitence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bancalite.Application.Catalogos.TiposMovimientoList
{
    /// <summary>
    /// Query para obtener la lista de tipos de movimiento activos (ej.: Débito, Crédito).
    /// </summary>
    public class TiposMovimientoListQuery
    {
        /// <summary>
        /// Mensaje sin parámetros (solo lectura).
        /// </summary>
        public record TiposMovimientoListQueryRequest() : IRequest<Result<List<CatalogoItemDto>>>;

        /// <summary>
        /// Manejador que consulta la base de datos y devuelve los tipos de movimiento activos.
        /// </summary>
        internal class Handler : IRequestHandler<TiposMovimientoListQueryRequest, Result<List<CatalogoItemDto>>>
        {
            private readonly BancaliteContext _context;
            public Handler(BancaliteContext context)
            {
                _context = context;
            }

            public async Task<Result<List<CatalogoItemDto>>> Handle(TiposMovimientoListQueryRequest request, CancellationToken cancellationToken)
            {
                var datos = await _context.TiposMovimiento.AsNoTracking()
                    .Where(t => t.Activo)
                    .OrderBy(t => t.Nombre)
                    .Select(t => new CatalogoItemDto { Id = t.Id, Codigo = t.Codigo, Nombre = t.Nombre, Activo = t.Activo })
                    .ToListAsync(cancellationToken);

                return Result<List<CatalogoItemDto>>.Success(datos);
            }
        }
    }
}

