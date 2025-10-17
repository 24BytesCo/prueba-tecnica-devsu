using FluentValidation;

namespace Bancalite.Application.Cuentas.CuentaEstado
{
    /// <summary>
    /// Validador del comando PATCH de estado de cuenta.
    /// </summary>
    public class CuentaEstadoPatchCommandValidator : AbstractValidator<CuentaEstadoPatchCommand.CuentaEstadoPatchCommandRequest>
    {
        public CuentaEstadoPatchCommandValidator()
        {
            RuleFor(x => x.Request.Estado)
                .NotEmpty()
                .Must(v => v.Equals("Activa", StringComparison.OrdinalIgnoreCase)
                         || v.Equals("Inactiva", StringComparison.OrdinalIgnoreCase)
                         || v.Equals("Bloqueada", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Estado inválido");

            RuleFor(x => x.Request.Motivo)
                .MaximumLength(200)
                .When(x => !string.IsNullOrWhiteSpace(x.Request.Motivo));
        }
    }
}

