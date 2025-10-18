using Bancalite.Persitence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Bancalite.Application.Cuentas.CuentaUpdate
{
    /// <summary>
    /// Validador del comando PATCH de cuenta.
    /// </summary>
    public class CuentaPatchCommandValidator : AbstractValidator<CuentaUpdateCommand.CuentaPatchCommandRequest>
    {
        private readonly BancaliteContext _context;
        public CuentaPatchCommandValidator(BancaliteContext context)
        {
            _context = context;

            RuleFor(x => x.Request.NumeroCuenta)
                .MaximumLength(30)
                .When(x => x.Request.NumeroCuenta != null);

            RuleFor(x => x.Request.TipoCuentaId)
                .MustAsync(async (id, ct) => id.HasValue && await _context.TiposCuenta.AsNoTracking().AnyAsync(t => t.Id == id.Value, ct))
                .When(x => x.Request.TipoCuentaId.HasValue)
                .WithMessage("TipoCuenta no existe");

            RuleFor(x => x.Request.ClienteId)
                .MustAsync(async (id, ct) => id.HasValue && await _context.Clientes.AsNoTracking().AnyAsync(t => t.Id == id.Value, ct))
                .When(x => x.Request.ClienteId.HasValue)
                .WithMessage("Cliente no existe");
        }
    }
}

