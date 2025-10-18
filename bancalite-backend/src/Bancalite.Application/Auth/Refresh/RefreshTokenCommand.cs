using Bancalite.Application.Config;
using Bancalite.Application.Core;
using Bancalite.Application.Interface;
using Bancalite.Persitence;
using Bancalite.Persitence.Model;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Bancalite.Application.Auth.Refresh;

/// <summary>
/// Renueva el access token rotando el refresh token almacenado en AspNetUserTokens.
/// </summary>
public class RefreshTokenCommand
{
    public record RefreshTokenCommandRequest(RefreshTokenRequest Request) : IRequest<Result<Profile>>;

    internal class Handler : IRequestHandler<RefreshTokenCommandRequest, Result<Profile>>
    {
        private readonly BancaliteContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IOptions<AuthOptions> _authOptions;

        public Handler(BancaliteContext context, UserManager<AppUser> userManager, ITokenService tokenService, IOptions<AuthOptions> authOptions)
        {
            _context = context;
            _userManager = userManager;
            _tokenService = tokenService;
            _authOptions = authOptions;
        }

        public async Task<Result<Profile>> Handle(RefreshTokenCommandRequest message, CancellationToken ct)
        {
            var provided = (message.Request.RefreshToken ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(provided)) return Result<Profile>.Failure("Unauthorized");

            // Se busca el token en AspNetUserTokens
            var userToken = await _context.Set<IdentityUserToken<Guid>>()
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.LoginProvider == "Bancalite" && t.Name == "RefreshToken" && t.Value == provided, ct);
            if (userToken == null) return Result<Profile>.Failure("Unauthorized");

            // Se valida la expiración (token formateado como "<guid>.<ticks>")
            var parts = provided.Split('.', 2);
            if (parts.Length != 2 || !long.TryParse(parts[1], out var ticks))
                return Result<Profile>.Failure("Unauthorized");
            var expires = new DateTime(ticks, DateTimeKind.Utc);
            if (expires < DateTime.UtcNow) return Result<Profile>.Failure("Unauthorized");

            var user = await _userManager.FindByIdAsync(userToken.UserId.ToString());
            if (user is null) return Result<Profile>.Failure("Unauthorized");

            // Se rota el refresh token
            var days = _authOptions.Value.RefreshDays > 0 ? _authOptions.Value.RefreshDays : 7;
            var newRefresh = GenerateRefreshTokenString(TimeSpan.FromDays(days));
            await _userManager.RemoveAuthenticationTokenAsync(user, "Bancalite", "RefreshToken");
            await _userManager.SetAuthenticationTokenAsync(user, "Bancalite", "RefreshToken", newRefresh);

            // Se genera un nuevo access token (JWT)
            var jwt = await _tokenService.GenerarToken(user);

            var profile = new Profile
            {
                NombreCompleto = user.DisplayName ?? user.UserName,
                Email = user.Email,
                Token = jwt,
                RefreshToken = newRefresh
            };
            return Result<Profile>.Success(profile);
        }

        private static string GenerateRefreshTokenString(TimeSpan lifetime)
        {
            var expires = DateTime.UtcNow.Add(lifetime).Ticks;
            return $"{Guid.NewGuid():N}.{expires}";
        }
    }
}
