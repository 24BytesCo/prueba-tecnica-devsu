using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Bancalite.Application.Cuentas.CuentaCreate;
using Bancalite.Application.Cuentas.CuentaDelete;
using Bancalite.Application.Cuentas.CuentaEstado;
using Bancalite.Application.Cuentas.CuentaList;
using Bancalite.Application.Cuentas.CuentaResponse;
using Bancalite.Application.Cuentas.GetCuenta;
using Bancalite.Application.Cuentas.CuentaUpdate;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bancalite.Application.Core;
using static Bancalite.Application.Cuentas.CuentaUpdate.CuentaUpdateCommand;
using static Bancalite.Application.Cuentas.CuentaCreate.CuentaCreateCommand;
using static Bancalite.Application.Cuentas.CuentaDelete.CuentaDeleteCommand;
using static Bancalite.Application.Cuentas.CuentaEstado.CuentaEstadoPatchCommand;
using static Bancalite.Application.Cuentas.CuentaList.CuentaListQuery;
using static Bancalite.Application.Cuentas.GetCuenta.GetCuentaQuery;
using Bancalite.Application.Cuentas.MisCuentas;
using Bancalite.WebApi.Extensions;

namespace Bancalite.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CuentasController : ControllerBase
    {
        private readonly ISender _sender;

        public CuentasController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// Lista cuentas con paginación y filtros.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result<Paged<CuentaListItem>>>> Listar(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamano = 10,
            [FromQuery] Guid? clienteId = null,
            [FromQuery] string? estado = null,
            CancellationToken ct = default)
        {
            var result = await _sender.Send(new CuentaListQueryRequest(pagina, tamano, clienteId, estado), ct);
            return this.FromResult(result);
        }

        /// <summary>
        /// Mis cuentas (según el usuario autenticado).
        /// </summary>
        [HttpGet("mias")]
        [Authorize]
        public async Task<ActionResult<Result<IReadOnlyList<CuentaListItem>>>> MisCuentas(CancellationToken ct)
        {
            var result = await _sender.Send(new MisCuentasQuery.MisCuentasQueryRequest(), ct);
            return this.FromResult(result);
        }

        /// <summary>
        /// Obtiene el detalle de una cuenta por Id.
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Result<CuentaDto>>> Obtener(Guid id, CancellationToken ct)
        {
            var result = await _sender.Send(new GetCuentaQueryRequest(id), ct);
            return this.FromResult(result);
        }

        /// <summary>
        /// Abre una nueva cuenta.
        /// </summary>
        [HttpPost]
        // Requiere estar autenticado
        [Authorize] 
        public async Task<ActionResult<Result<Guid>>> Crear([FromBody] CuentaCreateRequest request, CancellationToken ct)
        {
            var result = await _sender.Send(new CuentaCreateCommandRequest(request), ct);
            return this.FromResult(result);
        }

        /// <summary>
        /// Actualiza totalmente una cuenta.
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result<bool>>> Put(Guid id, [FromBody] CuentaPutRequest request, CancellationToken ct)
        {
            var result = await _sender.Send(new CuentaPutCommandRequest(id, request), ct);
            return this.FromResult(result);
        }

        /// <summary>
        /// Actualiza parcialmente una cuenta.
        /// </summary>
        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result<bool>>> Patch(Guid id, [FromBody] CuentaPatchRequest request, CancellationToken ct)
        {
            var result = await _sender.Send(new CuentaPatchCommandRequest(id, request), ct);
            return this.FromResult(result);
        }

        /// <summary>
        /// Cambia el estado de una cuenta.
        /// </summary>
        [HttpPatch("{id}/estado")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result<bool>>> CambiarEstado(Guid id, [FromBody] CuentaEstadoPatchRequest request, CancellationToken ct)
        {
            var result = await _sender.Send(new CuentaEstadoPatchCommandRequest(id, request), ct);
            return this.FromResult(result);
        }

        /// <summary>
        /// Soft-delete: inactiva una cuenta si no tiene saldo.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result<bool>>> Eliminar(Guid id, CancellationToken ct)
        {
            var result = await _sender.Send(new CuentaDeleteCommandRequest(id), ct);
            return this.FromResult(result);
        }
    }
}
