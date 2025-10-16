using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Bancalite.Application.Interface;
using Bancalite.Persitence;
using Bancalite.Persitence.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Bancalite.Infraestructure.Security
{
    public class TokenService : ITokenService
    {
        private readonly BancaliteContext _context;
        private readonly IOptions<JwtOptions> _jwtOptions;

        /// <summary>
        /// Servicio para generación de tokens JWT.
        /// </summary>
        public TokenService(BancaliteContext context, IOptions<JwtOptions> jwtOptions)
        {
            _context = context;
            _jwtOptions = jwtOptions;
        }

        /// <summary>
        /// Genera un token JWT para el usuario indicado con claims básicos y de rol.
        /// </summary>
        public Task<string> GenerarToken(AppUser user)
        {
            // Opciones tipadas (preferidas)
            var opts = _jwtOptions.Value;

            if (string.IsNullOrWhiteSpace(opts.Key))
                throw new InvalidOperationException("JWT:Key no está configurada");

            if (string.IsNullOrWhiteSpace(opts.Issuer))
                throw new InvalidOperationException("JWT:Issuer no está configurado");

            if (string.IsNullOrWhiteSpace(opts.Audience))
                throw new InvalidOperationException("JWT:Audience no está configurado");

            var expiresMinutes = opts.ExpiresMinutes > 0 ? opts.ExpiresMinutes : 60;

            //Buscando el cliente asociado al usuario
            var cliente = _context.Clientes
            .Include(c => c.Persona)
            .FirstOrDefault(c => c.AppUserId == user.Id);

            var nombreCompleto = cliente != null ?
            $"{cliente.Persona.Nombres} {cliente.Persona.Apellidos}" :
            user.UserName ?? "" ?? "";



            // Claims
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, nombreCompleto),
                new(ClaimTypes.Email, user.Email ?? cliente?.Persona.Email ?? "")
            };
            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email!));
            }

            // Roles desde tablas de Identity
            var roles = from ur in _context.UserRoles
                        join r in _context.Roles on ur.RoleId equals r.Id
                        where ur.UserId == user.Id
                        select r.Name!;
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Credenciales de firma
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opts.Key));
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            // Creacion del token
            var token = new JwtSecurityToken(
                issuer: opts.Issuer,
                audience: opts.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
                signingCredentials: creds
            );

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.WriteToken(token);
            return Task.FromResult(jwt);
        }
    }
}
