using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Infrastructure.Persistence;
using OmniManage.Modules.MasterData.Application;
using OmniManage.Modules.MasterData.Domain.Abstractions;
using OmniManage.Modules.MasterData.Infrastructure.Persistence;
using OmniManage.Modules.MasterData.Infrastructure.Persistence.Repositories;

namespace OmniManage.Modules.MasterData.Infrastructure;

public sealed class MasterDataModule : IModule
{
    private static readonly string[] PlatformHardDependencies = [ModuleCodes.PlatformCore];

    public string ModuleCode => ModuleCodes.CoreMasterData;

    public IReadOnlyCollection<string> HardDependencies => PlatformHardDependencies;

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDbContext<MasterDataDbContext>((serviceProvider, options) =>
        {
            var postgreSqlOptions = serviceProvider.GetRequiredService<IOptions<PostgreSqlOptions>>().Value;
            options.UseOmniManagePostgreSql(postgreSqlOptions, MasterDataDbContext.Schema);
            options.AddTenantIsolation(serviceProvider.GetRequiredService<ITenantContext>());
        });

        services.AddMasterDataApplication();

        services.AddScoped<IBusinessPartnerRepository, BusinessPartnerRepository>();
        services.AddScoped<IItemRepository, ItemRepository>();
    }
}
