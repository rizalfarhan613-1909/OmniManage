using System;
using Microsoft.EntityFrameworkCore.Migrations;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Migrations;

#nullable disable

namespace OmniManage.Modules.MasterData.Infrastructure.Migrations;

/// <inheritdoc />
public partial class CoreMasterData_Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "masterdata");

        migrationBuilder.CreateTable(
            name: "business_partners",
            schema: "masterdata",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                partner_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                tax_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                credit_limit = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<Guid>(type: "uuid", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_business_partners", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "inbox_messages",
            schema: "masterdata",
            columns: table => new
            {
                event_id = table.Column<Guid>(type: "uuid", nullable: false),
                consumer = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                processed_on_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_inbox_messages", x => new { x.event_id, x.consumer });
            });

        migrationBuilder.CreateTable(
            name: "items",
            schema: "masterdata",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                uom_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                base_price = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                cost_price = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                is_stock_tracked = table.Column<bool>(type: "boolean", nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<Guid>(type: "uuid", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_items", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "outbox_messages",
            schema: "masterdata",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                type = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                payload = table.Column<string>(type: "jsonb", nullable: false),
                occurred_on_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                processed_on_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                attempts = table.Column<int>(type: "integer", nullable: false),
                error = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_outbox_messages", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_business_partners_tenant_id_name",
            schema: "masterdata",
            table: "business_partners",
            columns: new[] { "tenant_id", "name" });

        migrationBuilder.CreateIndex(
            name: "uq_business_partners_tenant_id_code",
            schema: "masterdata",
            table: "business_partners",
            columns: new[] { "tenant_id", "code" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_inbox_messages_tenant_id_event_id",
            schema: "masterdata",
            table: "inbox_messages",
            columns: new[] { "tenant_id", "event_id" });

        migrationBuilder.CreateIndex(
            name: "ix_items_tenant_id_category",
            schema: "masterdata",
            table: "items",
            columns: new[] { "tenant_id", "category" });

        migrationBuilder.CreateIndex(
            name: "ix_items_tenant_id_name",
            schema: "masterdata",
            table: "items",
            columns: new[] { "tenant_id", "name" });

        migrationBuilder.CreateIndex(
            name: "uq_items_tenant_id_sku",
            schema: "masterdata",
            table: "items",
            columns: new[] { "tenant_id", "sku" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_outbox_messages_occurred_on_utc_unprocessed",
            schema: "masterdata",
            table: "outbox_messages",
            column: "occurred_on_utc",
            filter: "processed_on_utc IS NULL");

        migrationBuilder.CreateIndex(
            name: "ix_outbox_messages_tenant_id_occurred_on_utc",
            schema: "masterdata",
            table: "outbox_messages",
            columns: new[] { "tenant_id", "occurred_on_utc" });
        migrationBuilder.EnableTenantRls("masterdata", "business_partners");
        migrationBuilder.EnableTenantRls("masterdata", "inbox_messages");
        migrationBuilder.EnableTenantRls("masterdata", "items");
        migrationBuilder.EnableTenantRls("masterdata", "outbox_messages");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        throw new NotSupportedException(
            "OmniManage migrations are forward-only. Restore a backup or write a compensating migration.");
    }
}

