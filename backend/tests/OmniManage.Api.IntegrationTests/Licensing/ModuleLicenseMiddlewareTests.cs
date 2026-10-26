using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using OmniManage.Api.Middleware;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Contracts;

namespace OmniManage.Api.IntegrationTests.Licensing;

public sealed class ModuleLicenseMiddlewareTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Invoke_WhenRequiredModuleIsActive_ReturnsOk()
    {
        await using var app = await StartHostAsync(ModuleCodes.SalesCore);
        using var client = CreateClient(app);

        var response = await client.GetAsync(new Uri("/api/v1/test/licensing", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Invoke_WhenRequiredModuleIsInactive_ReturnsModuleNotLicensed()
    {
        await using var app = await StartHostAsync();
        using var client = CreateClient(app);

        var response = await client.GetAsync(new Uri("/api/v1/test/licensing", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

        payload.Should().NotBeNull();
        payload!.Success.Should().BeFalse();
        payload.Error.Should().NotBeNull();
        payload.Error!.Code.Should().Be("PLATFORM.MODULE_NOT_LICENSED");
        payload.Error.Details.Should().ContainSingle()
            .Which.Field.Should().Be("moduleCode");
    }

    [Fact]
    public async Task Invoke_WhenEndpointAllowsWithoutModule_ReturnsOk()
    {
        await using var app = await StartHostAsync();
        using var client = CreateClient(app);

        var response = await client.GetAsync(new Uri("/api/v1/test/licensing/open", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static HttpClient CreateClient(WebApplication app) =>
        app.GetTestClient();

    private static async Task<WebApplication> StartHostAsync(params string[] activeModules)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSingleton<ITenantContext>(new FakeTenantContext(TestIds.TenantId));
        builder.Services.AddSingleton<ITenantModuleCache>(new FakeTenantModuleCache(activeModules));
        builder.Services.AddSingleton<IApiResponseWriter, ApiResponseWriter>();
        builder.Services
            .AddAuthentication(TestAuthHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.SchemeName,
                _ => { });
        builder.Services.AddAuthorization();
        builder.Services
            .AddControllers()
            .AddApplicationPart(typeof(ModuleLicenseTestController).Assembly);

        var app = builder.Build();

        app.UseRouting();
        app.UseAuthentication();
        app.UseMiddleware<ModuleLicenseMiddleware>();
        app.UseAuthorization();
        app.MapControllers();

        await app.StartAsync();

        return app;
    }
}
