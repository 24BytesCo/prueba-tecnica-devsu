using System;
using System.Threading;
using System.Threading.Tasks;
using Bancalite.Persitence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bancalite.Application.Clientes.ClienteDelete
{
    /// <summary>
    /// Comando para realizar soft-delete del cliente (Estado = false).
    /// </summary>
    public class ClienteDeleteCommand
    {
        /// <summary>
        /// Solicitud con el Id del cliente a eliminar lógicamente.
        /// </summary>
        /// <param name="Id">Identificador del cliente a desactivar.</param>
        public record ClienteDeleteCommandRequest(Guid Id) : IRequest<Bancalite.Application.Core.Result<bool>>;

        internal class Handler : IRequestHandler<ClienteDeleteCommandRequest, Bancalite.Application.Core.Result<bool>>
        {
            private readonly BancaliteContext _context;

            public Handler(BancaliteContext context)
            {
                _context = context;
            }

            /// <summary>
            /// Marca el cliente como inactivo (soft-delete).
            /// </summary>
            /// <param name="request">Id del cliente a desactivar.</param>
            /// <param name="cancellationToken">Token de cancelación.</param>
            /// <returns>Resultado indicando éxito o error.</returns>
            public async Task<Bancalite.Application.Core.Result<bool>> Handle(ClienteDeleteCommandRequest request, CancellationToken cancellationToken)
            {
                var cliente = await _context.Clientes.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
                if (cliente == null)
                {
                    return Bancalite.Application.Core.Result<bool>.Failure("Cliente no encontrado");
                }

                if (!cliente.Estado)
                {
                    // Ya estaba inactivo
                    return Bancalite.Application.Core.Result<bool>.Success(true);
                }

                cliente.Estado = false;
                cliente.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);

                return Bancalite.Application.Core.Result<bool>.Success(true);
            }
        }
    }
}
