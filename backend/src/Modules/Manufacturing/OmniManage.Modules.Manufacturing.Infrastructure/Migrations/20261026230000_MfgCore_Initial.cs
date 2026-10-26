using System;
using Microsoft.EntityFrameworkCore.Migrations;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Migrations;

#nullable disable

namespace OmniManage.Modules.Manufacturing.Infrastructure.Migrations;

/// <inheritdoc />
public partial class MfgCore_Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "manufacturing");

        migrationBuilder.CreateTable(
            name: "bill_of_materials",
            schema: "manufacturing",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                product_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                version = table.Column<int>(type: "integer", nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<Guid>(type: "uuid", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_bill_of_materials", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "work_orders",
            schema: "manufacturing",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                order_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                product_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                bill_of_material_id = table.Column<Guid>(type: "uuid", nullable: false),
                bill_of_material_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                quantity_planned = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                quantity_produced = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                due_date = table.Column<DateOnly>(type: "date", nullable: true),
                notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<Guid>(type: "uuid", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_work_orders", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "bill_of_material_components",
            schema: "manufacturing",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                bill_of_material_id = table.Column<Guid>(type: "uuid", nullable: false),
                item_id = table.Column<Guid>(type: "uuid", nullable: false),
                quantity_per_unit = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_bill_of_material_components", x => x.id);
                table.ForeignKey(
                    name: "fk_bill_of_material_components_bill_of_materials_bill_of_material_id",
                    column: x => x.bill_of_material_id,
                    principalSchema: "manufacturing",
                    principalTable: "bill_of_materials",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_bill_of_material_components_bill_of_material_id_item_id",
            schema: "manufacturing",
            table: "bill_of_material_components",
            columns: new[] { "bill_of_material_id", "item_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_bill_of_materials_tenant_id_created_at",
            schema: "manufacturing",
            table: "bill_of_materials",
            columns: new[] { "tenant_id", "created_at" });

        migrationBuilder.CreateIndex(
            name: "ix_bill_of_materials_tenant_id_is_active",
            schema: "manufacturing",
            table: "bill_of_materials",
            columns: new[] { "tenant_id", "is_active" });

        migrationBuilder.CreateIndex(
            name: "ix_bill_of_materials_tenant_id_product_item_id",
            schema: "manufacturing",
            table: "bill_of_materials",
            columns: new[] { "tenant_id", "product_item_id" });

        migrationBuilder.CreateIndex(
            name: "uq_bill_of_materials_tenant_id_code_version",
            schema: "manufacturing",
            table: "bill_of_materials",
            columns: new[] { "tenant_id", "code", "version" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_work_orders_tenant_id_bill_of_material_id",
            schema: "manufacturing",
            table: "work_orders",
            columns: new[] { "tenant_id", "bill_of_material_id" });

        migrationBuilder.CreateIndex(
            name: "ix_work_orders_tenant_id_created_at",
            schema: "manufacturing",
            table: "work_orders",
            columns: new[] { "tenant_id", "created_at" });

        migrationBuilder.CreateIndex(
            name: "ix_work_orders_tenant_id_due_date",
            schema: "manufacturing",
            table: "work_orders",
            columns: new[] { "tenant_id", "due_date" });

        migrationBuilder.CreateIndex(
            name: "ix_work_orders_tenant_id_product_item_id",
            schema: "manufacturing",
            table: "work_orders",
            columns: new[] { "tenant_id", "product_item_id" });

        migrationBuilder.CreateIndex(
            name: "ix_work_orders_tenant_id_status",
            schema: "manufacturing",
            table: "work_orders",
            columns: new[] { "tenant_id", "status" });

        migrationBuilder.CreateIndex(
            name: "uq_work_orders_tenant_id_order_number",
            schema: "manufacturing",
            table: "work_orders",
            columns: new[] { "tenant_id", "order_number" },
            unique: true);

        migrationBuilder.EnableTenantRls("manufacturing", "bill_of_materials");
        migrationBuilder.EnableTenantRls("manufacturing", "work_orders");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        throw new NotSupportedException(
            "OmniManage migrations are forward-only. Restore a backup or write a compensating migration.");
    }
}
