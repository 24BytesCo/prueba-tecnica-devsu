using System.Threading;
using System.Threading.Tasks;
using Bancalite.Application.Core;
using Bancalite.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bancalite.WebApi.Controllers
{
    /// <summary>
    /// Catálogos básicos (Géneros, Tipos de Documento, etc.).
    /// Endpoints de solo lectura que delegan la obtención de datos a la capa Application (CQRS/MediatR).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogosController : ControllerBase
    {
        private readonly ISender _sender;
        public CatalogosController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// Devuelve la lista de géneros activos ordenados por nombre.
        /// </summary>
        [HttpGet("generos")]
        [Authorize]
        public async Task<ActionResult<Result<System.Collections.Generic.List<Bancalite.Application.Catalogos.CatalogoItemDto>>>> GetGeneros(CancellationToken ct)
        {
            var result = await _sender.Send(new Bancalite.Application.Catalogos.GenerosList.GenerosListQuery.GenerosListQueryRequest(), ct);
            return this.FromResult(result);
        }

        /// <summary>
        /// Devuelve la lista de tipos de documento de identidad activos ordenados por nombre.
        /// </summary>
        [HttpGet("tipos-documento")]
        [Authorize]
        public async Task<ActionResult<Result<System.Collections.Generic.List<Bancalite.Application.Catalogos.CatalogoItemDto>>>> GetTiposDocumento(CancellationToken ct)
        {
            var result = await _sender.Send(new Bancalite.Application.Catalogos.TiposDocumentoList.TiposDocumentoListQuery.TiposDocumentoListQueryRequest(), ct);
            return this.FromResult(result);
        }
    }
}
