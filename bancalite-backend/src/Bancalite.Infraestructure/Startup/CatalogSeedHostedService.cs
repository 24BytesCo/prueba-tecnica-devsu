using Bancalite.Persitence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data.Common;
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
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BancaliteContext>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        // 0) Si el proveedor no es relacional (InMemory en pruebas), omitir chequeos/migraciones
        var esRelacional = db.Database.IsRelational();
        if (esRelacional)
        {
            bool puedeConectar = false;
            try
            {
                puedeConectar = await db.Database.CanConnectAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo conectar a la base de datos. Se omiten migraciones y seed.");
            }
            if (!puedeConectar)
            {
                _logger.LogWarning("Base de datos no disponible; se omiten migraciones y seed en este arranque.");
                return;
            }
        }

        // 1) Migraciones (tolerantes en cualquier entorno con pre-chequeo)
        try
        {
            // Pre-chequeo: si existe esquema pero no historial de migraciones, omitir Migrate()
            var shouldMigrate = true;
            if (esRelacional)
            {
                // Usar una conexión NUEVA para no disponer la que EF administra internamente
                var connectionString = db.Database.GetConnectionString();
                await using (var conn = new NpgsqlConnection(connectionString))
                {
                    await conn.OpenAsync(cancellationToken);
                    // ¿Existen tablas señal (esquema ya creado)? Roles, Users o personas
                    await using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "select to_regclass('public.\"AspNetRoles\"') is not null";
                        var existsRoles = (bool)(await cmd.ExecuteScalarAsync(cancellationToken) ?? false);

                        cmd.CommandText = "select to_regclass('public.\"AspNetUsers\"') is not null";
                        var existsUsers = (bool)(await cmd.ExecuteScalarAsync(cancellationToken) ?? false);

                        cmd.CommandText = "select to_regclass('public.personas') is not null";
                        var existsPersonas = (bool)(await cmd.ExecuteScalarAsync(cancellationToken) ?? false);

                        // ¿Existe el historial de migraciones?
                        cmd.CommandText = "select to_regclass('public.\"__EFMigrationsHistory\"') is not null";
                        var existsHistoryTable = (bool)(await cmd.ExecuteScalarAsync(cancellationToken) ?? false);

                        // Si hay tabla de historial, consultar número de filas aplicadas
                        long appliedCount = -1;
                        if (existsHistoryTable)
                        {
                            cmd.CommandText = "select count(*) from \"__EFMigrationsHistory\"";
                            var obj = await cmd.ExecuteScalarAsync(cancellationToken);
                            if (obj != null && obj != DBNull.Value)
                            {
                                appliedCount = Convert.ToInt64(obj);
                            }
                        }

                        // Si ya existe esquema clave, evitamos ejecutar Migrate (evita intentos de CREATE duplicados)
                        if (existsRoles || existsUsers || existsPersonas)
                        {
                            shouldMigrate = false;
                            _logger.LogWarning("Esquema existente detectado (tablas clave presentes). Se omite Migrate().");
                        }
                    }
                }

                if (shouldMigrate)
                {
                    _logger.LogInformation("Aplicando migraciones de base de datos...");
                    await db.Database.MigrateAsync(cancellationToken);
                }
            }
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("PendingModelChangesWarning", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Omitiendo migraciones por cambios pendientes de modelo: {Mensaje}", ex.Message);
        }
        catch (PostgresException ex) when (ex.SqlState == "42P07") // relation already exists
        {
            _logger.LogWarning("Esquema existente detectado. Omitiendo migraciones: {Mensaje}", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error aplicando migraciones. Continúa el arranque en Development.");
        }

        // 2) Seed de catálogos/roles/usuario admin (best-effort)
        try
        {
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
            _logger.LogError(ex, "Error durante siembra de datos. La aplicación continúa ejecutándose.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
