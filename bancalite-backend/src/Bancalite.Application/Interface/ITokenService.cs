using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bancalite.Persitence.Model;

namespace Bancalite.Application.Interface
{
    public interface ITokenService
    {
        Task<string> GenerarToken(AppUser user);

    }
}