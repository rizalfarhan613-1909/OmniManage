using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Infrastructure.Persistence;
using OmniManage.Modules.Procurement.Application;
using OmniManage.Modules.Procurement.Domain.Abstractions;
using OmniManage.Modules.Procurement.Infrastructure.Persistence;
using OmniManage.Modules.Procurement.Infrastructure.Persistence.Repositories;

namespace OmniManage.Modules.Procurement.Infrastructure;

public sealed class ProcurementModule : IModule
{
    private static readonly string[] PlatformHardDependencies = [ModuleCodes.PlatformCore];

    public string ModuleCode => ModuleCodes.ProcCore;

    public IReadOnlyCollection<string> HardDependencies => PlatformHardDependencies;

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDbContext<ProcurementDbContext>((serviceProvider, options) =>
        {
            var postgreSqlOptions = serviceProvider.GetRequiredService<IOptions<PostgreSqlOptions>>().Value;
            options.UseOmniManagePostgreSql(postgreSqlOptions, ProcurementDbContext.Schema);
            options.AddTenantIsolation(serviceProvider.GetRequiredService<ITenantContext>());
        });

        services.AddProcurementApplication();

        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
    }
}
