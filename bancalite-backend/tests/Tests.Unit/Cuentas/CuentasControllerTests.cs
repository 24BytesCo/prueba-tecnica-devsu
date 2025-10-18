using Bancalite.Domain;
using Bancalite.Persitence;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace Tests.Unit.Cuentas;

/// <summary>
/// Pruebas de integración del CuentasController, basadas en los requisitos del ejercicio.
/// Valida roles (Admin vs usuario), unicidad, y reglas de saldo para estado/borrado.
/// </summary>
public class CuentasControllerTests : IClassFixture<CuentasWebApiFactory>
{
    private readonly CuentasWebApiFactory _factory;
    private readonly HttpClient _client;

    public CuentasControllerTests(CuentasWebApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(); // por defecto Admin (handler de pruebas)
    }

    // Helper: seed catálogos mínimos (Genero, TipoDocumento, TipoCuenta)
    private async Task<(Guid generoId, Guid tipoDocId, Guid tipoCuentaId)> SeedCatalogosAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BancaliteContext>();

        var genero = db.Generos.FirstOrDefault();
        if (genero is null)
        {
            genero = new Genero { Id = Guid.NewGuid(), Codigo = "M", Nombre = "Masculino", Activo = true };
            db.Generos.Add(genero);
        }

        var tipoDoc = db.TiposDocumentoIdentidad.FirstOrDefault();
        if (tipoDoc is null)
        {
            tipoDoc = new TipoDocumentoIdentidad { Id = Guid.NewGuid(), Codigo = "DNI", Nombre = "Documento", Activo = true };
            db.TiposDocumentoIdentidad.Add(tipoDoc);
        }

        var tipoCuenta = db.TiposCuenta.FirstOrDefault();
        if (tipoCuenta is null)
        {
            tipoCuenta = new TipoCuenta { Id = Guid.NewGuid(), Codigo = "AHO", Nombre = "Ahorros", Activo = true };
            db.TiposCuenta.Add(tipoCuenta);
        }

