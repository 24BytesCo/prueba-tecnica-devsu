using Bancalite.Application.Config;
using Bancalite.Application.Core;
using Bancalite.Application.Interface;
using Bancalite.Persitence;
using Bancalite.Persitence.Model;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Bancalite.Application.Auth.Login
{
    /// <summary>
    /// Comando de autenticación (login) de usuarios.
    /// </summary>
    public class LoginCommand
    {
        /// <summary>
        /// Mensaje de comando para realizar login.
        /// </summary>
        public record LoginCommandRequest(LoginRequest LoginRequest) : IRequest<Result<Profile>>;

        /// <summary>
        /// Manejador del comando de login.
        /// </summary>
        internal class LoginCommandHandler : IRequestHandler<LoginCommandRequest, Result<Profile>>
        {
            private readonly UserManager<AppUser> _userManager;
            private readonly ITokenService _tokenService;
            private readonly BancaliteContext _context;
            private readonly IOptions<AuthOptions> _authOptions;

            public LoginCommandHandler(UserManager<AppUser> userManager, ITokenService tokenService, BancaliteContext context, IOptions<AuthOptions> authOptions)
            {
                _userManager = userManager;
                _tokenService = tokenService;
                _context = context;
                _authOptions = authOptions;
            }

            /// <summary>
            /// Ejecuta el proceso de autenticación.
            /// </summary>
            /// <remarks>
            /// - Usa mensaje genérico para errores de credenciales (no enumerar usuarios).
            /// - Token opcional: puede agregarse inyectando un servicio de tokens.
            /// </remarks>
            public async Task<Result<Profile>> Handle(LoginCommandRequest request, CancellationToken cancellationToken)
            {
                var email = request.LoginRequest.Email!;
                var password = request.LoginRequest.Password!;

                // Buscar usuario por email
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken: cancellationToken);
                if (user is null)
                {
                    // Credenciales inválidas (mensaje genérico)
                    return Result<Profile>.Failure("Credenciales inválidas");
                }

                // Verificar contraseña
                var ok = await _userManager.CheckPasswordAsync(user, password);
                if (!ok)
                {
                    return Result<Profile>.Failure("Credenciales inválidas");
                }
                var cliente = await _context.Clientes
                .Include(c => c.Persona)
                .FirstOrDefaultAsync(c => c.AppUserId == user.Id, cancellationToken: cancellationToken);

                // Se genera el refresh token y se almacena en AspNetUserTokens
                var days = _authOptions.Value.RefreshDays > 0 ? _authOptions.Value.RefreshDays : 7;
                var refresh = GenerateRefreshTokenString(TimeSpan.FromDays(days));
                await _userManager.RemoveAuthenticationTokenAsync(user, "Bancalite", "RefreshToken");
                await _userManager.SetAuthenticationTokenAsync(user, "Bancalite", "RefreshToken", refresh);

                // Se mapea el perfil de retorno
                var profile = new Profile
                {
                    NombreCompleto = cliente != null ? $"{cliente.Persona.Nombres} {cliente.Persona.Apellidos}" : user.UserName,
                    Email = user.Email,
                    Token = await _tokenService.GenerarToken(user),
                    RefreshToken = refresh
                };

                return Result<Profile>.Success(profile);
            }
        }
        private static string GenerateRefreshTokenString(TimeSpan lifetime)
        {
            var expires = DateTime.UtcNow.Add(lifetime).Ticks;
            return $"{Guid.NewGuid():N}.{expires}";
        }
    }
}
