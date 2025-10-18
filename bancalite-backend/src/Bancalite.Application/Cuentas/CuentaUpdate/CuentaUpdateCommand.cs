using Bancalite.Application.Core;
using Bancalite.Persitence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bancalite.Application.Cuentas.CuentaUpdate
{
    /// <summary>
    /// Comandos para actualización total/parcial de cuentas.
    /// </summary>
    public class CuentaUpdateCommand
    {
        /// <summary>
        /// PUT: actualiza todos los campos básicos de la cuenta.
        /// </summary>
        /// <param name="Id">Identificador de la cuenta a actualizar.</param>
        /// <param name="Request">Contenido completo a aplicar.</param>
        public record CuentaPutCommandRequest(Guid Id, CuentaPutRequest Request) : IRequest<Result<bool>>;

        /// <summary>
        /// PATCH: actualiza parcialmente la cuenta (solo campos presentes).
        /// </summary>
        /// <param name="Id">Identificador de la cuenta a actualizar.</param>
        /// <param name="Request">Campos parciales.</param>
        public record CuentaPatchCommandRequest(Guid Id, CuentaPatchRequest Request) : IRequest<Result<bool>>;

        internal class PutHandler : IRequestHandler<CuentaPutCommandRequest, Result<bool>>
        {
            private readonly BancaliteContext _context;
            /// <summary>
            /// Crea un handler para PUT de cuenta.
            /// </summary>
            public PutHandler(BancaliteContext context) { _context = context; }

            /// <summary>
            /// Aplica actualización total validando unicidad de número y FKs.
            /// </summary>
            /// <param name="request">Id y datos completos.</param>
            /// <param name="cancellationToken">Token de cancelación.</param>
            /// <returns>True si se actualiza; error si no existe o hay conflicto.</returns>
            public async Task<Result<bool>> Handle(CuentaPutCommandRequest request, CancellationToken cancellationToken)
            {
                var cuenta = await _context.Cuentas.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
                if (cuenta == null) return Result<bool>.Failure("No encontrado");

                // Unicidad de numeroCuenta
                var dup = await _context.Cuentas.AsNoTracking().AnyAsync(c => c.Id != cuenta.Id && c.NumeroCuenta == request.Request.NumeroCuenta, cancellationToken);
                if (dup) return Result<bool>.Failure("Conflict: El numero de cuenta ya existe");

                cuenta.NumeroCuenta = request.Request.NumeroCuenta.Trim();
                cuenta.TipoCuentaId = request.Request.TipoCuentaId;
                cuenta.ClienteId = request.Request.ClienteId;
                cuenta.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
                return Result<bool>.Success(true);
            }
        }

        internal class PatchHandler : IRequestHandler<CuentaPatchCommandRequest, Result<bool>>
        {
            private readonly BancaliteContext _context;
            /// <summary>
            /// Crea un handler para PATCH de cuenta.
            /// </summary>
            public PatchHandler(BancaliteContext context) { _context = context; }

            /// <summary>
            /// Aplica cambios parciales; valida unicidad si cambia número.
            /// </summary>
            /// <param name="request">Id y campos parciales.</param>
            /// <param name="cancellationToken">Token de cancelación.</param>
            /// <returns>True si se actualiza; error si no existe o hay conflicto.</returns>
            public async Task<Result<bool>> Handle(CuentaPatchCommandRequest request, CancellationToken cancellationToken)
            {
                var cuenta = await _context.Cuentas.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
                if (cuenta == null) return Result<bool>.Failure("No encontrado");

                if (!string.IsNullOrWhiteSpace(request.Request.NumeroCuenta))
                {
                    var numero = request.Request.NumeroCuenta.Trim();
                    var dup = await _context.Cuentas.AsNoTracking().AnyAsync(c => c.Id != cuenta.Id && c.NumeroCuenta == numero, cancellationToken);
                    if (dup) return Result<bool>.Failure("Conflict: El numero de cuenta ya existe");
                    cuenta.NumeroCuenta = numero;
                }
                if (request.Request.TipoCuentaId.HasValue) cuenta.TipoCuentaId = request.Request.TipoCuentaId.Value;
                if (request.Request.ClienteId.HasValue) cuenta.ClienteId = request.Request.ClienteId.Value;

                cuenta.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
                return Result<bool>.Success(true);
            }
        }
    }
}
