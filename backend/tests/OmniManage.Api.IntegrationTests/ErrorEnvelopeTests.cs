using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.Modules.Platform.Application.Auth;

namespace OmniManage.Api.IntegrationTests;

public sealed class ErrorEnvelopeTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task UnexpectedException_ReturnsSystemUnexpectedEnvelope()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/__test/fault");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeFalse();
        payload.Data.Should().BeNull();
        payload.Error.Should().NotBeNull();
        payload.Error!.Code.Should().Be("SYSTEM.UNEXPECTED");
        payload.Error.Message.Should().NotContain("Intentional integration-test failure");
        payload.Meta.TraceId.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task MalformedJson_ReturnsValidationFailedEnvelope()
    {
        using var client = factory.CreateClient();
        using var content = new StringContent("{ not-json", Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/v1/auth/login", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeFalse();
        payload.Error.Should().NotBeNull();
        payload.Error!.Code.Should().Be("VALIDATION.FAILED");
        payload.Error.Details.Should().NotBeNullOrEmpty();
        payload.Meta.TraceId.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task InvalidBodyType_ReturnsValidationFailedEnvelope()
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { tenantCode = 123, email = true, password = new { nested = "value" } },
            JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeFalse();
        payload.Error!.Code.Should().Be("VALIDATION.FAILED");
        payload.Error.Details.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UnknownRoute_ForAuthenticatedCaller_ReturnsNotFoundEnvelope()
    {
        using var client = factory.CreateClient();

        var loginResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new
            {
                tenantCode = ApiFactory.TenantCode,
                email = ApiFactory.Email,
                password = ApiFactory.Password,
            },
            JsonOptions);
        var login = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(JsonOptions);

        login.Should().NotBeNull();
        login!.Data.Should().NotBeNull();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", login.Data!.AccessToken);

        var response = await client.GetAsync("/api/v1/this-route-does-not-exist");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeFalse();
        payload.Data.Should().BeNull();
        payload.Error!.Code.Should().Be("SYSTEM.NOT_FOUND");
        payload.Meta.TraceId.Should().NotBeNullOrWhiteSpace();
    }
}
