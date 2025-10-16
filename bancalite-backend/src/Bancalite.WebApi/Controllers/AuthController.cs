using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bancalite.Application.Auth.Login;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static Bancalite.Application.Auth.Login.LoginCommand;
using Microsoft.Extensions.Hosting;
using Bancalite.Application.Auth.ForgotPassword;
using Bancalite.Application.Auth.ResetPassword;
using Bancalite.Application.Auth.Refresh;
using Bancalite.Application.Auth.Logout;
using Bancalite.Application.Auth.Me;
using Microsoft.AspNetCore.Authorization;

namespace Bancalite.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ISender _iSender;
        private readonly IHostEnvironment _env;

        public AuthController(ISender iSender, IHostEnvironment env)
        {
            _iSender = iSender;
            _env = env;
        }

        //login
        [HttpPost("login")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var command = new LoginCommandRequest(request);

            var response = await _iSender.Send(command, cancellationToken);

            return response.IsSuccess ? Ok(response.Value)
                : Unauthorized();
        }

        /// <summary>
        /// Inicia el flujo de recuperación de contraseña enviando un link con token al correo.
        /// </summary>
        /// <param name="email">Email del usuario.</param>
        /// <param name="redirectBaseUrl">URL base del front para armar el link (ej: https://app/reset-password).</param>
        [HttpPost("forgot-password")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> ForgotPassword([FromQuery] string email, [FromQuery] string redirectBaseUrl)
        {
            var req = new ForgotPasswordRequest
            {
                Email = email,
                RedirectBaseUrl = redirectBaseUrl,
                IncludeDebug = _env.IsDevelopment()
            };
            var result = await _iSender.Send(new ForgotPasswordCommand.ForgotPasswordCommandRequest(req));
            if (!result.IsSuccess)
            {
                if (string.Equals(result.Error, "Unauthorized", StringComparison.OrdinalIgnoreCase))
                    return Unauthorized();
                return BadRequest(result.Error);
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// Completa el flujo de recuperación, aplicando la nueva contraseña con el token.
        /// </summary>
        [HttpPost("reset-password")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ResetPassword([FromQuery] string email, [FromQuery] string token, [FromQuery] string newPassword)
        {
            var req = new ResetPasswordRequest { Email = email, Token = token, NewPassword = newPassword };
            var result = await _iSender.Send(new ResetPasswordCommand.ResetPasswordCommandRequest(req));
            if (!result.IsSuccess)
            {
                if (string.Equals(result.Error, "Unauthorized", StringComparison.OrdinalIgnoreCase))
                    return Unauthorized();
                return BadRequest(result.Error);
            }
            return Ok(new { reset = true });
        }

        /// <summary>
        /// Renueva el access token rotando el refresh token.
        /// </summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var result = await _iSender.Send(new RefreshTokenCommand.RefreshTokenCommandRequest(request), cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : Unauthorized();
        }

        /// <summary>
        /// Revoca el refresh token (logout).
        /// </summary>
        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var result = await _iSender.Send(new LogoutCommand.LogoutCommandRequest(request.RefreshToken), cancellationToken);
            return result.IsSuccess ? Ok(new { revoked = true }) : Unauthorized();
        }

        /// <summary>
        /// Perfil del usuario autenticado actual.
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me(CancellationToken cancellationToken)
        {
            var result = await _iSender.Send(new MeQuery.MeQueryRequest(), cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : Unauthorized();
        }
    }
}
