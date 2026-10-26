using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.Modules.Platform.Application.Auth;

namespace OmniManage.Api.IntegrationTests.Authorization;

public sealed class PermissionAuthorizationTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Endpoint_WithGrantedPermission_ReturnsOk()
    {
        using var client = factory.CreateClient();
        var token = await LoginAsync(client);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync(new Uri("/api/v1/test/permissions/granted", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);

        payload.GetProperty("authorized").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task Endpoint_WithMissingPermission_ReturnsForbiddenEnvelope()
    {
        using var client = factory.CreateClient();
        var token = await LoginAsync(client);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync(new Uri("/api/v1/test/permissions/denied", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeFalse();
        payload.Error.Should().NotBeNull();
        payload.Error!.Code.Should().Be("AUTH.FORBIDDEN");
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
