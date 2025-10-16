using Bancalite.Application.Core;
using Bancalite.Persitence;
using Bancalite.Persitence.Model;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bancalite.Application.Auth.Logout;

/// <summary>
/// Revoca el refresh token (logout) eliminándolo de AspNetUserTokens.
/// </summary>
public class LogoutCommand
{
    public record LogoutCommandRequest(string RefreshToken) : IRequest<Result<bool>>;

    internal class Handler : IRequestHandler<LogoutCommandRequest, Result<bool>>
    {
        private readonly BancaliteContext _context;
        private readonly UserManager<AppUser> _userManager;

        public Handler(BancaliteContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<Result<bool>> Handle(LogoutCommandRequest request, CancellationToken ct)
        {
            var provided = (request.RefreshToken ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(provided)) return Result<bool>.Failure("Unauthorized");

            // Se busca el refresh token emitido
            var userToken = await _context.Set<IdentityUserToken<Guid>>()
                .FirstOrDefaultAsync(t => t.LoginProvider == "Bancalite" && t.Name == "RefreshToken" && t.Value == provided, ct);
            if (userToken == null) return Result<bool>.Failure("Unauthorized");

            var user = await _userManager.FindByIdAsync(userToken.UserId.ToString());
            if (user is null) return Result<bool>.Failure("Unauthorized");

            // Se elimina el refresh token para revocar la sesión
            await _userManager.RemoveAuthenticationTokenAsync(user, "Bancalite", "RefreshToken");
            // Se rota el SecurityStamp para invalidar todos los access tokens emitidos
            await _userManager.UpdateSecurityStampAsync(user);
            return Result<bool>.Success(true);
        }
    }
}
