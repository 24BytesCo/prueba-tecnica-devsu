using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Bancalite.Persitence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Tests.Unit.Clientes;

/// <summary>
/// WebApplicationFactory específico para pruebas de Clientes.
/// - InMemoryDb
/// - Autenticación de prueba que siempre autoriza como Admin
/// </summary>
public class ClientesWebApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((ctx, cfg) =>
        {
            // Config mínima para que el host arranque
            var dict = new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = "UseInMemoryDb"
            };
            cfg.AddInMemoryCollection(dict!);
        });

        builder.ConfigureServices(services =>
        {
            // DbContext en memoria
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<BancaliteContext>));
            if (descriptor != null) services.Remove(descriptor);
            services.AddDbContext<BancaliteContext>(o => o.UseInMemoryDatabase("ClientesTestsDb"));

            // Autenticación fake con rol Admin
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
                options.DefaultScheme = "Test";
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
        });
    }
}

// Handler que autoriza siempre y agrega rol Admin
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, "test-user"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
