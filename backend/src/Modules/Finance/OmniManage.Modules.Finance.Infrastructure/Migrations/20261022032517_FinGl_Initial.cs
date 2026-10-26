using System;
using Microsoft.EntityFrameworkCore.Migrations;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Migrations;

#nullable disable

namespace OmniManage.Modules.Finance.Infrastructure.Migrations;

/// <inheritdoc />
public partial class FinGl_Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "finance");

        migrationBuilder.CreateTable(
            name: "ar_invoices",
            schema: "finance",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                invoice_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                sales_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                total_amount = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<Guid>(type: "uuid", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_ar_invoices", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "gl_accounts",
            schema: "finance",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                account_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                account_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<Guid>(type: "uuid", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_gl_accounts", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "inbox_messages",
            schema: "finance",
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
            name: "journal_entries",
            schema: "finance",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                entry_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                posted_date_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<Guid>(type: "uuid", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_journal_entries", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "outbox_messages",
            schema: "finance",
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
            name: "journal_entry_lines",
            schema: "finance",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                journal_entry_id = table.Column<Guid>(type: "uuid", nullable: false),
                gl_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                debit = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                credit = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_journal_entry_lines", x => x.id);
                table.ForeignKey(
                    name: "fk_journal_entry_lines_journal_entries_journal_entry_id",
                    column: x => x.journal_entry_id,
                    principalSchema: "finance",
                    principalTable: "journal_entries",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_ar_invoices_tenant_id_created_at",
            schema: "finance",
            table: "ar_invoices",
            columns: new[] { "tenant_id", "created_at" });

        migrationBuilder.CreateIndex(
            name: "ix_ar_invoices_tenant_id_customer_id",
            schema: "finance",
            table: "ar_invoices",
            columns: new[] { "tenant_id", "customer_id" });

        migrationBuilder.CreateIndex(
            name: "ix_ar_invoices_tenant_id_status",
            schema: "finance",
            table: "ar_invoices",
            columns: new[] { "tenant_id", "status" });

        migrationBuilder.CreateIndex(
            name: "uq_ar_invoices_tenant_id_invoice_number",
            schema: "finance",
            table: "ar_invoices",
            columns: new[] { "tenant_id", "invoice_number" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "uq_ar_invoices_tenant_id_sales_order_id",
            schema: "finance",
            table: "ar_invoices",
            columns: new[] { "tenant_id", "sales_order_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_gl_accounts_tenant_id_type",
            schema: "finance",
            table: "gl_accounts",
            columns: new[] { "tenant_id", "type" });

        migrationBuilder.CreateIndex(
            name: "uq_gl_accounts_tenant_id_account_code",
            schema: "finance",
            table: "gl_accounts",
            columns: new[] { "tenant_id", "account_code" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_inbox_messages_tenant_id_event_id",
            schema: "finance",
            table: "inbox_messages",
            columns: new[] { "tenant_id", "event_id" });

        migrationBuilder.CreateIndex(
            name: "ix_journal_entries_tenant_id_posted_date_utc",
            schema: "finance",
            table: "journal_entries",
            columns: new[] { "tenant_id", "posted_date_utc" });

        migrationBuilder.CreateIndex(
            name: "uq_journal_entries_tenant_id_entry_number",
            schema: "finance",
            table: "journal_entries",
            columns: new[] { "tenant_id", "entry_number" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_journal_entry_lines_journal_entry_id_gl_account_id",
            schema: "finance",
            table: "journal_entry_lines",
            columns: new[] { "journal_entry_id", "gl_account_id" });

        migrationBuilder.CreateIndex(
            name: "ix_outbox_messages_occurred_on_utc_unprocessed",
            schema: "finance",
            table: "outbox_messages",
            column: "occurred_on_utc",
            filter: "processed_on_utc IS NULL");

        migrationBuilder.CreateIndex(
            name: "ix_outbox_messages_tenant_id_occurred_on_utc",
            schema: "finance",
            table: "outbox_messages",
            columns: new[] { "tenant_id", "occurred_on_utc" });
        migrationBuilder.EnableTenantRls("finance", "ar_invoices");
        migrationBuilder.EnableTenantRls("finance", "gl_accounts");
        migrationBuilder.EnableTenantRls("finance", "inbox_messages");
        migrationBuilder.EnableTenantRls("finance", "journal_entries");
        migrationBuilder.EnableTenantRls("finance", "outbox_messages");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        throw new NotSupportedException(
            "OmniManage migrations are forward-only. Restore a backup or write a compensating migration.");
    }
}

