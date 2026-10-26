namespace OmniManage.BuildingBlocks.Infrastructure.Persistence;

public sealed class PostgreSqlOptions
{
    public const string SectionName = "PostgreSql";

    public string ConnectionString { get; init; } = string.Empty;

    public int MajorVersion { get; init; } = 16;
}
