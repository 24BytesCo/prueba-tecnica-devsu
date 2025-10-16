using System;

namespace Bancalite.Infraestructure.Security
{
    /// <summary>
    /// Opciones de configuración para emisión de tokens JWT.
    /// Se vincula con la sección de configuración "JWT" (o "Jwt").
    /// </summary>
    public class JwtOptions
    {
        /// <summary>
        /// Clave secreta simétrica para firmar el token (HMAC-SHA256).
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// Issuer del token (emisor).
        /// </summary>
        public string? Issuer { get; set; }

        /// <summary>
        /// Audience esperada del token (consumidor).
        /// </summary>
        public string? Audience { get; set; }

        /// <summary>
        /// Minutos de expiración del token.
        /// </summary>
        public int ExpiresMinutes { get; set; } = 60;
    }
}

