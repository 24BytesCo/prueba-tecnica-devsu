using FluentValidation;

namespace Bancalite.Application.Auth.ResetPassword;

/// <summary>
/// Valida la solicitud de reseteo de contraseña.
/// </summary>
public class ResetPasswordValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es obligatorio")
            .EmailAddress().WithMessage("El email no es válido");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("El token es obligatorio");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("La nueva contraseña es obligatoria")
            .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres");
    }
}

