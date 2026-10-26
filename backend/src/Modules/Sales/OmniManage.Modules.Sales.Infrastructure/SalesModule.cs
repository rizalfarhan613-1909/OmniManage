using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Application.IntegrationEvents;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Infrastructure.Persistence;
using OmniManage.Modules.Sales.Application;
using OmniManage.Modules.Sales.Domain.Abstractions;
using OmniManage.Modules.Sales.Infrastructure.Outbox;
using OmniManage.Modules.Sales.Infrastructure.Persistence;
using OmniManage.Modules.Sales.Infrastructure.Persistence.Repositories;

namespace OmniManage.Modules.Sales.Infrastructure;

public sealed class SalesModule : IModule
{
    private static readonly string[] PlatformHardDependencies = [ModuleCodes.PlatformCore];

    public string ModuleCode => ModuleCodes.SalesCore;

    // SALES_CORE has no licensable hard dependency. INV_CORE, FIN_AR, CRM_CORE
    // and MFG_CORE are soft dependencies: confirmation always succeeds, and an
    // inactive consumer records SKIPPED_MODULE_DISABLED in its own inbox.
    public IReadOnlyCollection<string> HardDependencies => PlatformHardDependencies;

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDbContext<SalesDbContext>((serviceProvider, options) =>
        {
            var postgreSqlOptions = serviceProvider.GetRequiredService<IOptions<PostgreSqlOptions>>().Value;
            options.UseOmniManagePostgreSql(postgreSqlOptions, SalesDbContext.Schema);
            options.AddTenantIsolation(serviceProvider.GetRequiredService<ITenantContext>());
        });

        services.AddSalesApplication();

        services.AddScoped<ISalesOrderRepository, SalesOrderRepository>();
        services.AddScoped<IIntegrationEventOutbox, SalesOutbox>();
    }
}
