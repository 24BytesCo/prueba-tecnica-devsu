using MediatR;
using Microsoft.AspNetCore.Mvc;
using Bancalite.Application.Clientes.ClienteCreate;
using Bancalite.Application.Clientes.ClienteList;
using Microsoft.AspNetCore.Authorization;

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
        public async Task<ActionResult<Bancalite.Application.Core.Result<Guid>>> CreateCliente([FromForm] ClienteCreateRequest request, CancellationToken cancellationToken)
        {
            // Enviar comando a Application (CQRS)
            var command = new ClienteCreateCommand.ClienteCreateCommandRequest(request);
            
            var result = await _sender.Send(command, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Lista todos los clientes.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Listado de clientes.</returns>
        [HttpGet]
        public async Task<ActionResult<Bancalite.Application.Core.Result<IReadOnlyList<ClienteListItem>>>> GetClientes(CancellationToken cancellationToken)
        {
            // Enviar query para obtener cliente
            var query = new ClienteListQuery.ClienteListQueryRequest();
            var result = await _sender.Send(query, cancellationToken);
            return Ok(result);
        }

    }
}

