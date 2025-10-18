using Bancalite.Application.Core;
using Bancalite.Application.Interface;
using Bancalite.Persitence;
using Bancalite.Persitence.Model;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
        private readonly BancaliteContext _context;

        public Handler(IUserAccessor userAccessor, UserManager<AppUser> userManager, BancaliteContext context)
        {
            _userAccessor = userAccessor;
            _userManager = userManager;
            _context = context;
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

            // Resolver estado del cliente (si existe vínculo)
            bool? clienteActivo = null;
            try
            {
                clienteActivo = await _context.Clientes.AsNoTracking()
                    .Where(c => c.AppUserId == user.Id)
                    .Select(c => (bool?)c.Estado)
                    .FirstOrDefaultAsync(cancellationToken);
            }
            catch { /* ignore */ }

            var profile = new Profile
            {
                NombreCompleto = user.DisplayName ?? user.UserName,
                CodeRol = roles.First() ?? null,
                Email = user.Email,
                Token = null,
                RefreshToken = null,
                ClienteActivo = clienteActivo
            };
            return Result<Profile>.Success(profile);
        }
    }
}
