using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bancalite.Application.Auth
{
    public class Profile
    {
        public string? NombreCompleto { get; set; }
        public string? Email { get; set; }
        public string? Token { get; set; }
    }
}