using System;
using Microsoft.EntityFrameworkCore.Migrations;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Migrations;

#nullable disable

namespace OmniManage.Modules.Inventory.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InvCore_Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "inventory");

        migrationBuilder.CreateTable(
            name: "inbox_messages",
            schema: "inventory",
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
            name: "outbox_messages",
            schema: "inventory",
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

        migrationBuilder.CreateTable(
            name: "stock_ledger",
            schema: "inventory",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                warehouse_id = table.Column<Guid>(type: "uuid", nullable: false),
                item_id = table.Column<Guid>(type: "uuid", nullable: false),
                quantity_delta = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                movement_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                reference_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                occurred_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_stock_ledger", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "warehouses",
            schema: "inventory",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<Guid>(type: "uuid", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_warehouses", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_inbox_messages_tenant_id_event_id",
            schema: "inventory",
            table: "inbox_messages",
            columns: new[] { "tenant_id", "event_id" });

        migrationBuilder.CreateIndex(
            name: "ix_outbox_messages_occurred_on_utc_unprocessed",
            schema: "inventory",
            table: "outbox_messages",
            column: "occurred_on_utc",
            filter: "processed_on_utc IS NULL");

        migrationBuilder.CreateIndex(
            name: "ix_outbox_messages_tenant_id_occurred_on_utc",
            schema: "inventory",
            table: "outbox_messages",
            columns: new[] { "tenant_id", "occurred_on_utc" });

        migrationBuilder.CreateIndex(
            name: "ix_stock_ledger_tenant_id_occurred_at_utc",
            schema: "inventory",
            table: "stock_ledger",
            columns: new[] { "tenant_id", "occurred_at_utc" });

        migrationBuilder.CreateIndex(
            name: "ix_stock_ledger_tenant_id_warehouse_id_item_id",
            schema: "inventory",
            table: "stock_ledger",
            columns: new[] { "tenant_id", "warehouse_id", "item_id" });

        migrationBuilder.CreateIndex(
            name: "ix_warehouses_tenant_id_name",
            schema: "inventory",
            table: "warehouses",
            columns: new[] { "tenant_id", "name" });

        migrationBuilder.CreateIndex(
            name: "uq_warehouses_tenant_id_code",
            schema: "inventory",
            table: "warehouses",
            columns: new[] { "tenant_id", "code" },
            unique: true);
        migrationBuilder.EnableTenantRls("inventory", "inbox_messages");
        migrationBuilder.EnableTenantRls("inventory", "outbox_messages");
        migrationBuilder.EnableTenantRls("inventory", "stock_ledger");
        migrationBuilder.EnableTenantRls("inventory", "warehouses");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        throw new NotSupportedException(
            "OmniManage migrations are forward-only. Restore a backup or write a compensating migration.");
    }
}

