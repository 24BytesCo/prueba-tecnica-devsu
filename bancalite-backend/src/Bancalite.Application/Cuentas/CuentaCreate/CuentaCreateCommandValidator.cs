using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Bancalite.Persitence;

namespace Bancalite.Application.Cuentas.CuentaCreate
{
    /// <summary>
    /// Validador del comando de apertura de cuenta (patrón unificado: solo command).
    /// </summary>
    public class CuentaCreateCommandValidator : AbstractValidator<CuentaCreateCommand.CuentaCreateCommandRequest>
    {
        private readonly BancaliteContext _context;

        /// <summary>
        /// Crea un validador para el comando de creación de cuenta.
        /// </summary>
        public CuentaCreateCommandValidator(BancaliteContext context)
        {
            _context = context;

            // numeroCuenta opcional: si viene, longitud y unicidad
            RuleFor(x => x.Request.NumeroCuenta)
                .MaximumLength(30)
                .When(x => !string.IsNullOrWhiteSpace(x.Request.NumeroCuenta));

            RuleFor(x => x.Request)
                .MustAsync(async (req, ct) =>
                {
                    if (string.IsNullOrWhiteSpace(req.NumeroCuenta)) return true; // se generará
                    return !await _context.Cuentas.AsNoTracking()
                        .AnyAsync(c => c.NumeroCuenta == req.NumeroCuenta, ct);
                })
                .WithMessage("Conflict: El numero de cuenta ya existe");

            // saldoInicial >= 0
            RuleFor(x => x.Request.SaldoInicial)
                .GreaterThanOrEqualTo(0).WithMessage("SaldoInicial debe ser mayor o igual a 0");

            // TipoCuenta debe existir
            RuleFor(x => x.Request.TipoCuentaId)
                .NotEmpty()
                .MustAsync(async (id, ct) => await _context.TiposCuenta.AsNoTracking().AnyAsync(t => t.Id == id, ct))
                .WithMessage("TipoCuenta no existe");

            // Cliente debe existir
            RuleFor(x => x.Request.ClienteId)
                .NotEmpty()
                .MustAsync(async (id, ct) => await _context.Clientes.AsNoTracking().AnyAsync(t => t.Id == id, ct))
                .WithMessage("Cliente no existe");
        }
    }
}
