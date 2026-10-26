using System;
using Microsoft.EntityFrameworkCore.Migrations;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Migrations;

#nullable disable

namespace OmniManage.Modules.AssetMaintenance.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InitialAssetMaintenance : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "asset_maintenance");

        migrationBuilder.CreateTable(
            name: "assets",
            schema: "asset_maintenance",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                asset_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                serial_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                qr_code_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                purchase_value = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                acquisition_date = table.Column<DateOnly>(type: "date", nullable: false),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_assets", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "inbox_messages",
            schema: "asset_maintenance",
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
            schema: "asset_maintenance",
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
            name: "maintenance_orders",
            schema: "asset_maintenance",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                asset_id = table.Column<Guid>(type: "uuid", nullable: false),
                request_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                scheduled_date = table.Column<DateOnly>(type: "date", nullable: true),
                completed_date = table.Column<DateOnly>(type: "date", nullable: true),
                cost = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_maintenance_orders", x => x.id);
                table.ForeignKey(
                    name: "fk_maintenance_orders_assets_asset_id",
                    column: x => x.asset_id,
                    principalSchema: "asset_maintenance",
                    principalTable: "assets",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "ix_assets_tenant_id_category",
            schema: "asset_maintenance",
            table: "assets",
            columns: new[] { "tenant_id", "category" });

        migrationBuilder.CreateIndex(
            name: "ix_assets_tenant_id_serial_number",
            schema: "asset_maintenance",
            table: "assets",
            columns: new[] { "tenant_id", "serial_number" });

        migrationBuilder.CreateIndex(
            name: "ix_assets_tenant_id_status",
            schema: "asset_maintenance",
            table: "assets",
            columns: new[] { "tenant_id", "status" });

        migrationBuilder.CreateIndex(
            name: "uq_assets_tenant_id_asset_code",
            schema: "asset_maintenance",
            table: "assets",
            columns: new[] { "tenant_id", "asset_code" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_inbox_messages_tenant_id_event_id",
            schema: "asset_maintenance",
            table: "inbox_messages",
            columns: new[] { "tenant_id", "event_id" });

        migrationBuilder.CreateIndex(
            name: "ix_maintenance_orders_asset_id",
            schema: "asset_maintenance",
            table: "maintenance_orders",
            column: "asset_id");

        migrationBuilder.CreateIndex(
            name: "ix_maintenance_orders_tenant_id_asset_id_status",
            schema: "asset_maintenance",
            table: "maintenance_orders",
            columns: new[] { "tenant_id", "asset_id", "status" });

        migrationBuilder.CreateIndex(
            name: "ix_maintenance_orders_tenant_id_scheduled_date",
            schema: "asset_maintenance",
            table: "maintenance_orders",
            columns: new[] { "tenant_id", "scheduled_date" });

        migrationBuilder.CreateIndex(
            name: "uq_maintenance_orders_tenant_id_request_number",
            schema: "asset_maintenance",
            table: "maintenance_orders",
            columns: new[] { "tenant_id", "request_number" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_outbox_messages_occurred_on_utc_unprocessed",
            schema: "asset_maintenance",
            table: "outbox_messages",
            column: "occurred_on_utc",
            filter: "processed_on_utc IS NULL");

        migrationBuilder.CreateIndex(
            name: "ix_outbox_messages_tenant_id_occurred_on_utc",
            schema: "asset_maintenance",
            table: "outbox_messages",
            columns: new[] { "tenant_id", "occurred_on_utc" });

        migrationBuilder.EnableTenantRls("asset_maintenance", "assets");
        migrationBuilder.EnableTenantRls("asset_maintenance", "maintenance_orders");
        migrationBuilder.EnableTenantRls("asset_maintenance", "inbox_messages");
        migrationBuilder.EnableTenantRls("asset_maintenance", "outbox_messages");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        throw new NotSupportedException(
            "OmniManage migrations are forward-only. Restore a backup or write a compensating migration.");
    }
}
