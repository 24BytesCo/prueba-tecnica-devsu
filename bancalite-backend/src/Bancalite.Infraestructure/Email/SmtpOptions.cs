namespace Bancalite.Infraestructure.Email
{
    /// <summary>
    /// Opciones de configuración para envío SMTP.
    /// </summary>
    public class SmtpOptions
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = "Bancalite";
    }
}

