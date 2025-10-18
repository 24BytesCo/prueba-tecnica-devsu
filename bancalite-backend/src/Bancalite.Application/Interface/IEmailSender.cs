namespace Bancalite.Application.Interface
{
    /// <summary>
    /// Servicio para envío de correos electrónicos.
    /// </summary>
    public interface IEmailSender
    {
        /// <summary>
        /// Envía un correo electrónico.
        /// </summary>
        /// <param name="to">Correo destino.</param>
        /// <param name="subject">Asunto.</param>
        /// <param name="htmlBody">Cuerpo en HTML.</param>
        /// <param name="textBody">Cuerpo en texto plano (opcional).</param>
        /// <param name="ct">Token de cancelación.</param>
        Task SendAsync(string to, string subject, string htmlBody, string? textBody = null, CancellationToken ct = default);
    }
}

