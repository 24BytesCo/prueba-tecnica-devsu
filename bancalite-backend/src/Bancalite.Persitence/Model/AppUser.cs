using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Bancalite.Persitence.Model
{
    public class AppUser : IdentityUser<Guid>
    {
        public string? DisplayName { get; set; }
    }

}