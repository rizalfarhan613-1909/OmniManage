using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Application.Auditing;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Infrastructure;
using OmniManage.BuildingBlocks.Infrastructure.Persistence;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Audit;
using OmniManage.Modules.Platform.Application;
using OmniManage.Modules.Platform.Application.Auth;
using OmniManage.Modules.Platform.Domain.Abstractions;
using OmniManage.Modules.Platform.Infrastructure.Auditing;
using OmniManage.Modules.Platform.Infrastructure.Auth;
using OmniManage.Modules.Platform.Infrastructure.Licensing;
using OmniManage.Modules.Platform.Infrastructure.Persistence;
using OmniManage.Modules.Platform.Infrastructure.Persistence.Repositories;

namespace OmniManage.Modules.Platform.Infrastructure;

public sealed class PlatformModule : IModule
{
    private static readonly string[] NoHardDependencies = [];

    public string ModuleCode => ModuleCodes.PlatformCore;

    public IReadOnlyCollection<string> HardDependencies => NoHardDependencies;

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDbContext<PlatformDbContext>((serviceProvider, options) =>
        {
            var postgreSqlOptions = serviceProvider.GetRequiredService<IOptions<PostgreSqlOptions>>().Value;
            options.UseOmniManagePostgreSql(postgreSqlOptions, PlatformDbContext.Schema);
            options.AddInterceptors(serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>());
        });

        services.AddPlatformApplication();

        services.AddMemoryCache();

        services.AddSingleton<IGuidGenerator, GuidGenerator>();
        services.AddSingleton<TimeProvider>(TimeProvider.System);
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ITenantModuleRepository, TenantModuleRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<LicensingService>();
        services.AddScoped<ITenantModuleCache>(serviceProvider =>
            serviceProvider.GetRequiredService<LicensingService>());
        services.AddScoped<IModuleLicenseChecker>(serviceProvider =>
            serviceProvider.GetRequiredService<LicensingService>());

        services.AddScoped<AuditSaveChangesInterceptor>();
        services.AddScoped<IAuditLogger, AuditLogger>();

        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
        services.AddSingleton<JwtSigningKey>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IRefreshTokenGenerator, RefreshTokenGenerator>();
    }
}
