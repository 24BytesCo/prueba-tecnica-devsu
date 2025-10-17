using System.Net;
using System.Net.Http.Json;
using Bancalite.Persitence;
using Bancalite.Domain;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Unit.Reportes;

/// <summary>
/// Pruebas de integración del módulo Reportes (JSON y PDF).
/// </summary>
public class ReportesControllerTests : IClassFixture<ReportesWebApiFactory>
{
    private readonly ReportesWebApiFactory _factory;
    private readonly HttpClient _client;

    public ReportesControllerTests(ReportesWebApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // Utilidades de siembra
    private async Task SeedTiposMovimientoAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BancaliteContext>();
        if (!db.TiposMovimiento.Any())
        {
            db.TiposMovimiento.AddRange(
                new TipoMovimiento { Id = Guid.NewGuid(), Codigo = "CRE", Nombre = "Crédito", Activo = true },
                new TipoMovimiento { Id = Guid.NewGuid(), Codigo = "DEB", Nombre = "Débito", Activo = true }
            );
            await db.SaveChangesAsync();
        }
    }

    private async Task<(Guid generoId, Guid tipoDocId, Guid tipoCuentaId)> SeedCatalogosAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BancaliteContext>();
        var genero = db.Generos.FirstOrDefault() ?? db.Generos.Add(new Genero { Id = Guid.NewGuid(), Codigo = "M", Nombre = "Masculino", Activo = true }).Entity;
        var tipoDoc = db.TiposDocumentoIdentidad.FirstOrDefault() ?? db.TiposDocumentoIdentidad.Add(new TipoDocumentoIdentidad { Id = Guid.NewGuid(), Codigo = "DNI", Nombre = "Documento", Activo = true }).Entity;
        var tipoCuenta = db.TiposCuenta.FirstOrDefault() ?? db.TiposCuenta.Add(new TipoCuenta { Id = Guid.NewGuid(), Codigo = "AHO", Nombre = "Ahorros", Activo = true }).Entity;
        await db.SaveChangesAsync();
        return (genero.Id, tipoDoc.Id, tipoCuenta.Id);
    }

    private async Task<Guid> CrearClienteAsync(Guid generoId, Guid tipoDocId, string email)
    {
        var req = new
        {
            Nombres = "User",
            Apellidos = "RepQA",
            Edad = 30,
            GeneroId = generoId,
            TipoDocumentoIdentidad = tipoDocId,
            NumeroDocumento = $"DOC-{Guid.NewGuid():N}".Substring(0, 12),
            Email = email,
            Password = "Secret1$"
        };
        using var msg = new HttpRequestMessage(HttpMethod.Post, "/api/clientes") { Content = JsonContent.Create(req) };
        msg.Headers.Add("X-Test-Email", "admin@test.local");
        var resp = await _client.SendAsync(msg);
        resp.EnsureSuccessStatusCode();
        var created = await resp.Content.ReadFromJsonAsync<ApiResult<Guid>>();
        // Ajuste: asegurar vínculo con el usuario creado (propietario)
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<BancaliteContext>();
            var user = db.Users.First(u => u.Email == email);
            var cli = db.Clientes.First(c => c.Id == created!.Datos);
            cli.AppUserId = user.Id;
            await db.SaveChangesAsync();
        }
        return created!.Datos!;
    }

    private async Task<(Guid cuentaId, string numeroCuenta)> CrearCuentaAsync(Guid clienteId, Guid tipoCuentaId, decimal saldoInicial)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BancaliteContext>();
        string GenNumero()
        {
            var r = new Random();
            string d() => r.Next(0, 9999).ToString("0000");
            return $"{d()}-{d()}-{d()}";
        }
        var numero = GenNumero();
        while (db.Cuentas.Any(c => c.NumeroCuenta == numero)) numero = GenNumero();
        var cuenta = new Cuenta
        {
            Id = Guid.NewGuid(),
            NumeroCuenta = numero,
            TipoCuentaId = tipoCuentaId,
            ClienteId = clienteId,
            SaldoInicial = saldoInicial,
            SaldoActual = saldoInicial
        };
        db.Cuentas.Add(cuenta);
        await db.SaveChangesAsync();
        return (cuenta.Id, numero);
    }

    private async Task CrearMovimientoAsync(string email, string numeroCuenta, string tipo, decimal monto, string? key = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BancaliteContext>();
        var cuenta = db.Cuentas.First(c => c.NumeroCuenta == numeroCuenta);
        var tipoMov = db.TiposMovimiento.First(t => t.Codigo == tipo);
        var mov = new Movimiento
        {
            Id = Guid.NewGuid(),
            CuentaId = cuenta.Id,
            TipoId = tipoMov.Id,
            Fecha = DateTime.UtcNow,
            Monto = Math.Round(monto, 2, MidpointRounding.ToEven),
            SaldoPrevio = cuenta.SaldoActual,
            SaldoPosterior = tipo == "DEB" ? cuenta.SaldoActual - Math.Round(monto, 2, MidpointRounding.ToEven) : cuenta.SaldoActual + Math.Round(monto, 2, MidpointRounding.ToEven),
            Descripcion = "seed",
            CreatedBy = email
        };
        cuenta.SaldoActual = mov.SaldoPosterior;
        db.Movimientos.Add(mov);
        await db.SaveChangesAsync();
    }

    // JSON: reporte por cliente (dos cuentas)
    [Fact(DisplayName = "Reporte JSON por cliente -> totales y saldos correctos")]
    public async Task Json_Cliente_Ok()
    {
        await SeedTiposMovimientoAsync();
        var (generoId, tipoDocId, tipoCuentaId) = await SeedCatalogosAsync();
        var email = "rep.user01@test.local";
        var clienteId = await CrearClienteAsync(generoId, tipoDocId, email);
        var a1 = await CrearCuentaAsync(clienteId, tipoCuentaId, 500m);
        var a2 = await CrearCuentaAsync(clienteId, tipoCuentaId, 300m);

        await CrearMovimientoAsync(email, a1.numeroCuenta, "CRE", 200m);
        await CrearMovimientoAsync(email, a1.numeroCuenta, "DEB", 50m);
        await CrearMovimientoAsync(email, a2.numeroCuenta, "DEB", 100m);

        var desde = DateTime.UtcNow.Date;
        var hasta = DateTime.UtcNow.Date;
        var url = $"/api/reportes?clienteId={clienteId}&desde={desde:O}&hasta={hasta:O}";
        using var msg = new HttpRequestMessage(HttpMethod.Get, url);
        msg.Headers.Add("X-Test-Email", "admin@test.local");
        var res = await _client.SendAsync(msg);
        res.EnsureSuccessStatusCode();
        var dto = await res.Content.ReadFromJsonAsync<EstadoCuentaDto>();

        dto!.TotalCreditos.Should().Be(200m);
        dto.TotalDebitos.Should().Be(150m);
        dto.SaldoInicial.Should().Be(800m);
        dto.SaldoFinal.Should().Be(850m);
        dto.Movimientos.Count.Should().Be(3);
    }

    // JSON: reporte por número de cuenta
    [Fact(DisplayName = "Reporte JSON por cuenta -> totales correctos")]
    public async Task Json_Cuenta_Ok()
    {
        await SeedTiposMovimientoAsync();
        var (generoId, tipoDocId, tipoCuentaId) = await SeedCatalogosAsync();
        var email = "rep.user02@test.local";
        var clienteId = await CrearClienteAsync(generoId, tipoDocId, email);
        var a1 = await CrearCuentaAsync(clienteId, tipoCuentaId, 1000m);
        await CrearMovimientoAsync(email, a1.numeroCuenta, "DEB", 400m);
        await CrearMovimientoAsync(email, a1.numeroCuenta, "CRE", 50m);

        var desde = DateTime.UtcNow.Date;
        var hasta = DateTime.UtcNow.Date;
        var url = "/api/reportes?numeroCuenta=" + Uri.EscapeDataString(a1.numeroCuenta) + "&desde=" + desde.ToString("O") + "&hasta=" + hasta.ToString("O");
        using var msg = new HttpRequestMessage(HttpMethod.Get, url);
        msg.Headers.Add("X-Test-Email", "admin@test.local");
        var res = await _client.SendAsync(msg);
        res.EnsureSuccessStatusCode();
        var dto = await res.Content.ReadFromJsonAsync<EstadoCuentaDto>();

        dto!.TotalCreditos.Should().Be(50m);
        dto.TotalDebitos.Should().Be(400m);
        dto.SaldoInicial.Should().Be(1000m);
        dto.SaldoFinal.Should().Be(650m);
        dto.Movimientos.Count.Should().Be(2);
    }

    // JSON: validaciones (400)
    [Fact(DisplayName = "Reporte JSON sin filtros -> 400")]
    public async Task Json_SinFiltros_400()
    {
        var desde = DateTime.UtcNow.Date;
        var hasta = DateTime.UtcNow.Date;
        var url = $"/api/reportes?desde={desde:O}&hasta={hasta:O}";
        using var msg = new HttpRequestMessage(HttpMethod.Get, url);
        msg.Headers.Add("X-Test-Email", "admin@test.local");
        var res = await _client.SendAsync(msg);
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Reporte JSON con rango inválido -> 400")]
    public async Task Json_RangoInvalido_400()
    {
        var desde = DateTime.UtcNow.Date;
        var hasta = DateTime.UtcNow.Date.AddDays(-1);
        var url = $"/api/reportes?numeroCuenta=XXXX&desde={desde:O}&hasta={hasta:O}";
        using var msg = new HttpRequestMessage(HttpMethod.Get, url);
        msg.Headers.Add("X-Test-Email", "admin@test.local");
        var res = await _client.SendAsync(msg);
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // JSON: seguridad (403)
    [Fact(DisplayName = "Reporte JSON por cuenta de otro usuario -> 403")]
    public async Task Json_NoPropietario_403()
    {
        await SeedTiposMovimientoAsync();
        var (generoId, tipoDocId, tipoCuentaId) = await SeedCatalogosAsync();
        var emailOwner = "rep.owner@test.local";
        var clienteId = await CrearClienteAsync(generoId, tipoDocId, emailOwner);
        var a1 = await CrearCuentaAsync(clienteId, tipoCuentaId, 100m);

        var desde = DateTime.UtcNow.Date;
        var hasta = DateTime.UtcNow.Date;
        var url = "/api/reportes?numeroCuenta=" + Uri.EscapeDataString(a1.numeroCuenta) + "&desde=" + desde.ToString("O") + "&hasta=" + hasta.ToString("O");
        using var msg = new HttpRequestMessage(HttpMethod.Get, url);
        msg.Headers.Add("X-Test-Email", "otro.user@test.local");
        var res = await _client.SendAsync(msg);
        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // JSON: 404
    [Fact(DisplayName = "Reporte JSON número inexistente -> 404")]
    public async Task Json_NoDatos_404()
    {
        var desde = DateTime.UtcNow.Date;
        var hasta = DateTime.UtcNow.Date;
        var url = $"/api/reportes?numeroCuenta=NO-EXISTE&desde={desde:O}&hasta={hasta:O}";
        using var msg = new HttpRequestMessage(HttpMethod.Get, url);
        msg.Headers.Add("X-Test-Email", "admin@test.local");
        var res = await _client.SendAsync(msg);
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // PDF: cabeceras y no vacío
    [Fact(DisplayName = "Reporte PDF por cuenta -> headers correctos y no vacío")]
    public async Task Pdf_Ok_Headers()
    {
        await SeedTiposMovimientoAsync();
        var (generoId, tipoDocId, tipoCuentaId) = await SeedCatalogosAsync();
        var email = "rep.user03@test.local";
        var clienteId = await CrearClienteAsync(generoId, tipoDocId, email);
        var a1 = await CrearCuentaAsync(clienteId, tipoCuentaId, 100m);
        await CrearMovimientoAsync(email, a1.numeroCuenta, "CRE", 20m);

        var desde = DateTime.UtcNow.Date;
        var hasta = DateTime.UtcNow.Date;
        var url = "/api/reportes/pdf?numeroCuenta=" + Uri.EscapeDataString(a1.numeroCuenta) + "&desde=" + desde.ToString("O") + "&hasta=" + hasta.ToString("O");
        using var msg = new HttpRequestMessage(HttpMethod.Get, url);
        msg.Headers.Add("X-Test-Email", "admin@test.local");
        var res = await _client.SendAsync(msg);
        res.EnsureSuccessStatusCode();
        res.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
        var bytes = await res.Content.ReadAsByteArrayAsync();
        bytes.Length.Should().BeGreaterThan(200);
    }

    // PDF: paginación
    [Fact(DisplayName = "Reporte PDF con muchos movimientos -> tamaño mayor (paginación)")]
    public async Task Pdf_Paginacion_TamanoMayor()
    {
        await SeedTiposMovimientoAsync();
        var (generoId, tipoDocId, tipoCuentaId) = await SeedCatalogosAsync();
        var email = "rep.user04@test.local";
        var clienteId = await CrearClienteAsync(generoId, tipoDocId, email);
        var a1 = await CrearCuentaAsync(clienteId, tipoCuentaId, 1000m);

        for (int i = 0; i < 15; i++) await CrearMovimientoAsync(email, a1.numeroCuenta, "CRE", 10m, key: $"k{i}");
        var desde = DateTime.UtcNow.Date; var hasta = DateTime.UtcNow.Date;
        var urlSmall = "/api/reportes/pdf?numeroCuenta=" + Uri.EscapeDataString(a1.numeroCuenta) + "&desde=" + desde.ToString("O") + "&hasta=" + hasta.ToString("O");
        using var msgSmall = new HttpRequestMessage(HttpMethod.Get, urlSmall);
        msgSmall.Headers.Add("X-Test-Email", email);
        var pdfSmall = await (await _client.SendAsync(msgSmall)).Content.ReadAsByteArrayAsync();

        for (int i = 15; i < 75; i++) await CrearMovimientoAsync(email, a1.numeroCuenta, "DEB", 5m, key: $"k{i}");
        var urlBig = "/api/reportes/pdf?numeroCuenta=" + Uri.EscapeDataString(a1.numeroCuenta) + "&desde=" + desde.ToString("O") + "&hasta=" + hasta.ToString("O");
        using var msgBig = new HttpRequestMessage(HttpMethod.Get, urlBig);
        msgBig.Headers.Add("X-Test-Email", email);
        var pdfBig = await (await _client.SendAsync(msgBig)).Content.ReadAsByteArrayAsync();

        pdfBig.Length.Should().BeGreaterOrEqualTo(pdfSmall.Length);
    }

    // Tipos locales para respuestas simples
    private class ApiResult<T> { public bool IsSuccess { get; set; } public T? Datos { get; set; } public string? Error { get; set; } }
    private class EstadoCuentaDto
    {
        public Guid? ClienteId { get; set; }
        public string? ClienteNombre { get; set; }
        public string? NumeroCuenta { get; set; }
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public decimal TotalCreditos { get; set; }
        public decimal TotalDebitos { get; set; }
        public decimal SaldoInicial { get; set; }
        public decimal SaldoFinal { get; set; }
        public List<Item> Movimientos { get; set; } = new();
        public class Item
        {
            public DateTime Fecha { get; set; }
            public string NumeroCuenta { get; set; } = string.Empty;
            public string TipoCodigo { get; set; } = string.Empty;
            public decimal Monto { get; set; }
            public decimal SaldoPrevio { get; set; }
            public decimal SaldoPosterior { get; set; }
            public string? Descripcion { get; set; }
        }
    }
}
