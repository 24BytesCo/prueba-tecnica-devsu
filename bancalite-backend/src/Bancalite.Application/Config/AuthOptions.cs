namespace Bancalite.Application.Config
{
    /// <summary>
    /// Opciones de autenticación de la aplicación.
    /// </summary>
    public class AuthOptions
    {
        /// <summary>
        /// Días de vida útil del Refresh Token.
        /// </summary>
        public int RefreshDays { get; set; } = 7;
    }
}

