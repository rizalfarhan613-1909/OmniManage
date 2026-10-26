using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OmniManage.BuildingBlocks.Infrastructure.Persistence;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Migrations;

namespace OmniManage.Modules.Procurement.Infrastructure.Persistence;

public sealed class ProcurementDbContextFactory : IDesignTimeDbContextFactory<ProcurementDbContext>
{
    public ProcurementDbContext CreateDbContext(string[] args)
    {
        _ = args;

        var optionsBuilder = new DbContextOptionsBuilder<ProcurementDbContext>();
        optionsBuilder.UseOmniManagePostgreSql(
            new PostgreSqlOptions { ConnectionString = MigrationConnection.Resolve() },
            ProcurementDbContext.Schema);
        optionsBuilder.AddTenantIsolation(DesignTimeTenantContext.Instance);

        return new ProcurementDbContext(optionsBuilder.Options, DesignTimeTenantContext.Instance);
    }
}
