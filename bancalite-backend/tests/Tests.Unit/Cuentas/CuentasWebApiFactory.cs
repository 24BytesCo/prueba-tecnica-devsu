using Bancalite.Persitence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Unit.Cuentas;

/// <summary>
/// WebApplicationFactory para pruebas del módulo Cuentas.
/// Reutiliza el esquema de autenticación de pruebas y el seed de Identity.
/// </summary>
public class CuentasWebApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((ctx, cfg) =>
        {
            var dict = new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = "UseInMemoryDb"
            };
            cfg.AddInMemoryCollection(dict!);
        });

        builder.ConfigureServices(services =>
        {
            // DbContext en memoria dedicado a este set de pruebas
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<BancaliteContext>));
            if (descriptor != null) services.Remove(descriptor);
            services.AddDbContext<BancaliteContext>(o => o.UseInMemoryDatabase("CuentasTestsDb"));

            // Autenticación de pruebas (mismo handler del módulo Clientes)
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
                options.DefaultScheme = "Test";
            }).AddScheme<AuthenticationSchemeOptions, Tests.Unit.Clientes.TestAuthHandler>("Test", _ => { });

            // Seed de roles y usuario admin
            services.AddHostedService<Tests.Unit.Clientes.ClientesSeedHostedService>();
        });
    }
}

