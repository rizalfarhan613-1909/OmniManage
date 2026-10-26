using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Infrastructure.Persistence;
using OmniManage.Modules.Manufacturing.Application;
using OmniManage.Modules.Manufacturing.Domain.Abstractions;
using OmniManage.Modules.Manufacturing.Infrastructure.Persistence;
using OmniManage.Modules.Manufacturing.Infrastructure.Persistence.Repositories;

namespace OmniManage.Modules.Manufacturing.Infrastructure;

public sealed class ManufacturingModule : IModule
{
    // MFG_CORE consumes inventory stock and material issues, so INV_CORE is a
    // hard dependency. ASSET_CORE is a soft dependency for equipment downtime.
    private static readonly string[] RequiredModules =
        [ModuleCodes.PlatformCore, ModuleCodes.InvCore];

    public string ModuleCode => ModuleCodes.MfgCore;

    public IReadOnlyCollection<string> HardDependencies => RequiredModules;

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDbContext<ManufacturingDbContext>((serviceProvider, options) =>
        {
            var postgreSqlOptions = serviceProvider.GetRequiredService<IOptions<PostgreSqlOptions>>().Value;
            options.UseOmniManagePostgreSql(postgreSqlOptions, ManufacturingDbContext.Schema);
            options.AddTenantIsolation(serviceProvider.GetRequiredService<ITenantContext>());
        });

        services.AddManufacturingApplication();

        services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();
        services.AddScoped<IBillOfMaterialRepository, BillOfMaterialRepository>();
    }
}
