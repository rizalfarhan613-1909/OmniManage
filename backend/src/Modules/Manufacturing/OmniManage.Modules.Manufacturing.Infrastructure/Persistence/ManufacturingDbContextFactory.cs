using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OmniManage.BuildingBlocks.Infrastructure.Persistence;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Migrations;

namespace OmniManage.Modules.Manufacturing.Infrastructure.Persistence;

public sealed class ManufacturingDbContextFactory : IDesignTimeDbContextFactory<ManufacturingDbContext>
{
    public ManufacturingDbContext CreateDbContext(string[] args)
    {
        _ = args;

        var optionsBuilder = new DbContextOptionsBuilder<ManufacturingDbContext>();
        optionsBuilder.UseOmniManagePostgreSql(
            new PostgreSqlOptions { ConnectionString = MigrationConnection.Resolve() },
            ManufacturingDbContext.Schema);
        optionsBuilder.AddTenantIsolation(DesignTimeTenantContext.Instance);

        return new ManufacturingDbContext(optionsBuilder.Options, DesignTimeTenantContext.Instance);
    }
}
