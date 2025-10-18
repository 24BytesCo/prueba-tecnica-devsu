using Bancalite.Persitence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Unit.Movimientos;

/// <summary>
/// WebApplicationFactory para pruebas de Movimientos.
/// </summary>
public class MovimientosWebApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((ctx, cfg) =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = "UseInMemoryDb",
                ["Movimientos:TopeDiario"] = "1000"
            }!);
        });
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<BancaliteContext>));
            if (descriptor != null) services.Remove(descriptor);
            services.AddDbContext<BancaliteContext>(o => o.UseInMemoryDatabase("MovimientosTestsDb"));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
                options.DefaultScheme = "Test";
            }).AddScheme<AuthenticationSchemeOptions, Tests.Unit.Clientes.TestAuthHandler>("Test", _ => { });

            services.AddHostedService<Tests.Unit.Clientes.ClientesSeedHostedService>();
        });
    }
}

