using System.Net;
using Bancalite.Application.Core;
using Bancalite.Application.Interface;
using Bancalite.Persitence.Model;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;

namespace Bancalite.Application.Auth.ForgotPassword;

/// <summary>
/// Comando para iniciar el flujo de recuperación de contraseña.
/// </summary>
public class ForgotPasswordCommand
{
    /// <summary>
    /// Resultado del inicio de recuperación.
    /// </summary>
    public class ForgotPasswordResponse
    {
        public bool Sent { get; set; }
        public string? Link { get; set; }
        public string? Token { get; set; }
    }

    /// <summary>
    /// Mensaje CQRS para forgot password.
    /// </summary>
    public record ForgotPasswordCommandRequest(ForgotPasswordRequest Request) : IRequest<Result<ForgotPasswordResponse>>;

    internal class Handler : IRequestHandler<ForgotPasswordCommandRequest, Result<ForgotPasswordResponse>>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IValidator<ForgotPasswordRequest> _validator;
        private readonly IHostEnvironment _env;

        public Handler(UserManager<AppUser> userManager, IEmailSender emailSender, IValidator<ForgotPasswordRequest> validator, IHostEnvironment env)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _validator = validator;
            _env = env;
        }

        public async Task<Result<ForgotPasswordResponse>> Handle(ForgotPasswordCommandRequest message, CancellationToken ct)
        {
            // Validación
            await _validator.ValidateAndThrowAsync(message.Request, ct);

            // Verificar existencia del usuario
            var user = await _userManager.FindByEmailAsync(message.Request.Email);
            if (user is null)
            {
                return Result<ForgotPasswordResponse>.Failure("Unauthorized");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            string? link = null;
            if (!string.IsNullOrWhiteSpace(message.Request.RedirectBaseUrl))
            {
                var baseUrl = message.Request.RedirectBaseUrl!.TrimEnd('/');
                var email = System.Net.WebUtility.UrlEncode(message.Request.Email);
                var tokenEnc = System.Net.WebUtility.UrlEncode(token);
                link = $"{baseUrl}?email={email}&token={tokenEnc}";
            }

            var subject = "Recuperación de contraseña";
            var html = !string.IsNullOrWhiteSpace(link)
                ? $"<p>Para restablecer su contraseña haga clic en el siguiente enlace:</p><p><a href=\"{link}\">Restablecer contraseña</a></p>"
                : $"<p>Use el siguiente token para restablecer su contraseña:</p><pre>{System.Net.WebUtility.HtmlEncode(token)}</pre>";

            await _emailSender.SendAsync(message.Request.Email, subject, html, ct: ct);

            var includeDebug = _env.IsDevelopment() || message.Request.IncludeDebug;
            var response = new ForgotPasswordResponse
            {
                Sent = true,
                Link = includeDebug ? link : null,
                Token = includeDebug ? token : null
            };

            return Result<ForgotPasswordResponse>.Success(response);
        }
    }
}
