using System.Text;
using Bancalite.Infraestructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Bancalite.WebApi.Extensions
{
    public static class IdentityServiceExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
        {
            // Configuración de autenticación JWT basada en appsettings (JWT/Jwt)
            var section = config.GetSection("JWT");
            if (!section.Exists()) section = config.GetSection("Jwt");

            var opts = section.Get<JwtOptions>() ?? new JwtOptions();
            var key = opts.Key ?? string.Empty;

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(o =>
                {
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = opts.Issuer,
                        ValidateAudience = true,
                        ValidAudience = opts.Audience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(2)
                    };
                });

            services.AddAuthorization();

            return services;
        }
    }
}
