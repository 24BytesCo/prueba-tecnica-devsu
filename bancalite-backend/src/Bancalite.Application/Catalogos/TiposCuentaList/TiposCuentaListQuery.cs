using Bancalite.Application.Core;
using Bancalite.Persitence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bancalite.Application.Catalogos.TiposCuentaList
{
    /// <summary>
    /// Query para obtener la lista de tipos de cuenta activos.
    /// </summary>
    public class TiposCuentaListQuery
    {
        /// <summary>
        /// Mensaje sin parámetros (solo lectura).
        /// </summary>
        public record TiposCuentaListQueryRequest() : IRequest<Result<List<CatalogoItemDto>>>;

        /// <summary>
        /// Manejador que consulta la base de datos y devuelve los tipos de cuenta activos.
        /// </summary>
        internal class Handler : IRequestHandler<TiposCuentaListQueryRequest, Result<List<CatalogoItemDto>>>
        {
            private readonly BancaliteContext _context;
            public Handler(BancaliteContext context)
            {
                _context = context;
            }

            public async Task<Result<List<CatalogoItemDto>>> Handle(TiposCuentaListQueryRequest request, CancellationToken cancellationToken)
            {
                var datos = await _context.TiposCuenta.AsNoTracking()
                    .Where(t => t.Activo)
                    .OrderBy(t => t.Nombre)
                    .Select(t => new CatalogoItemDto { Id = t.Id, Codigo = t.Codigo, Nombre = t.Nombre, Activo = t.Activo })
                    .ToListAsync(cancellationToken);

                return Result<List<CatalogoItemDto>>.Success(datos);
            }
        }
    }
}

