namespace Bancalite.Application.Auth
{
    public class Profile
    {
        public string? NombreCompleto { get; set; }
        public string? Email { get; set; }
        public string? CodeRol { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        /// <summary>
        /// Estado del cliente vinculado al usuario (true=activo, false=inactivo). Null si no aplica.
        /// </summary>
        public bool? ClienteActivo { get; set; }
    }
}
