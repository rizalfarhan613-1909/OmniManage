using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Infrastructure.Persistence;
using OmniManage.Modules.AssetMaintenance.Application;
using OmniManage.Modules.AssetMaintenance.Application.IntegrationEvents;
using OmniManage.Modules.AssetMaintenance.Domain.Abstractions;
using OmniManage.Modules.AssetMaintenance.Infrastructure.Outbox;
using OmniManage.Modules.AssetMaintenance.Infrastructure.Persistence;
using OmniManage.Modules.AssetMaintenance.Infrastructure.Persistence.Repositories;

namespace OmniManage.Modules.AssetMaintenance.Infrastructure;

public sealed class AssetMaintenanceModule : IModule
{
    private static readonly string[] PlatformHardDependencies = [ModuleCodes.PlatformCore];

    public string ModuleCode => ModuleCodes.AssetCore;

    public IReadOnlyCollection<string> HardDependencies => PlatformHardDependencies;

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDbContext<AssetMaintenanceDbContext>((serviceProvider, options) =>
        {
            var postgreSqlOptions = serviceProvider.GetRequiredService<IOptions<PostgreSqlOptions>>().Value;
            options.UseOmniManagePostgreSql(postgreSqlOptions, AssetMaintenanceDbContext.Schema);
            options.AddTenantIsolation(serviceProvider.GetRequiredService<ITenantContext>());
        });

        services.AddAssetMaintenanceApplication();

        services.AddScoped<IAssetRepository, AssetRepository>();
        services.AddScoped<IMaintenanceOrderRepository, MaintenanceOrderRepository>();
        services.AddScoped<IAssetMaintenanceOutbox, AssetMaintenanceOutbox>();
    }
}
