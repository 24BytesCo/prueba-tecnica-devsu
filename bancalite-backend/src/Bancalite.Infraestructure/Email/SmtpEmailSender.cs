using Bancalite.Application.Interface;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Bancalite.Infraestructure.Email
{
    /// <summary>
    /// Implementación SMTP simple de IEmailSender.
    /// </summary>
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IOptions<SmtpOptions> _options;

        public SmtpEmailSender(IOptions<SmtpOptions> options)
        {
            _options = options;
        }

        public async Task SendAsync(string to, string subject, string htmlBody, string? textBody = null, CancellationToken ct = default)
        {
            var opts = _options.Value;
            using var client = new SmtpClient(opts.Host, opts.Port)
            {
                EnableSsl = opts.EnableSsl,
                Credentials = new NetworkCredential(opts.Username, opts.Password)
            };

            using var message = new MailMessage()
            {
                From = new MailAddress(opts.SenderEmail, opts.SenderName),
                Subject = subject,
                Body = string.IsNullOrWhiteSpace(textBody) ? htmlBody : textBody,
                IsBodyHtml = string.IsNullOrWhiteSpace(textBody)
            };
            message.To.Add(to);

            await client.SendMailAsync(message, ct);
        }
    }
}

