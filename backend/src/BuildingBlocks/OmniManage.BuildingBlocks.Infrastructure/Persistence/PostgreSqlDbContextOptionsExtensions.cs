using Microsoft.EntityFrameworkCore;

namespace OmniManage.BuildingBlocks.Infrastructure.Persistence;

public static class PostgreSqlDbContextOptionsExtensions
{
    public static DbContextOptionsBuilder UseOmniManagePostgreSql(
        this DbContextOptionsBuilder optionsBuilder,
        PostgreSqlOptions options,
        string schema)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(schema);

        optionsBuilder.UseNpgsql(
            options.ConnectionString,
            npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", schema));

        optionsBuilder.UseSnakeCaseNamingConvention();

        return optionsBuilder;
    }

    public static DbContextOptionsBuilder<TContext> UseOmniManagePostgreSql<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        PostgreSqlOptions options,
        string schema)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);

        ((DbContextOptionsBuilder)optionsBuilder).UseOmniManagePostgreSql(options, schema);

        return optionsBuilder;
    }
}
