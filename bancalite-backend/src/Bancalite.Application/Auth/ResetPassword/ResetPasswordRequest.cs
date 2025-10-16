namespace Bancalite.Application.Auth.ResetPassword;

/// <summary>
/// Datos para completar el reseteo de contraseña.
/// </summary>
public class ResetPasswordRequest
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

