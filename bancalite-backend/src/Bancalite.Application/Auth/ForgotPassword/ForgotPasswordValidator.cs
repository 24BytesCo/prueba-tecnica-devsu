using FluentValidation;

namespace Bancalite.Application.Auth.ForgotPassword;

/// <summary>
/// Valida la solicitud de recuperación de contraseña.
/// </summary>
public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es obligatorio")
            .EmailAddress().WithMessage("El email no es válido");

        RuleFor(x => x.RedirectBaseUrl)
            .Must(url => string.IsNullOrWhiteSpace(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("redirectBaseUrl debe ser una URL absoluta");
    }
}

