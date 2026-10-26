using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OmniManage.Api.Authentication;
using OmniManage.Api.Middleware;
using OmniManage.Api.Seeding;
using OmniManage.Api.Tenancy;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Application.Auditing;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.BuildingBlocks.Infrastructure;
using OmniManage.BuildingBlocks.Infrastructure.IntegrationEvents;
using OmniManage.BuildingBlocks.Infrastructure.Persistence;
using OmniManage.Modules.AssetMaintenance.Infrastructure;
using OmniManage.Modules.AssetMaintenance.Presentation;
using OmniManage.Modules.Finance.Infrastructure;
using OmniManage.Modules.Finance.Presentation;
using OmniManage.Modules.Hris.Infrastructure;
using OmniManage.Modules.Hris.Presentation;
using OmniManage.Modules.Inventory.Infrastructure;
using OmniManage.Modules.Inventory.Presentation;
using OmniManage.Modules.Manufacturing.Infrastructure;
using OmniManage.Modules.Manufacturing.Presentation;
using OmniManage.Modules.MasterData.Infrastructure;
using OmniManage.Modules.MasterData.Presentation;
using OmniManage.Modules.Platform.Application.Auth;
using OmniManage.Modules.Platform.Infrastructure;
using OmniManage.Modules.Platform.Infrastructure.Auth;
using OmniManage.Modules.Platform.Presentation;
using OmniManage.Modules.Procurement.Infrastructure;
using OmniManage.Modules.Procurement.Presentation;
using OmniManage.Modules.Projects.Infrastructure;
using OmniManage.Modules.Projects.Presentation;
using OmniManage.Modules.Sales.Infrastructure;
using OmniManage.Modules.Sales.Presentation;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var usesEphemeralJwtSigningKey = false;

if (builder.Environment.IsDevelopment() &&
    string.IsNullOrWhiteSpace(builder.Configuration["Jwt:PrivateKeyPem"]))
{
    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["Jwt:PrivateKeyPem"] = DevelopmentSigningKey.CreatePrivateKeyPem(),
    });

    usesEphemeralJwtSigningKey = true;
}

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

builder.Services
    .AddOptions<PostgreSqlOptions>()
    .Bind(builder.Configuration.GetSection(PostgreSqlOptions.SectionName))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.ConnectionString),
        "PostgreSql:ConnectionString must be configured.")
    .Validate(
        options => options.MajorVersion == 16,
        "PostgreSql:MajorVersion must be 16.")
    .ValidateOnStart();

builder.Services
    .AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.Issuer),
        "Jwt:Issuer must be configured.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.Audience),
        "Jwt:Audience must be configured.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.KeyId),
        "Jwt:KeyId must be configured.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.PrivateKeyPem),
        "Jwt:PrivateKeyPem must be configured with a PEM-encoded RSA private key.")
    .Validate(
        options => JwtSigningKey.IsValidPrivateKeyPem(options.PrivateKeyPem),
        "Jwt:PrivateKeyPem must be a valid PEM-encoded RSA private key.")
    .Validate(
        options => options.AccessTokenMinutes is > 0 and <= 15,
        "Jwt:AccessTokenMinutes must be between 1 and 15.")
    .ValidateOnStart();

builder.Services
    .AddOptions<RefreshTokenOptions>()
    .Bind(builder.Configuration.GetSection(RefreshTokenOptions.SectionName))
    .Validate(
        options => options.LifetimeDays is > 0 and <= 30,
        "RefreshTokens:LifetimeDays must be between 1 and 30.")
    .ValidateOnStart();

builder.Services
    .AddOptions<SeedingOptions>()
    .Bind(builder.Configuration.GetSection(SeedingOptions.SectionName))
    .Validate(
        options => !options.Enabled || !string.IsNullOrWhiteSpace(options.SuperadminPassword),
        "Seeding:SuperadminPassword must be configured when seeding is enabled.")
    .Validate(
        options => !options.Enabled || !string.IsNullOrWhiteSpace(options.DemoAdminPassword),
        "Seeding:DemoAdminPassword must be configured when seeding is enabled.")
    .Validate(
        options => !options.Enabled || !string.IsNullOrWhiteSpace(options.DemoUserPassword),
        "Seeding:DemoUserPassword must be configured when seeding is enabled.")
    .Validate(
        options => !options.Enabled || !string.IsNullOrWhiteSpace(options.DemoTenantCode),
        "Seeding:DemoTenantCode must be configured when seeding is enabled.")
    .ValidateOnStart();

