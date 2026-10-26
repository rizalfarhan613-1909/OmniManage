using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OmniManage.BuildingBlocks.Infrastructure.Persistence;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Migrations;

namespace OmniManage.Modules.Sales.Infrastructure.Persistence;

public sealed class SalesDbContextFactory : IDesignTimeDbContextFactory<SalesDbContext>
{
    public SalesDbContext CreateDbContext(string[] args)
    {
        _ = args;

        var optionsBuilder = new DbContextOptionsBuilder<SalesDbContext>();
        optionsBuilder.UseOmniManagePostgreSql(
            new PostgreSqlOptions { ConnectionString = MigrationConnection.Resolve() },
            SalesDbContext.Schema);
        optionsBuilder.AddTenantIsolation(DesignTimeTenantContext.Instance);

        return new SalesDbContext(optionsBuilder.Options, DesignTimeTenantContext.Instance);
    }
}
