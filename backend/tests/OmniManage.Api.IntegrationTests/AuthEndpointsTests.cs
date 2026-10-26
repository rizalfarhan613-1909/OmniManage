using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.IdentityModel.JsonWebTokens;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.Modules.Platform.Application.Auth;
using OmniManage.Modules.Platform.Domain.Identity;

namespace OmniManage.Api.IntegrationTests;

public sealed class AuthEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokenAndSession()
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new
            {
                tenantCode = ApiFactory.TenantCode,
                email = ApiFactory.Email,
                password = ApiFactory.Password,
            },
            JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeTrue();
        payload.Error.Should().BeNull();
        payload.Data.Should().NotBeNull();
        payload.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();
        payload.Data.TokenType.Should().Be("Bearer");
        payload.Data.RefreshToken.Should().NotBeNullOrWhiteSpace();
        payload.Data.RefreshTokenExpiresAt.Should().BeAfter(payload.Data.ExpiresAt);
        payload.Data.Session.ActiveModules.Should().Contain(ModuleCodes.SalesCore);
        payload.Data.Session.ActiveModules.Should().Contain(ModuleCodes.FinGl);
        payload.Data.Session.ActiveModules.Should().Contain(ModuleCodes.FinAr);
        payload.Data.Session.Permissions.Should().Contain(Permissions.Sales.OrderRead);
        payload.Data.Session.Permissions.Should().Contain(Permissions.Finance.InvoiceRead);
        payload.Data.Session.Permissions.Should().Contain(Permissions.Finance.JournalRead);
        payload.Data.Session.User.Roles.Should().Contain(SystemRoles.TenantAdmin);

        var token = new JsonWebTokenHandler().ReadJsonWebToken(payload.Data.AccessToken);
        token.Claims.Should().Contain(claim =>
            claim.Type == "tenant_id" && claim.Value == ApiFactory.TenantId.ToString());
        token.Claims.Should().Contain(claim =>
            claim.Type == "sub" && claim.Value == ApiFactory.UserId.ToString());
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsInvalidCredentials()
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new
            {
                tenantCode = ApiFactory.TenantCode,
                email = ApiFactory.Email,
                password = "WrongPassword123!",
            },
            JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeFalse();
        payload.Data.Should().BeNull();
        payload.Error.Should().NotBeNull();
        payload.Error!.Code.Should().Be("AUTH.INVALID_CREDENTIALS");
    }

    [Fact]
    public async Task GetSession_WithIssuedToken_ReturnsActiveModulesAndPermissions()
    {
        using var client = factory.CreateClient();
        var token = await LoginAsync(client);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/v1/auth/session");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<AuthSessionDto>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeTrue();
        payload.Data.Should().NotBeNull();
        payload.Data!.Tenant.Code.Should().Be(ApiFactory.TenantCode);
        payload.Data.User.Id.Should().Be(ApiFactory.UserId);
        payload.Data.ActiveModules.Should().Contain(ModuleCodes.SalesCore);
        payload.Data.Permissions.Should().Contain(Permissions.Sales.OrderRead);
    }

    [Fact]
    public async Task GetSession_WithoutToken_ReturnsUnauthenticatedEnvelope()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/auth/session");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeFalse();
        payload.Error!.Code.Should().Be("AUTH.UNAUTHENTICATED");
    }

    private static async Task<string> LoginAsync(HttpClient client)
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

        return payload!.Data!.AccessToken;
    }
}
