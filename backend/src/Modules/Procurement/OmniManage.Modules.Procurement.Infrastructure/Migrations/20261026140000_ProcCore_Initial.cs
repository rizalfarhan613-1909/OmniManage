using System;
using Microsoft.EntityFrameworkCore.Migrations;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Migrations;

#nullable disable

namespace OmniManage.Modules.Procurement.Infrastructure.Migrations;

/// <inheritdoc />
public partial class ProcCore_Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "procurement");

        migrationBuilder.CreateTable(
            name: "suppliers",
            schema: "procurement",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                contact_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                payment_terms_days = table.Column<int>(type: "integer", nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<Guid>(type: "uuid", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_suppliers", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "purchase_orders",
            schema: "procurement",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                order_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                supplier_id = table.Column<Guid>(type: "uuid", nullable: false),
                supplier_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                total_amount = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                expected_delivery_date = table.Column<DateOnly>(type: "date", nullable: true),
                notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<Guid>(type: "uuid", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_purchase_orders", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "purchase_order_lines",
            schema: "procurement",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                purchase_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                item_id = table.Column<Guid>(type: "uuid", nullable: false),
                quantity = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                unit_price = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                line_total = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_purchase_order_lines", x => x.id);
                table.ForeignKey(
                    name: "fk_purchase_order_lines_purchase_orders_purchase_order_id",
                    column: x => x.purchase_order_id,
                    principalSchema: "procurement",
                    principalTable: "purchase_orders",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_purchase_order_lines_purchase_order_id_item_id",
            schema: "procurement",
            table: "purchase_order_lines",
            columns: new[] { "purchase_order_id", "item_id" });

        migrationBuilder.CreateIndex(
            name: "ix_purchase_orders_tenant_id_created_at",
            schema: "procurement",
            table: "purchase_orders",
            columns: new[] { "tenant_id", "created_at" });

        migrationBuilder.CreateIndex(
            name: "ix_purchase_orders_tenant_id_expected_delivery_date",
            schema: "procurement",
            table: "purchase_orders",
            columns: new[] { "tenant_id", "expected_delivery_date" });

        migrationBuilder.CreateIndex(
            name: "ix_purchase_orders_tenant_id_status",
            schema: "procurement",
            table: "purchase_orders",
            columns: new[] { "tenant_id", "status" });

        migrationBuilder.CreateIndex(
            name: "ix_purchase_orders_tenant_id_supplier_id",
            schema: "procurement",
            table: "purchase_orders",
            columns: new[] { "tenant_id", "supplier_id" });

        migrationBuilder.CreateIndex(
            name: "uq_purchase_orders_tenant_id_order_number",
            schema: "procurement",
            table: "purchase_orders",
            columns: new[] { "tenant_id", "order_number" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_suppliers_tenant_id_is_active",
            schema: "procurement",
            table: "suppliers",
            columns: new[] { "tenant_id", "is_active" });

        migrationBuilder.CreateIndex(
            name: "ix_suppliers_tenant_id_name",
            schema: "procurement",
            table: "suppliers",
            columns: new[] { "tenant_id", "name" });

        migrationBuilder.CreateIndex(
            name: "uq_suppliers_tenant_id_code",
            schema: "procurement",
            table: "suppliers",
            columns: new[] { "tenant_id", "code" },
            unique: true);
        migrationBuilder.EnableTenantRls("procurement", "purchase_orders");
        migrationBuilder.EnableTenantRls("procurement", "suppliers");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        throw new NotSupportedException(
            "OmniManage migrations are forward-only. Restore a backup or write a compensating migration.");
    }
}
