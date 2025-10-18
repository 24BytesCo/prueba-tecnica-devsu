using Bancalite.Application.Core;
using Bancalite.Persitence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bancalite.Application.Catalogos.GenerosList
{
    /// <summary>
    /// Query para obtener la lista de géneros activos.
    /// </summary>
    public class GenerosListQuery
    {
        /// <summary>
        /// Mensaje sin parámetros (solo lectura).
        /// </summary>
        public record GenerosListQueryRequest() : IRequest<Result<List<CatalogoItemDto>>>;

        /// <summary>
        /// Manejador que consulta la base de datos y devuelve la lista de géneros.
        /// </summary>
        internal class Handler : IRequestHandler<GenerosListQueryRequest, Result<List<CatalogoItemDto>>>
        {
            private readonly BancaliteContext _context;
            public Handler(BancaliteContext context)
            {
                _context = context;
            }

            public async Task<Result<List<CatalogoItemDto>>> Handle(GenerosListQueryRequest request, CancellationToken cancellationToken)
            {
                var datos = await _context.Generos.AsNoTracking()
                    .Where(g => g.Activo)
                    .OrderBy(g => g.Nombre)
                    .Select(g => new CatalogoItemDto { Id = g.Id, Codigo = g.Codigo, Nombre = g.Nombre, Activo = g.Activo })
                    .ToListAsync(cancellationToken);

                return Result<List<CatalogoItemDto>>.Success(datos);
            }
        }
    }
}

