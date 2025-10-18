using Microsoft.AspNetCore.Identity;

namespace Bancalite.Persitence.Model
{
    public class AppUser : IdentityUser<Guid>
    {
        public string? DisplayName { get; set; }
    }

}