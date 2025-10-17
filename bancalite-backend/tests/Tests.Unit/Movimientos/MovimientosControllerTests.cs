using System.Net;
using System.Net.Http.Json;
using Bancalite.Persitence;
using Bancalite.Domain;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Unit.Movimientos;

/// <summary>
/// Pruebas de integración del módulo Movimientos basadas en el documento de requisitos.
/// </summary>
public class MovimientosControllerTests : IClassFixture<MovimientosWebApiFactory>
{
    private readonly MovimientosWebApiFactory _factory;
    private readonly HttpClient _client;

    public MovimientosControllerTests(MovimientosWebApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // Utilidades de prueba
    private async Task SeedTiposMovimientoAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BancaliteContext>();
        if (!db.TiposMovimiento.Any())
        {
            db.TiposMovimiento.AddRange(new TipoMovimiento { Id = Guid.NewGuid(), Codigo = "CRE", Nombre = "Crédito", Activo = true },
                                        new TipoMovimiento { Id = Guid.NewGuid(), Codigo = "DEB", Nombre = "Débito", Activo = true });
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
        var req = new { Nombres = "User", Apellidos = "MovQA", Edad = 30, GeneroId = generoId, TipoDocumentoIdentidad = tipoDocId, NumeroDocumento = $"DOC-{Guid.NewGuid():N}".Substring(0, 12), Email = email, Password = "Secret1$" };
        var resp = await _client.PostAsJsonAsync("/api/clientes", req);
        resp.EnsureSuccessStatusCode();
        var created = await resp.Content.ReadFromJsonAsync<ApiResult<Guid>>();
        return created!.Datos!;
    }

    private async Task<(string numeroCuenta, Guid cuentaId)> CrearCuentaAsync(Guid clienteId, Guid tipoCuentaId, decimal saldoInicial, string email)
    {
        var req = new { ClienteId = clienteId, TipoCuentaId = tipoCuentaId, SaldoInicial = saldoInicial, NumeroCuenta = "" };
        using var msg = new HttpRequestMessage(HttpMethod.Post, "/api/cuentas") { Content = JsonContent.Create(req) };
        msg.Headers.Add("X-Test-Email", email);
        var resp = await _client.SendAsync(msg);
        resp.EnsureSuccessStatusCode();
        var created = await resp.Content.ReadFromJsonAsync<ApiResult<Guid>>();
        // Obtener detalle para conocer el número asignado
        var cuentaId = created!.Datos;
        var det = await _client.GetFromJsonAsync<ApiResult<CuentaDto>>($"/api/cuentas/{cuentaId}");
        return (det!.Datos!.NumeroCuenta, cuentaId);
    }

    private async Task<(HttpResponseMessage resp, MovimientoDto? mov)> PostMovimientoAsync(string email, string numeroCuenta, string tipoCodigo, decimal monto, string? key = null)
    {
        var req = new { NumeroCuenta = numeroCuenta, TipoCodigo = tipoCodigo, Monto = monto, IdempotencyKey = key, Descripcion = "QA" };
        using var msg = new HttpRequestMessage(HttpMethod.Post, "/api/movimientos") { Content = JsonContent.Create(req) };
        msg.Headers.Add("X-Test-Email", email);
        var resp = await _client.SendAsync(msg);
        MovimientoDto? mov = null;
        if ((int)resp.StatusCode >= 200 && (int)resp.StatusCode < 300)
            mov = await resp.Content.ReadFromJsonAsync<MovimientoDto>();
        return (resp, mov);
    }

    // Casos válidos
    [Fact(DisplayName = "Crédito y Débito dentro de límites → 201 y saldoPosterior correcto")]
    public async Task Credito_Debito_Validos_Should_Return201_And_UpdateSaldo()
    {
        await SeedTiposMovimientoAsync();
        var (generoId, tipoDocId, tipoCuentaId) = await SeedCatalogosAsync();
        var email = "mov.user01@test.local";
        var clienteId = await CrearClienteAsync(generoId, tipoDocId, email);
        var (numero, _) = await CrearCuentaAsync(clienteId, tipoCuentaId, 700m, email);

        // CR 600 → saldo 1300
        var (crResp, cr) = await PostMovimientoAsync(email, numero, "CRE", 600m);
        crResp.StatusCode.Should().Be(HttpStatusCode.Created);
        cr!.SaldoPosterior.Should().Be(1300m);

        // DB 575 → saldo 725
        var (dbResp, db) = await PostMovimientoAsync(email, numero, "DEB", 575m);
        dbResp.StatusCode.Should().Be(HttpStatusCode.Created);
        db!.SaldoPosterior.Should().Be(725m);
    }

    // Caso: débito sin saldo suficiente
    [Fact(DisplayName = "Débito sin saldo suficiente → 422 'Saldo no disponible'")]
    public async Task Debito_SinSaldo_Should_422()
    {
        await SeedTiposMovimientoAsync();
        var (generoId, tipoDocId, tipoCuentaId) = await SeedCatalogosAsync();
        var email = "mov.user02@test.local";
        var clienteId = await CrearClienteAsync(generoId, tipoDocId, email);
        var (numero, _) = await CrearCuentaAsync(clienteId, tipoCuentaId, 0m, email);

        var (resp, _) = await PostMovimientoAsync(email, numero, "DEB", 150m);
        resp.StatusCode.Should().Be((HttpStatusCode)422);
        var err = await resp.Content.ReadAsStringAsync();
        err.Should().Contain("Saldo no disponible");
    }

    // Caso: tope diario excedido
    [Fact(DisplayName = "Tope diario excedido → 422 'Cupo diario Excedido'")]
    public async Task Debito_TopeDiario_Should_422()
    {
        await SeedTiposMovimientoAsync();
        var (generoId, tipoDocId, tipoCuentaId) = await SeedCatalogosAsync();
        var email = "mov.user03@test.local";
        var clienteId = await CrearClienteAsync(generoId, tipoDocId, email);
        var (numero, _) = await CrearCuentaAsync(clienteId, tipoCuentaId, 2000m, email);

        // Acumular 800 debit
        (await PostMovimientoAsync(email, numero, "DEB", 800m)).resp.EnsureSuccessStatusCode();

        // Intentar 300 más (total 1100) → tope 1000 excedido
        var (resp, _) = await PostMovimientoAsync(email, numero, "DEB", 300m);
        resp.StatusCode.Should().Be((HttpStatusCode)422);
        var err = await resp.Content.ReadAsStringAsync();
        err.Should().Contain("Cupo diario Excedido");
    }

    // Caso: idempotencia (reintentos)
    [Fact(DisplayName = "Idempotencia (misma key) → mismo MovimientoId y sin duplicar")]
    public async Task Idempotencia_Should_Return_Same_Result()
    {
        await SeedTiposMovimientoAsync();
        var (generoId, tipoDocId, tipoCuentaId) = await SeedCatalogosAsync();
        var email = "mov.user04@test.local";
        var clienteId = await CrearClienteAsync(generoId, tipoDocId, email);
        var (numero, _) = await CrearCuentaAsync(clienteId, tipoCuentaId, 500m, email);

        var key = Guid.NewGuid().ToString("N");
        var (r1, m1) = await PostMovimientoAsync(email, numero, "CRE", 123.456m, key);
        r1.IsSuccessStatusCode.Should().BeTrue();

        var (r2, m2) = await PostMovimientoAsync(email, numero, "CRE", 123.456m, key);
        r2.IsSuccessStatusCode.Should().BeTrue();

        m2!.MovimientoId.Should().Be(m1!.MovimientoId);
        m2.SaldoPosterior.Should().Be(m1.SaldoPosterior);
    }
    // Caso: débito exacto al saldo deja saldo en cero
    [Fact(DisplayName = "Débito exacto al saldo → 201 y saldo en 0.00")]
    public async Task Debito_Exacto_DejaSaldoCero()
    {
        await SeedTiposMovimientoAsync();
        var (generoId, tipoDocId, tipoCuentaId) = await SeedCatalogosAsync();
        var email = "mov.user05@test.local";
        var clienteId = await CrearClienteAsync(generoId, tipoDocId, email);
        var (numero, _) = await CrearCuentaAsync(clienteId, tipoCuentaId, 540m, email);

        var (resp, mov) = await PostMovimientoAsync(email, numero, "DEB", 540m);
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        mov!.SaldoPosterior.Should().Be(0m);
    }

    // Caso: saldo 0 y débito
    [Fact(DisplayName = "Saldo 0 y débito → 422 'Saldo no disponible'")]
    public async Task Debito_SobreSaldoCero_Should_422()
    {
        await SeedTiposMovimientoAsync();
        var (generoId, tipoDocId, tipoCuentaId) = await SeedCatalogosAsync();
        var email = "mov.user06@test.local";
        var clienteId = await CrearClienteAsync(generoId, tipoDocId, email);
        var (numero, _) = await CrearCuentaAsync(clienteId, tipoCuentaId, 0m, email);

        var (resp, _) = await PostMovimientoAsync(email, numero, "DEB", 10m);
        resp.StatusCode.Should().Be((HttpStatusCode)422);
        var err = await resp.Content.ReadAsStringAsync();
        err.Should().Contain("Saldo no disponible");
    }

    // Caso: tope diario con 900 acumulado y débito 150
    [Fact(DisplayName = "Tope diario: 900 acumulado + débito 150 → 422")]
    public async Task TopeDiario_900_Mas_150_Should_422()
    {
        await SeedTiposMovimientoAsync();
        var (generoId, tipoDocId, tipoCuentaId) = await SeedCatalogosAsync();
        var email = "mov.user07@test.local";
        var clienteId = await CrearClienteAsync(generoId, tipoDocId, email);
        var (numero, _) = await CrearCuentaAsync(clienteId, tipoCuentaId, 2000m, email);

        (await PostMovimientoAsync(email, numero, "DEB", 400m)).resp.EnsureSuccessStatusCode();
        (await PostMovimientoAsync(email, numero, "DEB", 300m)).resp.EnsureSuccessStatusCode();
        (await PostMovimientoAsync(email, numero, "DEB", 200m)).resp.EnsureSuccessStatusCode();

        var (resp, _) = await PostMovimientoAsync(email, numero, "DEB", 150m);
        resp.StatusCode.Should().Be((HttpStatusCode)422);
        var err = await resp.Content.ReadAsStringAsync();
        err.Should().Contain("Cupo diario Excedido");
    }

    // Caso: monto cero inválido
    [Fact(DisplayName = "Monto 0 → 422 'Monto inválido'")]
    public async Task Monto_Cero_Should_422()
    {
        await SeedTiposMovimientoAsync();
        var (generoId, tipoDocId, tipoCuentaId) = await SeedCatalogosAsync();
        var email = "mov.user08@test.local";
        var clienteId = await CrearClienteAsync(generoId, tipoDocId, email);
        var (numero, _) = await CrearCuentaAsync(clienteId, tipoCuentaId, 0m, email);

        var (resp, _) = await PostMovimientoAsync(email, numero, "DEB", 0m);
        resp.StatusCode.Should().Be((HttpStatusCode)422);
        var err = await resp.Content.ReadAsStringAsync();
        err.Should().Contain("Unprocessable");
    }

    // Caso: cuenta inactiva
    [Fact(DisplayName = "Cuenta inactiva, intento de movimiento → 404")]
    public async Task Cuenta_Inactiva_Should_404()
    {
        await SeedTiposMovimientoAsync();
        var (generoId, tipoDocId, tipoCuentaId) = await SeedCatalogosAsync();
        var email = "mov.user09@test.local";
        var clienteId = await CrearClienteAsync(generoId, tipoDocId, email);
        var (numero, cuentaId) = await CrearCuentaAsync(clienteId, tipoCuentaId, 0m, email);

        var estadoReq = new { Estado = "Inactiva" };
        var estadoOk = await _client.PatchAsJsonAsync($"/api/cuentas/{cuentaId}/estado", estadoReq);
        estadoOk.EnsureSuccessStatusCode();

        var (resp, _) = await PostMovimientoAsync(email, numero, "CRE", 10m);
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // Caso: dos débitos secuenciales (simula concurrencia)
    [Fact(DisplayName = "Dos débitos 800 y 300 sobre saldo 1000 → uno OK, otro 422")]
    public async Task Doble_Debito_Secuencial_Simula_Concurrencia()
    {
        await SeedTiposMovimientoAsync();
        var (generoId, tipoDocId, tipoCuentaId) = await SeedCatalogosAsync();
        var email = "mov.user10@test.local";
        var clienteId = await CrearClienteAsync(generoId, tipoDocId, email);
        var (numero, _) = await CrearCuentaAsync(clienteId, tipoCuentaId, 1000m, email);

        var r1 = (await PostMovimientoAsync(email, numero, "DEB", 800m)).resp;
        var r2 = (await PostMovimientoAsync(email, numero, "DEB", 300m)).resp;

        r1.IsSuccessStatusCode.Should().BeTrue();
        r2.StatusCode.Should().Be((HttpStatusCode)422);
    }
// Tipos locales para mapear respuestas
    private class ApiResult<T> { public bool IsSuccess { get; set; } public T? Datos { get; set; } public string? Error { get; set; } }
    private class CuentaDto { public Guid CuentaId { get; set; } public string NumeroCuenta { get; set; } = string.Empty; }
    private class MovimientoDto { public Guid MovimientoId { get; set; } public Guid CuentaId { get; set; } public string NumeroCuenta { get; set; } = string.Empty; public string TipoCodigo { get; set; } = string.Empty; public decimal Monto { get; set; } public decimal SaldoPrevio { get; set; } public decimal SaldoPosterior { get; set; } public DateTime Fecha { get; set; } }
}






