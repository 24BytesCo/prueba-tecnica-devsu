using MediatR;
using Microsoft.AspNetCore.Mvc;
using Bancalite.Application.Clientes.ClienteCreate;
using Bancalite.Application.Clientes.ClienteList;
using Microsoft.AspNetCore.Authorization;
using Bancalite.Application.Clientes.GetCliente;
using Bancalite.Application.Clientes.ClienteUpdate;
using Bancalite.Application.Clientes.ClienteDelete;

namespace Bancalite.WebApi.Controllers
{
    /// <summary>
    /// Controlador para operaciones relacionadas con clientes.
    /// </summary>
    /// <remarks>
    /// Expone endpoints para crear clientes y orquesta comandos via MediatR.
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesController : ControllerBase
    {
        private readonly ISender _sender;
        
        /// <summary>
        /// Inicializa el controlador de clientes.
        /// </summary>
        /// <param name="sender">MediatR sender para enviar comandos/queries.</param>
        public ClientesController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// Crea un nuevo cliente.
        /// </summary>
        /// <param name="request">Datos de creación del cliente.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Id del cliente creado.</returns>
        /// <remarks>
        /// Envía un comando de creación al Application layer utilizando MediatR.
        /// </remarks>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Bancalite.Application.Core.Result<Guid>>> CreateCliente([FromBody] ClienteCreateRequest request, CancellationToken cancellationToken)
        {
            // Enviar comando a Application (CQRS)
            var command = new ClienteCreateCommand.ClienteCreateCommandRequest(request);
            
            var result = await _sender.Send(command, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Lista clientes con paginación y filtros básicos.
        /// </summary>
        /// <param name="pagina">Página (1-based).</param>
        /// <param name="tamano">Tamaño de página.</param>
        /// <param name="nombres">Filtro por nombre/apellido (contiene).</param>
        /// <param name="numeroDocumento">Filtro por número de documento (exacto).</param>
        /// <param name="estado">Estado del cliente (true=activo, false=inactivo).</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Listado paginado de clientes.</returns>
        [HttpGet]
        public async Task<ActionResult<Bancalite.Application.Core.Result<Bancalite.Application.Core.Paged<ClienteListItem>>>> GetClientes(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamano = 10,
            [FromQuery] string? nombres = null,
            [FromQuery] string? numeroDocumento = null,
            [FromQuery] bool? estado = null,
            CancellationToken cancellationToken = default)
        {
            // Enviar query para obtener clientes
            var query = new ClienteListQuery.ClienteListQueryRequest(pagina, tamano, nombres, numeroDocumento, estado);
            var result = await _sender.Send(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene el detalle de un cliente por Id.
        /// </summary>
        /// <param name="id">Id del cliente.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<Bancalite.Application.Core.Result<ClienteDto>>> GetClienteById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _sender.Send(new GetClienteQuery.GetClienteQueryRequest(id), cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Actualiza totalmente un cliente (PUT).
        /// </summary>
        /// <param name="id">Id del cliente a actualizar.</param>
        /// <param name="request">Datos completos del cliente a aplicar.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Bancalite.Application.Core.Result<bool>>> PutCliente(Guid id, [FromBody] ClientePutRequest request, CancellationToken cancellationToken)
        {
            var result = await _sender.Send(new ClienteUpdateCommand.ClientePutCommandRequest(id, request), cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Actualiza parcialmente un cliente (PATCH).
        /// </summary>
        /// <param name="id">Id del cliente a actualizar.</param>
        /// <param name="request">Campos parciales del cliente.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Bancalite.Application.Core.Result<bool>>> PatchCliente(Guid id, [FromBody] ClientePatchRequest request, CancellationToken cancellationToken)
        {
            var result = await _sender.Send(new ClienteUpdateCommand.ClientePatchCommandRequest(id, request), cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Elimina lógicamente (soft-delete) un cliente (Estado=false).
        /// </summary>
        /// <param name="id">Id del cliente a desactivar.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Bancalite.Application.Core.Result<bool>>> DeleteCliente(Guid id, CancellationToken cancellationToken)
        {
            var result = await _sender.Send(new ClienteDeleteCommand.ClienteDeleteCommandRequest(id), cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }
    }
}

