using System;
using Microsoft.EntityFrameworkCore.Migrations;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Migrations;

#nullable disable

namespace OmniManage.Modules.Sales.Infrastructure.Migrations;

/// <inheritdoc />
public partial class SalesCore_Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "sales");

        migrationBuilder.CreateTable(
            name: "inbox_messages",
            schema: "sales",
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
            schema: "sales",
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
            name: "sales_orders",
            schema: "sales",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                order_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                total_amount = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<Guid>(type: "uuid", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_sales_orders", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "sales_order_lines",
            schema: "sales",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                sales_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                item_id = table.Column<Guid>(type: "uuid", nullable: false),
                quantity = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                unit_price = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                line_total = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_sales_order_lines", x => x.id);
                table.ForeignKey(
                    name: "fk_sales_order_lines_sales_orders_sales_order_id",
                    column: x => x.sales_order_id,
                    principalSchema: "sales",
                    principalTable: "sales_orders",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_inbox_messages_tenant_id_event_id",
            schema: "sales",
            table: "inbox_messages",
            columns: new[] { "tenant_id", "event_id" });

        migrationBuilder.CreateIndex(
            name: "ix_outbox_messages_occurred_on_utc_unprocessed",
            schema: "sales",
            table: "outbox_messages",
            column: "occurred_on_utc",
            filter: "processed_on_utc IS NULL");

        migrationBuilder.CreateIndex(
            name: "ix_outbox_messages_tenant_id_occurred_on_utc",
            schema: "sales",
            table: "outbox_messages",
            columns: new[] { "tenant_id", "occurred_on_utc" });

        migrationBuilder.CreateIndex(
            name: "ix_sales_order_lines_sales_order_id_item_id",
            schema: "sales",
            table: "sales_order_lines",
            columns: new[] { "sales_order_id", "item_id" });

        migrationBuilder.CreateIndex(
            name: "ix_sales_orders_tenant_id_created_at",
            schema: "sales",
            table: "sales_orders",
            columns: new[] { "tenant_id", "created_at" });

        migrationBuilder.CreateIndex(
            name: "ix_sales_orders_tenant_id_status",
            schema: "sales",
            table: "sales_orders",
            columns: new[] { "tenant_id", "status" });

        migrationBuilder.CreateIndex(
            name: "uq_sales_orders_tenant_id_order_number",
            schema: "sales",
            table: "sales_orders",
            columns: new[] { "tenant_id", "order_number" },
            unique: true);
        migrationBuilder.EnableTenantRls("sales", "inbox_messages");
        migrationBuilder.EnableTenantRls("sales", "outbox_messages");
        migrationBuilder.EnableTenantRls("sales", "sales_orders");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        throw new NotSupportedException(
            "OmniManage migrations are forward-only. Restore a backup or write a compensating migration.");
    }
}

