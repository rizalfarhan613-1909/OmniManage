using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OmniManage.BuildingBlocks.Infrastructure.Persistence;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Migrations;

namespace OmniManage.Modules.Platform.Infrastructure.Persistence;

public sealed class PlatformDbContextFactory : IDesignTimeDbContextFactory<PlatformDbContext>
{
    public PlatformDbContext CreateDbContext(string[] args)
    {
        _ = args;

        var optionsBuilder = new DbContextOptionsBuilder<PlatformDbContext>();
        optionsBuilder.UseOmniManagePostgreSql(
            new PostgreSqlOptions { ConnectionString = MigrationConnection.Resolve() },
            PlatformDbContext.Schema);

        return new PlatformDbContext(optionsBuilder.Options);
    }
}
