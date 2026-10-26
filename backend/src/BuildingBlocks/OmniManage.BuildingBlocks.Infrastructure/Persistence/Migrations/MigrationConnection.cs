namespace OmniManage.BuildingBlocks.Infrastructure.Persistence.Migrations;

/// <summary>
/// Resolves the privileged connection used only by <c>dotnet ef</c>. Runtime
/// connections never use this value; they use <c>PostgreSql:ConnectionString</c>
/// with the non-owner <c>omni_app</c> role (Architecture.md §7.5).
/// </summary>
public static class MigrationConnection
{
    public const string EnvironmentVariable = "OMNIMANAGE_MIGRATION_CONNECTION";

    private const string LocalDefault =
        "Host=localhost;Port=5433;Database=omnimanage_db;Username=omni_migrator;SslMode=Prefer;Include Error Detail=false";

    public static string Resolve()
    {
        var configured = Environment.GetEnvironmentVariable(EnvironmentVariable);

        return string.IsNullOrWhiteSpace(configured)
            ? LocalDefault
            : configured;
    }
}
