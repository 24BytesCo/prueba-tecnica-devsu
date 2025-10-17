using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Bancalite.Application.Interface;
using Bancalite.Persitence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Bancalite.Persitence.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Tests.Unit.Auth;

// WebApplicationFactory personalizado para pruebas de integración
public class CustomWebApiFactory : WebApplicationFactory<Program>
{
    // Configuración del host para pruebas
    // Usa InMemoryDb, parámetros fijos para JWT y un IEmailSender de prueba
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Production");
        builder.ConfigureAppConfiguration((ctx, cfg) =>
        {
            var dict = new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = "UseInMemoryDb",
                ["JWT:Key"] = "unit-test-key-0123456789abcdef0123456789abcdef",
                ["JWT:Issuer"] = "Bancalite.WebApi",
                ["JWT:Audience"] = "Bancalite.Client",
                ["JWT:ExpiresMinutes"] = "30",
                ["Auth:RefreshDays"] = "7",
                ["ADMIN_USERNAME"] = "admin",
                ["ADMIN_EMAIL"] = "admin@test.local",
                ["ADMIN_PASSWORD"] = "Admin123$",
                ["Smtp:Host"] = "smtp.test",
                ["Smtp:Port"] = "25",
                ["Smtp:EnableSsl"] = "false",
                ["Smtp:Username"] = "user",
                ["Smtp:Password"] = "pass",
                ["Smtp:SenderEmail"] = "no-reply@test.local"
            };
            cfg.AddInMemoryCollection(dict!);
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<BancaliteContext>));
            if (descriptor != null) services.Remove(descriptor);
            services.AddDbContext<BancaliteContext>(o => o.UseInMemoryDatabase("AuthTestsDb"));

            services.AddSingleton<IEmailSender, TestEmailSender>();
            services.AddHostedService<TestSeedHostedService>();

            // Forzar par�metros de JwtBearer en pruebas (clave y validaci�n)
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, o =>
            {
                var key = Encoding.UTF8.GetBytes("unit-test-key-0123456789abcdef0123456789abcdef");
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = "Bancalite.WebApi",
                    ValidateAudience = true,
                    ValidAudience = "Bancalite.Client",
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
        });
    }
}

// IEmailSender de prueba que almacena el último mensaje enviado en memoria
public class TestEmailSender : IEmailSender
{
    public record Message(string To, string Subject, string Html, string? Text);
    public Message? Last { get; private set; }

    public Task SendAsync(string to, string subject, string htmlBody, string? textBody = null, CancellationToken ct = default)
    {
        Last = new Message(to, subject, htmlBody, textBody);
        return Task.CompletedTask;
    }
}

// HostedService para inicializar datos de prueba (roles y usuario admin)
public class TestSeedHostedService : IHostedService
{
    private readonly IServiceProvider _services;
    public TestSeedHostedService(IServiceProvider services) { _services = services; }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BancaliteContext>();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        foreach (var r in new[] { "Admin", "User" })
            if (!await roleManager.RoleExistsAsync(r)) await roleManager.CreateAsync(new IdentityRole<Guid>(r));

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var admin = await userManager.FindByEmailAsync("admin@test.local");
        if (admin == null)
        {
            admin = new AppUser { Id = Guid.NewGuid(), Email = "admin@test.local", UserName = "admin", EmailConfirmed = true, DisplayName = "Admin System" };
            await userManager.CreateAsync(admin, "Admin123$");
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}





