using Bancalite.Persitence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;
using Bancalite.Persitence.Model;
using Bancalite.Domain;

namespace Bancalite.Infraestructure.Startup;

/// <summary>
/// Aplica migraciones y siembra catálogos al iniciar la aplicación (solo Development).
/// </summary>
public class CatalogSeedHostedService : IHostedService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<CatalogSeedHostedService> _logger;

    public CatalogSeedHostedService(IServiceProvider services, ILogger<CatalogSeedHostedService> logger)
    {
        _services = services;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BancaliteContext>();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            _logger.LogInformation("Aplicando migraciones de base de datos...");
            await db.Database.MigrateAsync(cancellationToken);

            _logger.LogInformation("Sembrando catálogos si están vacíos...");
            await db.SeedCatalogosAsync(cancellationToken);

            // Sembrar roles de Identity (Admin, User)
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            foreach (var roleName in new[] { "Admin", "User" })
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                    if (!result.Succeeded)
                    {
                        _logger.LogWarning("No se pudo crear el rol {Role}: {Errors}", roleName, string.Join(",", result.Errors));
                    }
                }
            }

            // Crear usuario Admin por defecto (solo Development)
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var adminUserName = config["ADMIN_USERNAME"] ?? "admin";
            var adminEmail = config["ADMIN_EMAIL"] ?? "admin@bancalite.local";
            var adminPassword = config["ADMIN_PASSWORD"] ?? "Admin123$"; // uso dev

            var admin = await userManager.FindByNameAsync(adminUserName) ?? await userManager.FindByEmailAsync(adminEmail);
            if (admin is null)
            {
                admin = new AppUser
                {
                    Id = Guid.NewGuid(),
                    UserName = adminUserName,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                var createResult = await userManager.CreateAsync(admin, adminPassword);
                if (!createResult.Succeeded)
                {
                    _logger.LogWarning("No se pudo crear el usuario admin: {Errors}", string.Join(",", createResult.Errors));
                }
                else
                {
                    await userManager.AddToRoleAsync(admin, "Admin");

                    // Vincular a un Cliente del dominio si no existe
                    var tieneCliente = await db.Clientes.AnyAsync(c => c.AppUserId == admin.Id, cancellationToken);
                    if (!tieneCliente)
                    {
                        // Obtener catálogos requeridos
                        var generoId = await db.Generos.Select(g => g.Id).FirstOrDefaultAsync(cancellationToken);
                        var tipoDocId = await db.TiposDocumentoIdentidad.Select(t => t.Id).FirstOrDefaultAsync(cancellationToken);

                        var persona = new Persona
                        {
                            Id = Guid.NewGuid(),
                            Nombres = "Admin",
                            Apellidos = "System",
                            Edad = 30,
                            GeneroId = generoId,
                            TipoDocumentoIdentidadId = tipoDocId,
                            NumeroDocumento = "ADM-0001",
                            Email = adminEmail
                        };
                        db.Personas.Add(persona);

                        var cliente = new Cliente
                        {
                            Id = Guid.NewGuid(),
                            PersonaId = persona.Id,
                            AppUserId = admin.Id,
                            PasswordHash = admin.PasswordHash,
                            Estado = true
                        };
                        db.Clientes.Add(cliente);
                        await db.SaveChangesAsync(cancellationToken);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante migración/siembra de catálogos");
            // Re-lanzar no es deseable en dev; se continúa para permitir el arranque.
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
