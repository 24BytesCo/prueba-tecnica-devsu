using System;
using System.Threading;
using System.Threading.Tasks;
using Bancalite.Application.Core;
using Bancalite.Persitence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bancalite.Application.Cuentas.CuentaDelete
{
    /// <summary>
    /// Soft-delete de cuenta: se marca Inactiva si es posible.
    /// </summary>
    public class CuentaDeleteCommand
    {
        public record CuentaDeleteCommandRequest(Guid Id) : IRequest<Result<bool>>;

        internal class Handler : IRequestHandler<CuentaDeleteCommandRequest, Result<bool>>
        {
            private readonly BancaliteContext _context;
            /// <summary>
            /// Crea un handler para el borrado lógico de cuentas.
            /// </summary>
            public Handler(BancaliteContext context) { _context = context; }

            /// <summary>
            /// Inactiva la cuenta si el saldo actual es 0.
            /// </summary>
            /// <param name="request">Id de cuenta a inactivar.</param>
            /// <param name="cancellationToken">Token de cancelación.</param>
            /// <returns>True si se inactiva; 422 si saldo ≠ 0.</returns>
            public async Task<Result<bool>> Handle(CuentaDeleteCommandRequest request, CancellationToken cancellationToken)
            {
                var cuenta = await _context.Cuentas.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
                if (cuenta == null) return Result<bool>.Failure("No encontrado");

                // Regla: solo inactivar si saldo es 0
                if (cuenta.SaldoActual != 0)
                    return Result<bool>.Failure("Unprocessable: No se puede eliminar (inactivar) una cuenta con saldo distinto de 0");

                cuenta.Desactivar();
                cuenta.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
                return Result<bool>.Success(true);
            }
        }
    }
}
