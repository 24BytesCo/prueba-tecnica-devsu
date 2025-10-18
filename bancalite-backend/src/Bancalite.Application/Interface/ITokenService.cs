using Bancalite.Persitence.Model;

namespace Bancalite.Application.Interface
{
    public interface ITokenService
    {
        Task<string> GenerarToken(AppUser user);

    }
}