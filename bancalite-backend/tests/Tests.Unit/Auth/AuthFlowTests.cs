using System.Net.Http.Json;
using FluentAssertions;

namespace Tests.Unit.Auth;

// Tests del flujo completo de autenticación: login, me, logout, refresh
public class AuthFlowTests : IClassFixture<CustomWebApiFactory>
{
    private readonly CustomWebApiFactory _factory;
    private readonly HttpClient _client;

    public AuthFlowTests(CustomWebApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact(DisplayName = "Login -> Me: retorna perfil con token válido")]
    public async Task Login_Then_Me_Should_Return_Profile()
    {
        var login = new { email = "admin@test.local", password = "Admin123$" };
        var loginResp = await _client.PostAsJsonAsync("/api/auth/login", login);
        loginResp.EnsureSuccessStatusCode();
        var profile = await loginResp.Content.ReadFromJsonAsync<ProfileDto>();
        profile.Should().NotBeNull();
        profile!.token.Should().NotBeNullOrEmpty();

        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", profile.token);
        var meResp = await _client.GetAsync("/api/auth/me");
        meResp.IsSuccessStatusCode.Should().BeTrue();
    }

    // Logout invalida access token inmediatamente
    [Fact(DisplayName = "Logout revoca access token inmediatamente (401 en /me)")]
    public async Task Logout_Should_Invalidate_AccessToken()
    {
        var login = new { email = "admin@test.local", password = "Admin123$" };
        var loginResp = await _client.PostAsJsonAsync("/api/auth/login", login);
        loginResp.EnsureSuccessStatusCode();
        var profile = await loginResp.Content.ReadFromJsonAsync<ProfileDto>();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", profile!.token);

        var meOk = await _client.GetAsync("/api/auth/me");
        meOk.IsSuccessStatusCode.Should().BeTrue();

        var logoutResp = await _client.PostAsJsonAsync("/api/auth/logout", new { refreshToken = profile.refreshToken });
        logoutResp.EnsureSuccessStatusCode();

        var meUnauthorized = await _client.GetAsync("/api/auth/me");
        meUnauthorized.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    // Refresh rota refresh token y entrega nuevo access token
    [Fact(DisplayName = "Refresh rota refresh token y entrega nuevo access token")]
    public async Task Refresh_Should_Rotate_And_Return_New_Tokens()
    {
        var login = new { email = "admin@test.local", password = "Admin123$" };
        var loginResp = await _client.PostAsJsonAsync("/api/auth/login", login);
        loginResp.EnsureSuccessStatusCode();
        var profile = await loginResp.Content.ReadFromJsonAsync<ProfileDto>();

        var refreshResp = await _client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = profile!.refreshToken });
        refreshResp.EnsureSuccessStatusCode();
        var profile2 = await refreshResp.Content.ReadFromJsonAsync<ProfileDto>();

        profile2!.token.Should().NotBeNullOrEmpty().And.NotBe(profile.token);
        profile2.refreshToken.Should().NotBe(profile.refreshToken);
    }

    private record ProfileDto(string? nombreCompleto, string? email, string? token, string? refreshToken);
}