builder.Services.AddSingleton(serviceProvider =>
    serviceProvider.GetRequiredService<IOptions<RefreshTokenOptions>>().Value);

builder.Services.AddScoped<DbSeeder>();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, HttpTenantContext>();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddScoped<IAuditContext, HttpAuditContext>();
builder.Services.AddSingleton<IApiResponseWriter, ApiResponseWriter>();

builder.Services
    .AddControllers()
    .AddApplicationPart(PlatformPresentationAssembly.Value)
    .AddApplicationPart(MasterDataPresentationAssembly.Value)
    .AddApplicationPart(FinancePresentationAssembly.Value)
    .AddApplicationPart(InventoryPresentationAssembly.Value)
    .AddApplicationPart(ManufacturingPresentationAssembly.Value)
    .AddApplicationPart(SalesPresentationAssembly.Value)
    .AddApplicationPart(ProcurementPresentationAssembly.Value)
    .AddApplicationPart(HrisPresentationAssembly.Value)
    .AddApplicationPart(ProjectsPresentationAssembly.Value)
    .AddApplicationPart(AssetMaintenancePresentationAssembly.Value);
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var clock = context.HttpContext.RequestServices.GetRequiredService<TimeProvider>();
        var details = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .SelectMany(entry => entry.Value!.Errors.Select(_ => new ApiErrorDetail(
                string.IsNullOrWhiteSpace(entry.Key) ? null : entry.Key,
                CommonErrors.ValidationFailed.Code,
                "The supplied value is invalid.")))
            .ToArray();
        var meta = new ApiMeta(
            Activity.Current?.Id ?? context.HttpContext.TraceIdentifier,
            clock.GetUtcNow().UtcDateTime);

        return new BadRequestObjectResult(
            ApiResponse<object>.Fail(
                new ApiError(
                    CommonErrors.ValidationFailed.Code,
                    CommonErrors.ValidationFailed.Message,
                    details),
                meta));
    };
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

IModule[] modules =
[
    new PlatformModule(),
    new MasterDataModule(),
    new FinanceModule(),
    new SalesModule(),
    new InventoryModule(),
    new ManufacturingModule(),
    new ProcurementModule(),
    new HrisModule(),
    new ProjectsModule(),
    new AssetMaintenanceModule(),
];

foreach (var module in modules)
{
    module.RegisterServices(builder.Services, builder.Configuration);
}

builder.Services.AddOmniManageInfrastructure(builder.Configuration);

builder.Services.AddIntegrationEventDispatcher();

builder.Services.AddPlatformAuthentication();

var app = builder.Build();

if (usesEphemeralJwtSigningKey)
{
    app.Logger.LogWarning(
        "Jwt:PrivateKeyPem is not configured. Using an ephemeral development signing key; " +
        "tokens issued before a restart will no longer validate.");
}

if (app.Configuration.GetValue<bool>($"{SeedingOptions.SectionName}:Enabled"))
{
    await using var seedingScope = app.Services.CreateAsyncScope();
    var seeder = seedingScope.ServiceProvider.GetRequiredService<DbSeeder>();
    await seeder.SeedAsync().ConfigureAwait(false);
}

app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseStatusCodePages(async statusCodeContext =>
{
    var context = statusCodeContext.HttpContext;

    if (context.Response.StatusCode != StatusCodes.Status404NotFound)
    {
        return;
    }

    var error = CommonErrors.NotFound;

    var writer = context.RequestServices.GetRequiredService<IApiResponseWriter>();
    await writer.WriteErrorAsync(
        context,
        context.Response.StatusCode,
        error.Code,
        error.Message);
});
app.UseRouting();
app.UseAuthentication();
app.UseMiddleware<ModuleLicenseMiddleware>();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health/live").AllowAnonymous();
app.MapControllers();

await app.RunAsync().ConfigureAwait(false);

public partial class Program
{
}
