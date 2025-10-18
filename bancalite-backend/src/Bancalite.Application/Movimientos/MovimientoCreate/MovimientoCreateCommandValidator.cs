using Bancalite.Persitence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Bancalite.Application.Movimientos.MovimientoCreate
{
    /// <summary>
    /// Validador del comando de creación de movimiento.
    /// </summary>
    public class MovimientoCreateCommandValidator : AbstractValidator<MovimientoCreateCommand.MovimientoCreateCommandRequest>
    {
        private readonly BancaliteContext _context;

        public MovimientoCreateCommandValidator(BancaliteContext context)
        {
            _context = context;

            RuleFor(x => x.Request.NumeroCuenta)
                .NotEmpty().WithMessage("NumeroCuenta es requerido")
                .MaximumLength(30);

            RuleFor(x => x.Request.TipoCodigo)
                .NotEmpty().WithMessage("TipoCodigo es requerido")
                .Must(tc => tc == "CRE" || tc == "DEB").WithMessage("TipoCodigo debe ser CRE o DEB");

            // El monto se valida en el handler para devolver 422 (regla de dominio)

            RuleFor(x => x.Request.IdempotencyKey)
                .MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.Request.IdempotencyKey));

            // La cuenta debe existir
            RuleFor(x => x.Request)
                .MustAsync(async (req, ct) =>
                {
                    var numero = req.NumeroCuenta.Trim();
                    return await _context.Cuentas.AsNoTracking().AnyAsync(c => c.NumeroCuenta == numero, ct);
                })
                .WithMessage("Cuenta no encontrada");
        }
    }
}
