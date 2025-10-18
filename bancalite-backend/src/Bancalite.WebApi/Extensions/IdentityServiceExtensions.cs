using Bancalite.Infraestructure.Security;
using Bancalite.Persitence.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

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

            // Fallback seguro para evitar key vacía (tests/entornos sin configuración completa)
            if (string.IsNullOrWhiteSpace(key))
            {
                key = "dev-fallback-key-0123456789abcdef0123456789abcdef"; // 64 chars
                opts.Issuer ??= "Bancalite.WebApi";
                opts.Audience ??= "Bancalite.Client";
            }

            // Configuración de autenticación JWT
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

                    // Validación del SecurityStamp para revocación inmediata (logout)
                    o.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async context =>
                        {
                            var principal = context.Principal;
                            var userId = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                            var sst = principal?.FindFirst("sst")?.Value;
                            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(sst))
                            {
                                context.Fail("Unauthorized");
                                return;
                            }

                            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<AppUser>>();
                            var user = await userManager.FindByIdAsync(userId);
                            if (user == null || !string.Equals(user.SecurityStamp, sst, StringComparison.Ordinal))
                            {
                                context.Fail("Token revoked");
                            }
                        }
                    };
                });

            services.AddAuthorization();

            return services;
        }
    }
}


