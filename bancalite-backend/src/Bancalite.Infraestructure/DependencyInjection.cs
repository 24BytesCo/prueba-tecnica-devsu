using Bancalite.Persitence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Bancalite.Infraestructure.Startup;
using Microsoft.AspNetCore.Identity;
using Bancalite.Persitence.Model;
using Bancalite.Application.Interface;
using Bancalite.Infraestructure.Security;
using Microsoft.Extensions.Options;
using Bancalite.Infraestructure.Email;
using Bancalite.Application.Config;

namespace Bancalite.Infraestructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registra servicios de infraestructura: persistencia (DbContext), Identity y seeding.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        // Registrar persistencia (DbContext y opciones) centralizada en capa Persistence
        services.AddPersistence();

        // Identity Core (usuarios y roles) usando el mismo DbContext
        services.AddIdentityCore<AppUser>(options =>
            {
                options.User.RequireUniqueEmail = false;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<BancaliteContext>()
            .AddDefaultTokenProviders();

        // Opciones tipadas para JWT
        services.AddOptions<JwtOptions>()
            .Bind(config.GetSection("JWT").Exists() ? config.GetSection("JWT") : config.GetSection("Jwt"))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Key), "JWT:Key es requerido")
            .Validate(o => !string.IsNullOrWhiteSpace(o.Issuer), "JWT:Issuer es requerido")
            .Validate(o => !string.IsNullOrWhiteSpace(o.Audience), "JWT:Audience es requerido")
            .Validate(o => o.ExpiresMinutes > 0, "JWT:ExpiresMinutes debe ser > 0");

        // Opciones de Auth (refresh)
        services.AddOptions<AuthOptions>()
            .Bind(config.GetSection("Auth"))
            .Validate(o => o.RefreshDays > 0, "Auth:RefreshDays debe ser > 0");

        // Servicios de seguridad / tokens
        services.AddScoped<ITokenService, TokenService>();
        services.AddHttpContextAccessor();
        services.AddScoped<IUserAccessor, UserAccessor>();

        // Email (SMTP)
        services.AddOptions<SmtpOptions>()
            .Bind(config.GetSection("Smtp"))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Host), "Smtp:Host es requerido")
            .Validate(o => !string.IsNullOrWhiteSpace(o.SenderEmail), "Smtp:SenderEmail es requerido");
        services.AddScoped<IEmailSender, SmtpEmailSender>();

        // Ejecuta migraciones y seed en Development al iniciar la app
        if (env.IsDevelopment())
        {
            services.AddHostedService<CatalogSeedHostedService>();
        }

        return services;
    }
}
