using System;
using System.Threading;
using System.Threading.Tasks;
using Bancalite.Application.Core;
using Bancalite.Persitence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bancalite.Application.Cuentas.CuentaEstado
{
    /// <summary>
    /// Comando para cambio de estado de la cuenta.
    /// </summary>
    public class CuentaEstadoPatchCommand
    {
        public record CuentaEstadoPatchCommandRequest(Guid Id, CuentaEstadoPatchRequest Request) : IRequest<Result<bool>>;

        internal class Handler : IRequestHandler<CuentaEstadoPatchCommandRequest, Result<bool>>
        {
            private readonly BancaliteContext _context;
            /// <summary>
            /// Crea un handler para cambio de estado de cuenta.
            /// </summary>
            public Handler(BancaliteContext context) { _context = context; }

            /// <summary>
            /// Aplica el cambio de estado controlando reglas de negocio.
            /// </summary>
            /// <param name="request">Id y estado destino.</param>
            /// <param name="cancellationToken">Token de cancelación.</param>
            /// <returns>True si se aplicó; 422 si no cumple reglas.</returns>
            public async Task<Result<bool>> Handle(CuentaEstadoPatchCommandRequest request, CancellationToken cancellationToken)
            {
                var cuenta = await _context.Cuentas.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
                if (cuenta == null) return Result<bool>.Failure("No encontrado");

                var destino = request.Request.Estado.Trim();

                // Regla de negocio: para pasar a Inactiva (cierre lógico), saldo debe ser 0
                if (destino.Equals("Inactiva", StringComparison.OrdinalIgnoreCase) && cuenta.SaldoActual != 0)
                {
                    return Result<bool>.Failure("Unprocessable: No se puede inactivar/cerrar una cuenta con saldo distinto de 0");
                }

                if (destino.Equals("Activa", StringComparison.OrdinalIgnoreCase)) cuenta.Activar();
                else if (destino.Equals("Inactiva", StringComparison.OrdinalIgnoreCase)) cuenta.Desactivar();
                else if (destino.Equals("Bloqueada", StringComparison.OrdinalIgnoreCase)) cuenta.Bloquear();

                cuenta.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
                return Result<bool>.Success(true);
            }
        }
    }
}
