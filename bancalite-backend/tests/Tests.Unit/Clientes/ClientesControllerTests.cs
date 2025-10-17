using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Bancalite.Persitence;
using Bancalite.Domain;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Unit.Clientes;

/// <summary>
/// Pruebas de integración del ClientesController.
/// Usa InMemoryDb, usuario Admin y JWT reales del proyecto.
/// </summary>
public class ClientesControllerTests : IClassFixture<ClientesWebApiFactory>
{
    private readonly ClientesWebApiFactory _factory;
    private readonly HttpClient _client;

    public ClientesControllerTests(ClientesWebApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact(DisplayName = "GET /status responde OK (smoke)")]
    public async Task Status_Should_Work()
    {
        // smoke: verifica host arriba
        var resp = await _client.GetAsync("/status");
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync();
            throw new Exception($"/status => {(int)resp.StatusCode}: {body}");
        }
    }

    [Fact(DisplayName = "GET /api/clientes (sin datos) responde 200")]
    public async Task GetClientes_Should_Work()
    {
        // call list
        var resp = await _client.GetAsync("/api/clientes");
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync();
            throw new Exception($"/api/clientes => {(int)resp.StatusCode}: {body}");
        }
    }

    // Helper: crea catálogo mínimo (Genero y TipoDocumento) para validar requests
    private async Task<(Guid generoId, Guid tipoDocId)> SeedCatalogosAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BancaliteContext>();

        // Genero, si no existe, crear uno simple
        var genero = db.Generos.FirstOrDefault();
        if (genero is null)
        {
            genero = new Genero { Id = Guid.NewGuid(), Codigo = "M", Nombre = "Masculino", Activo = true };
            db.Generos.Add(genero);
        }

        // Tipo Doc, si no existe, crear uno simple
        var tipoDoc = db.TiposDocumentoIdentidad.FirstOrDefault();
        if (tipoDoc is null)
        {
            tipoDoc = new TipoDocumentoIdentidad { Id = Guid.NewGuid(), Codigo = "DNI", Nombre = "Documento", Activo = true };
            db.TiposDocumentoIdentidad.Add(tipoDoc);
        }

        await db.SaveChangesAsync();
        return (genero.Id, tipoDoc.Id);
    }

    // No-op: con autenticación de prueba siempre somos Admin
    private Task AuthenticateAsAdminAsync() => Task.CompletedTask;

    [Fact(DisplayName = "POST /clientes crea y GET /clientes/{id} devuelve el detalle")]
    public async Task Create_Then_GetById_Should_Work()
    {
        // arrange: catálogos mínimos
        var (generoId, tipoDocId) = await SeedCatalogosAsync();

        // arrange: autenticar admin
        await AuthenticateAsAdminAsync();

        // arrange: payload válido
        var req = new
        {
            Nombres = "Juan Test",
            Apellidos = "Pérez QA",
            Edad = 33,
            GeneroId = generoId,
            TipoDocumentoIdentidad = tipoDocId,
            NumeroDocumento = "QA-0001",
            Direccion = "Calle 1",
            Telefono = "999999",
            Email = "juan.qa0001@test.local",
            Password = "Secret1$"
        };

        // act: crear
        var post = await _client.PostAsJsonAsync("/api/clientes", req);
        post.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await post.Content.ReadFromJsonAsync<ApiResult<Guid>>();
        created!.IsSuccess.Should().BeTrue();
        created.Datos.Should().NotBe(Guid.Empty);

        // act: consultar detalle
        var get = await _client.GetAsync($"/api/clientes/{created!.Datos}");
        get.IsSuccessStatusCode.Should().BeTrue();
        var detail = await get.Content.ReadFromJsonAsync<ApiResult<ClienteDto>>();

        // assert: datos básicos
        detail!.IsSuccess.Should().BeTrue();
        detail.Datos!.Nombres.Should().Be("Juan Test");
        detail.Datos!.Apellidos.Should().Be("Pérez QA");
        detail.Datos!.Estado.Should().BeTrue();
    }

    [Fact(DisplayName = "GET /clientes pagina y filtra por nombres")]
    public async Task List_Pagination_And_Filters_Should_Work()
    {
        // arrange: catálogos mínimos
        var (generoId, tipoDocId) = await SeedCatalogosAsync();
        await AuthenticateAsAdminAsync();

        // arrange: prefijo único
        var prefix = $"PX-{Guid.NewGuid():N}".Substring(0, 12);

        // arrange: 3 clientes del prefijo
        for (var i = 1; i <= 3; i++)
        {
            var req = new
            {
                Nombres = $"{prefix} Nombre {i}",
                Apellidos = "List QA",
                Edad = 25 + i,
                GeneroId = generoId,
                TipoDocumentoIdentidad = tipoDocId,
                NumeroDocumento = $"{prefix}-DOC-{i}",
                Email = $"{prefix}.{i}@test.local",
                Password = "Secret1$"
            };
            var post = await _client.PostAsJsonAsync("/api/clientes", req);
            post.EnsureSuccessStatusCode();
        }

        // act: página 1 tamaño 2
        var page1 = await _client.GetFromJsonAsync<ApiResult<Paged<ClienteListItem>>>(
            $"/api/clientes?nombres={Uri.EscapeDataString(prefix)}&pagina=1&tamano=2");

        // assert: 2 elementos y total 3
        page1!.IsSuccess.Should().BeTrue();
        page1.Datos!.Items.Count.Should().Be(2);
        page1.Datos!.Total.Should().Be(3);

        // act: página 2 tamaño 2
        var page2 = await _client.GetFromJsonAsync<ApiResult<Paged<ClienteListItem>>>(
            $"/api/clientes?nombres={Uri.EscapeDataString(prefix)}&pagina=2&tamano=2");

        // assert: 1 elemento restante
        page2!.Datos!.Items.Count.Should().Be(1);
    }

    [Fact(DisplayName = "PUT /clientes/{id} actualiza todos los campos básicos")]
    public async Task Put_Should_Update_All_Fields()
    {
        // arrange: catálogos
        var (generoId, tipoDocId) = await SeedCatalogosAsync();
        await AuthenticateAsAdminAsync();

        // arrange: crear base
        var createReq = new
        {
            Nombres = "Maria",
            Apellidos = "Inicial",
            Edad = 20,
            GeneroId = generoId,
            TipoDocumentoIdentidad = tipoDocId,
            NumeroDocumento = "PUT-001",
            Email = "put.001@test.local",
            Password = "Secret1$"
        };
        var post = await _client.PostAsJsonAsync("/api/clientes", createReq);
        post.EnsureSuccessStatusCode();
        var created = await post.Content.ReadFromJsonAsync<ApiResult<Guid>>();

        // arrange: payload PUT
        var putReq = new
        {
            Nombres = "Maria",
            Apellidos = "Actualizada",
            Edad = 21,
            GeneroId = generoId,
            TipoDocumentoIdentidadId = tipoDocId,
            NumeroDocumento = "PUT-001",
            Direccion = "Nueva 123",
            Telefono = "555",
            Email = "put.001@test.local",
            Estado = true
        };

        // act: PUT
        var put = await _client.PutAsJsonAsync($"/api/clientes/{created!.Datos}", putReq);
        put.EnsureSuccessStatusCode();

        // assert: ver detalle actualizado
        var detail = await _client.GetFromJsonAsync<ApiResult<ClienteDto>>($"/api/clientes/{created!.Datos}");
        detail!.Datos!.Apellidos.Should().Be("Actualizada");
        detail.Datos!.Direccion.Should().Be("Nueva 123");
        detail.Datos!.Telefono.Should().Be("555");
    }

    [Fact(DisplayName = "PATCH /clientes/{id} actualiza parcialmente (Telefono, Estado)")]
    public async Task Patch_Should_Update_Partial_Fields()
    {
        // arrange: catálogos
        var (generoId, tipoDocId) = await SeedCatalogosAsync();
        await AuthenticateAsAdminAsync();

        // arrange: crear base
        var createReq = new
        {
            Nombres = "Pedro",
            Apellidos = "Patch",
            Edad = 28,
            GeneroId = generoId,
            TipoDocumentoIdentidad = tipoDocId,
            NumeroDocumento = "PATCH-001",
            Email = "patch.001@test.local",
            Password = "Secret1$"
        };
        var post = await _client.PostAsJsonAsync("/api/clientes", createReq);
        post.EnsureSuccessStatusCode();
        var created = await post.Content.ReadFromJsonAsync<ApiResult<Guid>>();

        // arrange: payload PATCH
        var patchReq = new
        {
            Telefono = "777",
            Estado = false
        };

        // act: PATCH
        var patch = await _client.PatchAsJsonAsync($"/api/clientes/{created!.Datos}", patchReq);
        patch.EnsureSuccessStatusCode();

        // assert: ver detalle actualizado
        var detail = await _client.GetFromJsonAsync<ApiResult<ClienteDto>>($"/api/clientes/{created!.Datos}");
        detail!.Datos!.Telefono.Should().Be("777");
        detail.Datos!.Estado.Should().BeFalse();
    }

    [Fact(DisplayName = "DELETE /clientes/{id} realiza soft-delete (Estado=false)")]
    public async Task Delete_Should_SoftDelete()
    {
        // arrange: catálogos
        var (generoId, tipoDocId) = await SeedCatalogosAsync();
        await AuthenticateAsAdminAsync();

        // arrange: crear base
        var createReq = new
        {
            Nombres = "Laura",
            Apellidos = "Delete",
            Edad = 26,
            GeneroId = generoId,
            TipoDocumentoIdentidad = tipoDocId,
            NumeroDocumento = "DEL-001",
            Email = "delete.001@test.local",
            Password = "Secret1$"
        };
        var post = await _client.PostAsJsonAsync("/api/clientes", createReq);
        post.EnsureSuccessStatusCode();
        var created = await post.Content.ReadFromJsonAsync<ApiResult<Guid>>();

        // act: DELETE
        var del = await _client.DeleteAsync($"/api/clientes/{created!.Datos}");
        del.EnsureSuccessStatusCode();

        // assert: detalle con Estado=false
        var detail = await _client.GetFromJsonAsync<ApiResult<ClienteDto>>($"/api/clientes/{created!.Datos}");
        detail!.Datos!.Estado.Should().BeFalse();
    }

    // Tipos para mapear respuestas simples del API
    private class ApiResult<T> { public bool IsSuccess { get; set; } public T? Datos { get; set; } public string? Error { get; set; } }
    private class Paged<T> { public List<T> Items { get; set; } = new(); public int Total { get; set; } public int Pagina { get; set; } public int Tamano { get; set; } }
    private class ClienteListItem { public Guid ClienteId { get; set; } public string Nombres { get; set; } = string.Empty; public string Apellidos { get; set; } = string.Empty; }
    private class ClienteDto { public Guid ClienteId { get; set; } public string Nombres { get; set; } = string.Empty; public string Apellidos { get; set; } = string.Empty; public bool Estado { get; set; } public string? Telefono { get; set; } public string? Direccion { get; set; } }
}
