using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Infrastructure.Persistence;
using OmniManage.Modules.Inventory.Application;
using OmniManage.Modules.Inventory.Domain.Abstractions;
using OmniManage.Modules.Inventory.Infrastructure.Persistence;
using OmniManage.Modules.Inventory.Infrastructure.Persistence.Inbox;
using OmniManage.Modules.Inventory.Infrastructure.Persistence.Repositories;

namespace OmniManage.Modules.Inventory.Infrastructure;

public sealed class InventoryModule : IModule
{
    private static readonly string[] PlatformHardDependencies = [ModuleCodes.PlatformCore];

    public string ModuleCode => ModuleCodes.InvCore;

    public IReadOnlyCollection<string> HardDependencies => PlatformHardDependencies;

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDbContext<InventoryDbContext>((serviceProvider, options) =>
        {
            var postgreSqlOptions = serviceProvider.GetRequiredService<IOptions<PostgreSqlOptions>>().Value;
            options.UseOmniManagePostgreSql(postgreSqlOptions, InventoryDbContext.Schema);
            options.AddTenantIsolation(serviceProvider.GetRequiredService<ITenantContext>());
        });

        services.AddInventoryApplication();

        services.AddScoped<IStockLedgerRepository, StockLedgerRepository>();
        services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        services.AddScoped<IInventoryInbox, InventoryInbox>();
    }
}
