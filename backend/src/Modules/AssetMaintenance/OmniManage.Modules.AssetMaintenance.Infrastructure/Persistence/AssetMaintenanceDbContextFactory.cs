using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OmniManage.BuildingBlocks.Infrastructure.Persistence;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Migrations;

namespace OmniManage.Modules.AssetMaintenance.Infrastructure.Persistence;

public sealed class AssetMaintenanceDbContextFactory : IDesignTimeDbContextFactory<AssetMaintenanceDbContext>
{
    public AssetMaintenanceDbContext CreateDbContext(string[] args)
    {
        _ = args;

        var optionsBuilder = new DbContextOptionsBuilder<AssetMaintenanceDbContext>();
        optionsBuilder.UseOmniManagePostgreSql(
            new PostgreSqlOptions { ConnectionString = MigrationConnection.Resolve() },
            AssetMaintenanceDbContext.Schema);
        optionsBuilder.AddTenantIsolation(DesignTimeTenantContext.Instance);

        return new AssetMaintenanceDbContext(optionsBuilder.Options, DesignTimeTenantContext.Instance);
    }
}
