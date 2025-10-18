using Bancalite.Application.Core;
using Bancalite.Application.Movimientos.MovimientoCreate;
using Bancalite.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Bancalite.Application.Movimientos.MovimientoCreate.MovimientoCreateCommand;
using static Bancalite.Application.Movimientos.MovimientoList.MovimientoListQuery;

namespace Bancalite.WebApi.Controllers
{
    /// <summary>
    /// Controlador para registrar y consultar movimientos de cuentas.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MovimientosController : ControllerBase
    {
        private readonly ISender _sender;
        public MovimientosController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// Registra un movimiento (Crédito/Débito) aplicando reglas de negocio.
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Result<MovimientoDto>>> Crear([FromBody] MovimientoCreateRequest request, CancellationToken ct)
        {
            var result = await _sender.Send(new MovimientoCreateCommandRequest(request), ct);
            if (result.IsSuccess)
            {
                // 201 para creación de recurso
                return Created(string.Empty, result.Datos);
            }
            return this.FromResult(result);
        }

        /// <summary>
        /// Lista movimientos por cuenta y rango de fechas.
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<Result<IReadOnlyList<Item>>>> Listar([FromQuery] string numeroCuenta, [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta, CancellationToken ct)
        {
            var result = await _sender.Send(new MovimientoListQueryRequest(numeroCuenta, desde, hasta), ct);
            return this.FromResult(result);
        }
    }
}
