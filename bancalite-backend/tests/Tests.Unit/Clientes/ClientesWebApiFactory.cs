using Bancalite.Persitence;
using Bancalite.Persitence.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

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

            // Seed de roles y usuario admin para que las verificaciones en handlers (DB) pasen
            services.AddHostedService<ClientesSeedHostedService>();
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
        // Permitir sobreescribir el Email vía cabecera para simular distintos usuarios
        var email = Context.Request.Headers.TryGetValue("X-Test-Email", out var hVal)
            ? (string)hVal!
            : "admin@test.local";

        // Si el email no es el admin, no agregamos rol Admin (simula usuario normal)
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, email.Split('@')[0])
        };
        if (string.Equals(email, "admin@test.local", StringComparison.OrdinalIgnoreCase))
        {
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

// HostedService para crear roles y usuario admin en la DB en memoria
public class ClientesSeedHostedService : IHostedService
{
    private readonly IServiceProvider _services;
    public ClientesSeedHostedService(IServiceProvider services) { _services = services; }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        foreach (var r in new[] { "Admin", "User" })
            if (!await roleManager.RoleExistsAsync(r)) await roleManager.CreateAsync(new IdentityRole<Guid>(r));

        var admin = await userManager.FindByEmailAsync("admin@test.local");
        if (admin == null)
        {
            admin = new AppUser { Id = Guid.NewGuid(), Email = "admin@test.local", UserName = "admin", EmailConfirmed = true, DisplayName = "Admin Test" };
            await userManager.CreateAsync(admin, "Admin123$");
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
