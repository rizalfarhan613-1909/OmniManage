using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OmniManage.BuildingBlocks.Infrastructure.Persistence;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Migrations;

namespace OmniManage.Modules.Hris.Infrastructure.Persistence;

public sealed class HrisDbContextFactory : IDesignTimeDbContextFactory<HrisDbContext>
{
    public HrisDbContext CreateDbContext(string[] args)
    {
        _ = args;

        var optionsBuilder = new DbContextOptionsBuilder<HrisDbContext>();
        optionsBuilder.UseOmniManagePostgreSql(
            new PostgreSqlOptions { ConnectionString = MigrationConnection.Resolve() },
            HrisDbContext.Schema);
        optionsBuilder.AddTenantIsolation(DesignTimeTenantContext.Instance);

        return new HrisDbContext(optionsBuilder.Options, DesignTimeTenantContext.Instance);
    }
}
