using Bancalite.Application.Core;
using Bancalite.Application.Interface;
using Bancalite.Persitence.Model;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Bancalite.Application.Auth.Me;

/// <summary>
/// Obtiene el perfil del usuario autenticado actual.
/// </summary>
public class MeQuery
{
    public record MeQueryRequest() : IRequest<Result<Profile>>;

    internal class Handler : IRequestHandler<MeQueryRequest, Result<Profile>>
    {
        private readonly IUserAccessor _userAccessor;
        private readonly UserManager<AppUser> _userManager;

        public Handler(IUserAccessor userAccessor, UserManager<AppUser> userManager)
        {
            _userAccessor = userAccessor;
            _userManager = userManager;
        }

        public async Task<Result<Profile>> Handle(MeQueryRequest request, CancellationToken cancellationToken)
        {
            var idOrEmailOrName = _userAccessor.GetUsername();
            if (string.IsNullOrWhiteSpace(idOrEmailOrName))
                return Result<Profile>.Failure("Unauthorized");

            AppUser? user = null;
            if (Guid.TryParse(idOrEmailOrName, out var idGuid))
            {
                user = await _userManager.FindByIdAsync(idGuid.ToString());
            }
            user ??= await _userManager.FindByEmailAsync(idOrEmailOrName)
                ?? await _userManager.FindByNameAsync(idOrEmailOrName);
            if (user is null) return Result<Profile>.Failure("Unauthorized");

            //Buscar rol
            var roles = await _userManager.GetRolesAsync(user);

            var profile = new Profile
            {
                NombreCompleto = user.DisplayName ?? user.UserName,
                CodeRol = roles.First() ?? null,
                Email = user.Email,
                Token = null,
                RefreshToken = null
            };
            return Result<Profile>.Success(profile);
        }
    }
}
