using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Bancalite.Persitence
{
    public static class DependencyInjection
    {
        /// <summary>
        /// Registra servicios de persistencia (DbContext y utilidades relacionadas).
        /// </summary>
        /// <remarks>
        /// Configura <see cref="BancaliteContext"/> con cadena de conexión proveniente de
        /// ConnectionStrings:Default o variables de entorno (DB_HOST, DB_PORT, etc.).
        /// Habilita logs detallados en entorno de desarrollo.
        /// </remarks>
        public static IServiceCollection AddPersistence(this IServiceCollection services)
        {
            // Registrar DbContext usando Npgsql y configuración desde IConfiguration
            services.AddDbContext<BancaliteContext>((sp, options) =>
            {
                var config = sp.GetRequiredService<IConfiguration>();

                // Obtener cadena de conexión
                var conn = config.GetConnectionString("Default");
                if (string.IsNullOrWhiteSpace(conn))
                {
                    var host = config["DB_HOST"] ?? "localhost";
                    var port = config["DB_PORT"] ?? "5432";
                    var db   = config["DB_NAME"] ?? "bancalite";
                    var user = config["DB_USER"] ?? "admin";
                    var pass = config["DB_PASSWORD"] ?? string.Empty;
                    conn = $"Host={host};Port={port};Database={db};Username={user};Password={pass};Pooling=true";
                }

                if (string.Equals(conn, "UseInMemoryDb", StringComparison.OrdinalIgnoreCase))
                {
                    options.UseInMemoryDatabase("Bancalite_InMemory");
                }
                else
                {
                    options.UseNpgsql(conn);
                }

                // Diagnóstico útil en desarrollo
                var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                              ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                              ?? string.Empty;
                if (string.Equals(envName, "Development", StringComparison.OrdinalIgnoreCase))
                {
                    options.EnableDetailedErrors();
                    options.EnableSensitiveDataLogging();
                }
            });

            return services;
        }
    }
}

