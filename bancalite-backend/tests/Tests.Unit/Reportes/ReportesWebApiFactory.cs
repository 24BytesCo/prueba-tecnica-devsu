using Bancalite.Persitence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Unit.Reportes;

/// <summary>
/// Fábrica para pruebas de Reportes (InMemory + auth de prueba).
/// </summary>
public class ReportesWebApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((ctx, cfg) =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = "UseInMemoryDb"
            }!);
        });

        builder.ConfigureServices(services =>
        {
            // DbContext InMemory
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<BancaliteContext>));
            if (descriptor != null) services.Remove(descriptor);
            services.AddDbContext<BancaliteContext>(o => o.UseInMemoryDatabase("ReportesTestsDb"));

            // Autenticación de pruebas
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
                options.DefaultScheme = "Test";
            }).AddScheme<AuthenticationSchemeOptions, Tests.Unit.Clientes.TestAuthHandler>("Test", _ => { });

            // Seed de roles/usuario admin
            services.AddHostedService<Tests.Unit.Clientes.ClientesSeedHostedService>();
        });
    }
}

