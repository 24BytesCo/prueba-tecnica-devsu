using Bancalite.Persitence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Bancalite.Infraestructure.Startup;
using Microsoft.AspNetCore.Identity;
using Bancalite.Persitence.Model;

namespace Bancalite.Infraestructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registra servicios de infraestructura: DbContext, seeding y utilidades.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        // Construir cadena para Postgres desde appsettings o variables de entorno
        var connectionString = config.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var host = config["DB_HOST"] ?? "localhost";
            var port = config["DB_PORT"] ?? "5432";
            var db   = config["DB_NAME"] ?? "bancalite";
            var user = config["DB_USER"] ?? "admin";
            var pass = config["DB_PASSWORD"] ?? string.Empty;
            connectionString = $"Host={host};Port={port};Database={db};Username={user};Password={pass};Pooling=true";
        }

        services.AddDbContext<BancaliteContext>(options => options.UseNpgsql(connectionString));

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
            .AddEntityFrameworkStores<BancaliteContext>();

        // Ejecuta migraciones y seed en Development al iniciar la app
        if (env.IsDevelopment())
        {
            services.AddHostedService<CatalogSeedHostedService>();
        }

        return services;
    }
}
