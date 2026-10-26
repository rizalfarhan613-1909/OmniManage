using Microsoft.EntityFrameworkCore.Migrations;

namespace OmniManage.BuildingBlocks.Infrastructure.Persistence.Migrations;

/// <summary>
/// Applies the PostgreSQL Row-Level Security policy from Architecture.md §7.5.
/// Every tenant table (a table with a <c>tenant_id</c> column) MUST call this
/// helper from its migration; policies are never hand-written ad hoc.
/// </summary>
public static class MigrationBuilderExtensions
{
    public static void EnableTenantRls(this MigrationBuilder migrationBuilder, string schema, string table)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);
        ArgumentException.ThrowIfNullOrWhiteSpace(schema);
        ArgumentException.ThrowIfNullOrWhiteSpace(table);

        var qualifiedTable = $"\"{schema}\".\"{table}\"";

        migrationBuilder.Sql($"ALTER TABLE {qualifiedTable} ENABLE ROW LEVEL SECURITY;");
        migrationBuilder.Sql($"ALTER TABLE {qualifiedTable} FORCE ROW LEVEL SECURITY;");
        migrationBuilder.Sql($"DROP POLICY IF EXISTS tenant_isolation ON {qualifiedTable};");
        migrationBuilder.Sql(
            $"CREATE POLICY tenant_isolation ON {qualifiedTable} " +
            "USING (tenant_id = current_setting('app.tenant_id', true)::uuid) " +
            "WITH CHECK (tenant_id = current_setting('app.tenant_id', true)::uuid);");
    }
}
