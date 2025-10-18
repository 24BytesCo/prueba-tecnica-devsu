using Bancalite.Application.Catalogos;
using Bancalite.Application.Core;
using Bancalite.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Bancalite.Application.Catalogos.TiposCuentaList.TiposCuentaListQuery;
using static Bancalite.Application.Catalogos.TiposDocumentoList.TiposDocumentoListQuery;
using static Bancalite.Application.Catalogos.TiposMovimientoList.TiposMovimientoListQuery;
using GenerosListQuery = Bancalite.Application.Catalogos.GenerosList.GenerosListQuery;

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
        public async Task<ActionResult<Result<List<CatalogoItemDto>>>> GetGeneros(CancellationToken ct)
        {
            var result = await _sender.Send(new GenerosListQuery.GenerosListQueryRequest(), ct);
            return this.FromResult(result);
        }

        /// <summary>
        /// Devuelve la lista de tipos de documento de identidad activos ordenados por nombre.
        /// </summary>
        [HttpGet("tipos-documento")]
        [Authorize]
        public async Task<ActionResult<Result<List<CatalogoItemDto>>>> GetTiposDocumento(CancellationToken ct)
        {
            var result = await _sender.Send(new TiposDocumentoListQueryRequest(), ct);
            return this.FromResult(result);
        }

        /// <summary>
        /// Devuelve la lista de tipos de cuenta activos ordenados por nombre.
        /// </summary>
        [HttpGet("tipos-cuenta")]
        [Authorize]
        public async Task<ActionResult<Result<List<CatalogoItemDto>>>> GetTiposCuenta(CancellationToken ct)
        {
            var result = await _sender.Send(new TiposCuentaListQueryRequest(), ct);
            return this.FromResult(result);
        }

        /// <summary>
        /// Devuelve la lista de tipos de movimiento activos ordenados por nombre.
        /// </summary>
        [HttpGet("tipos-movimiento")]
        [Authorize]
        public async Task<ActionResult<Result<List<CatalogoItemDto>>>> GetTiposMovimiento(CancellationToken ct)
        {
            var result = await _sender.Send(new TiposMovimientoListQueryRequest(), ct);
            return this.FromResult(result);
        }
    }
}
