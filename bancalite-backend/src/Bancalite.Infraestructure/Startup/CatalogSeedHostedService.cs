using Bancalite.Persitence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
            var db = scope.ServiceProvider.GetRequiredService<BancaliteDbContext>();

            _logger.LogInformation("Aplicando migraciones de base de datos...");
            await db.Database.MigrateAsync(cancellationToken);

            _logger.LogInformation("Sembrando catálogos si están vacíos...");
            await db.SeedCatalogosAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante migración/siembra de catálogos");
            // Re-lanzar no es deseable en dev; se continúa para permitir el arranque.
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

