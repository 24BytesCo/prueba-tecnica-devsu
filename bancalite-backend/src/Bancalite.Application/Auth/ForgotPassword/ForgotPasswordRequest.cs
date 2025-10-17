namespace Bancalite.Application.Auth.ForgotPassword;

/// <summary>
/// Datos para iniciar el flujo de recuperación de contraseña.
/// </summary>
public class ForgotPasswordRequest
{
    /// <summary>
    /// Email del usuario.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// URL base del front para armar el link de reset.
    /// Ej: https://app/reset-password
    /// </summary>
    public string? RedirectBaseUrl { get; set; }

    /// <summary>
    /// Incluir datos de depuración (token/link) en la respuesta.
    /// </summary>
    public bool IncludeDebug { get; set; } = false;
}

