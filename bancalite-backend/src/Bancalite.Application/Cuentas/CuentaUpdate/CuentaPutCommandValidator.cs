using Bancalite.Persitence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Bancalite.Application.Cuentas.CuentaUpdate
{
    /// <summary>
    /// Validador del comando PUT de cuenta.
    /// </summary>
    public class CuentaPutCommandValidator : AbstractValidator<CuentaUpdateCommand.CuentaPutCommandRequest>
    {
        private readonly BancaliteContext _context;
        public CuentaPutCommandValidator(BancaliteContext context)
        {
            _context = context;
            RuleFor(x => x.Request.NumeroCuenta).NotEmpty().MaximumLength(30);
            RuleFor(x => x.Request.TipoCuentaId)
                .NotEmpty()
                .MustAsync(async (id, ct) => await _context.TiposCuenta.AsNoTracking().AnyAsync(t => t.Id == id, ct))
                .WithMessage("TipoCuenta no existe");
            RuleFor(x => x.Request.ClienteId)
                .NotEmpty()
                .MustAsync(async (id, ct) => await _context.Clientes.AsNoTracking().AnyAsync(t => t.Id == id, ct))
                .WithMessage("Cliente no existe");
        }
    }
}

