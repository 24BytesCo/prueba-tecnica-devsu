namespace Bancalite.Application.Auth.Refresh;

/// <summary>
/// Solicitud para renovar el access token usando un refresh token.
/// </summary>
public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

