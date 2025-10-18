using FluentValidation;

namespace Bancalite.Application.Auth.Login
{
    /// <summary>
    /// Validador para la operación de Login.
    /// Solo valida formato y requeridos.
    /// La existencia del usuario y la contraseña se validan en el handler.
    /// </summary>
    public class LoginValidator : AbstractValidator<LoginRequest>
    {
        /// <summary>
        /// Crea una nueva instancia del validador de login.
        /// </summary>
        public LoginValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es obligatorio.")
                .EmailAddress().WithMessage("El email no es válido.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es obligatoria.")
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.");
        }
    }
}