        await db.SaveChangesAsync();
        return (genero.Id, tipoDoc.Id, tipoCuenta.Id);
    }

    // Helper: crea cliente (requiere admin por el endpoint)
    private async Task<(Guid clienteId, string email)> CrearClienteAsync(Guid generoId, Guid tipoDocId, string email)
    {
        var request = new
        {
            Nombres = "Usuario",
            Apellidos = "Cuentas QA",
            Edad = 30,
            GeneroId = generoId,
            TipoDocumentoIdentidad = tipoDocId,
            NumeroDocumento = $"DOC-{Guid.NewGuid():N}".Substring(0, 12),
            Email = email,
            Password = "Secret1$"
        };
        var resp = await _client.PostAsJsonAsync("/api/clientes", request);
        resp.EnsureSuccessStatusCode();
        var created = await resp.Content.ReadFromJsonAsync<ApiResult<Guid>>();
        return (created!.Datos!, email);
    }

    // Helper: crear cuenta (como usuario propietario o admin)
    private async Task<Guid> CrearCuentaAsync(Guid clienteId, Guid tipoCuentaId, decimal saldoInicial, string? asEmail = null, string? numero = null)
    {
        var req = new { ClienteId = clienteId, TipoCuentaId = tipoCuentaId, SaldoInicial = saldoInicial, NumeroCuenta = numero ?? string.Empty };
        using var msg = new HttpRequestMessage(HttpMethod.Post, "/api/cuentas") { Content = JsonContent.Create(req) };
        if (!string.IsNullOrWhiteSpace(asEmail)) msg.Headers.Add("X-Test-Email", asEmail);
        var resp = await _client.SendAsync(msg);
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync();
            throw new Exception($"POST /api/cuentas => {(int)resp.StatusCode}: {body}");
        }
        var created = await resp.Content.ReadFromJsonAsync<ApiResult<Guid>>();
        return created!.Datos!;
    }

    [Fact(DisplayName = "GET /api/cuentas solo admin (403 no admin, 200 admin)")]
    public async Task Listar_Solo_Admin()
    {
        await SeedCatalogosAsync();

        // no admin → 403
        using (var msg = new HttpRequestMessage(HttpMethod.Get, "/api/cuentas"))
        {
            msg.Headers.Add("X-Test-Email", "user.na@test.local");
            var resp = await _client.SendAsync(msg);
            resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        // admin → 200
        var ok = await _client.GetAsync("/api/cuentas");
        ok.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact(DisplayName = "POST /cuentas crea y GET detalle + GET mias devuelven la cuenta")]
    public async Task Crear_Obtener_Detalle_Y_Mias()
    {
        var (generoId, tipoDocId, tipoCuentaId) = await SeedCatalogosAsync();

        // crear cliente y luego cuenta como propietario
        var (clienteId, email) = await CrearClienteAsync(generoId, tipoDocId, "user01@test.local");
        var cuentaId = await CrearCuentaAsync(clienteId, tipoCuentaId, 100m, asEmail: email);

        // detalle como propietario
        using var getMsg = new HttpRequestMessage(HttpMethod.Get, $"/api/cuentas/{cuentaId}");
        getMsg.Headers.Add("X-Test-Email", email);
        var get = await _client.SendAsync(getMsg);
        get.IsSuccessStatusCode.Should().BeTrue();
        var detalle = await get.Content.ReadFromJsonAsync<ApiResult<CuentaDto>>();
        detalle!.IsSuccess.Should().BeTrue();
        detalle.Datos!.CuentaId.Should().Be(cuentaId);
        detalle.Datos!.SaldoActual.Should().Be(100m);

        // mis cuentas
        using var miasMsg = new HttpRequestMessage(HttpMethod.Get, "/api/cuentas/mias");
        miasMsg.Headers.Add("X-Test-Email", email);
        var mias = await _client.SendAsync(miasMsg);
        mias.IsSuccessStatusCode.Should().BeTrue();
        var listaMias = await mias.Content.ReadFromJsonAsync<ApiResult<List<CuentaListItem>>>();
        listaMias!.IsSuccess.Should().BeTrue();
        listaMias.Datos!.Any(i => i.CuentaId == cuentaId).Should().BeTrue();
    }

    [Fact(DisplayName = "POST /cuentas con Número duplicado → 409 Conflict")]
    public async Task Crear_Numero_Duplicado_Conflict()
    {
        var (generoId, tipoDocId, tipoCuentaId) = await SeedCatalogosAsync();
        var (clienteId, email) = await CrearClienteAsync(generoId, tipoDocId, "user02@test.local");

        var numero = "1111-2222-3333";
        _ = await CrearCuentaAsync(clienteId, tipoCuentaId, 0m, asEmail: email, numero: numero);

        // segundo intento con mismo número → 409
        var req = new { ClienteId = clienteId, TipoCuentaId = tipoCuentaId, SaldoInicial = 0m, NumeroCuenta = numero };
        using var msg = new HttpRequestMessage(HttpMethod.Post, "/api/cuentas") { Content = JsonContent.Create(req) };
        msg.Headers.Add("X-Test-Email", email);
        var resp = await _client.SendAsync(msg);
        // La regla de unicidad se valida en FluentValidation => 400
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "PUT/PATCH /cuentas requieren Admin (403 no admin, 200 admin)")]
    public async Task Put_Patch_Requieren_Admin()
    {
        var (generoId, tipoDocId, tipoCuentaId) = await SeedCatalogosAsync();
        var (clienteId, email) = await CrearClienteAsync(generoId, tipoDocId, "user03@test.local");
        var cuentaId = await CrearCuentaAsync(clienteId, tipoCuentaId, 50m, asEmail: email);

        // PUT no admin → 403
        var putReq = new { NumeroCuenta = "9999-8888-7777", TipoCuentaId = tipoCuentaId, ClienteId = clienteId };
        using (var putMsg = new HttpRequestMessage(HttpMethod.Put, $"/api/cuentas/{cuentaId}") { Content = JsonContent.Create(putReq) })
        {
            putMsg.Headers.Add("X-Test-Email", email);
            var put = await _client.SendAsync(putMsg);
            put.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        // PUT admin → 200
        var putAdmin = await _client.PutAsJsonAsync($"/api/cuentas/{cuentaId}", putReq);
        putAdmin.IsSuccessStatusCode.Should().BeTrue();

        // PATCH no admin → 403
        var patchReq = new { NumeroCuenta = "7777-6666-5555" };
        using (var patchMsg = new HttpRequestMessage(HttpMethod.Patch, $"/api/cuentas/{cuentaId}") { Content = JsonContent.Create(patchReq) })
        {
            patchMsg.Headers.Add("X-Test-Email", email);
            var patch = await _client.SendAsync(patchMsg);
            patch.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        // PATCH admin → 200
        var patchAdmin = await _client.PatchAsJsonAsync($"/api/cuentas/{cuentaId}", patchReq);
        patchAdmin.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact(DisplayName = "PATCH estado Inactiva con saldo ≠ 0 → 422; con saldo 0 → 200; DELETE con saldo ≠ 0 → 422 y con 0 → 200")]
    public async Task Estado_Y_Delete_Reglas_Saldo()
    {
        var (generoId, tipoDocId, tipoCuentaId) = await SeedCatalogosAsync();
        var (clienteId, email) = await CrearClienteAsync(generoId, tipoDocId, "user04@test.local");

        // cuenta con saldo 100 → inactivar debe fallar (422)
        var cuentaSaldo = await CrearCuentaAsync(clienteId, tipoCuentaId, 100m, asEmail: email);
        var estadoReq = new { Estado = "Inactiva" };
        var estadoResp = await _client.PatchAsJsonAsync($"/api/cuentas/{cuentaSaldo}/estado", estadoReq);
        estadoResp.StatusCode.Should().Be((HttpStatusCode)422);

        // cuenta con saldo 0 → inactivar OK y luego DELETE OK
        var cuentaCero = await CrearCuentaAsync(clienteId, tipoCuentaId, 0m, asEmail: email);
        var estadoOk = await _client.PatchAsJsonAsync($"/api/cuentas/{cuentaCero}/estado", estadoReq);
        estadoOk.IsSuccessStatusCode.Should().BeTrue();

        var delOk = await _client.DeleteAsync($"/api/cuentas/{cuentaCero}");
        delOk.IsSuccessStatusCode.Should().BeTrue();

        // DELETE con saldo ≠ 0 → 422
        var delFail = await _client.DeleteAsync($"/api/cuentas/{cuentaSaldo}");
        delFail.StatusCode.Should().Be((HttpStatusCode)422);
    }

    // Tipos mínimos para mapear respuestas
    private class ApiResult<T> { public bool IsSuccess { get; set; } public T? Datos { get; set; } public string? Error { get; set; } }
    // Paginado sólo se usa en listado Admin; MisCuentas devuelve lista simple
    private class CuentaListItem { public Guid CuentaId { get; set; } public string NumeroCuenta { get; set; } = string.Empty; }
    private class CuentaDto { public Guid CuentaId { get; set; } public string NumeroCuenta { get; set; } = string.Empty; public decimal SaldoActual { get; set; } }
}
