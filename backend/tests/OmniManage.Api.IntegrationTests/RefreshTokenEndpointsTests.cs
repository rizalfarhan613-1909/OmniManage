using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.Modules.Platform.Application.Auth;

namespace OmniManage.Api.IntegrationTests;

public sealed class RefreshTokenEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Refresh_WithActiveToken_RotatesRefreshToken()
    {
        using var client = factory.CreateClient();
        var login = await LoginAsync(client);

        var first = await RefreshAsync(client, login.RefreshToken);

        first.StatusCode.Should().Be(HttpStatusCode.OK);

        var firstPayload = await ReadAsync(first);

        firstPayload.Success.Should().BeTrue();
        firstPayload.Data.Should().NotBeNull();
        firstPayload.Data!.RefreshToken.Should().NotBe(login.RefreshToken);

        var reused = await RefreshAsync(client, login.RefreshToken);

        reused.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var reusedPayload = await ReadAsync(reused);

        reusedPayload.Success.Should().BeFalse();
        reusedPayload.Error!.Code.Should().Be("AUTH.INVALID_REFRESH_TOKEN");

        var revokedFamily = await RefreshAsync(client, firstPayload.Data.RefreshToken);

        revokedFamily.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_RevokesSession()
    {
        using var client = factory.CreateClient();
        var login = await LoginAsync(client);

        var logout = await client.PostAsJsonAsync(
            "/api/v1/auth/logout",
            new { refreshToken = login.RefreshToken },
            JsonOptions);

        logout.StatusCode.Should().Be(HttpStatusCode.OK);

        var logoutPayload = await logout.Content.ReadFromJsonAsync<ApiResponse<LogoutResponse>>(JsonOptions);

        logoutPayload.Should().NotBeNull();
        logoutPayload!.Success.Should().BeTrue();
        logoutPayload.Data!.Revoked.Should().BeTrue();

        var refresh = await RefreshAsync(client, login.RefreshToken);

        refresh.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithUnknownToken_ReturnsInvalidRefreshToken()
    {
        using var client = factory.CreateClient();

        var response = await RefreshAsync(client, "not-a-real-token");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var payload = await ReadAsync(response);

        payload.Success.Should().BeFalse();
        payload.Error!.Code.Should().Be("AUTH.INVALID_REFRESH_TOKEN");
    }

    private static async Task<LoginResponse> LoginAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new
            {
                tenantCode = ApiFactory.TenantCode,
                email = ApiFactory.Email,
                password = ApiFactory.Password,
            },
            JsonOptions);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(JsonOptions);

        return payload!.Data!;
    }

    private static Task<HttpResponseMessage> RefreshAsync(HttpClient client, string refreshToken) =>
        client.PostAsJsonAsync(
            "/api/v1/auth/refresh",
            new { refreshToken },
            JsonOptions);

    private static async Task<ApiResponse<LoginResponse>> ReadAsync(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(JsonOptions))!;
}
