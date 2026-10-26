using System;
using Microsoft.EntityFrameworkCore.Migrations;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Migrations;

#nullable disable

namespace OmniManage.Modules.Hris.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InitialHris : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "hris");

        migrationBuilder.CreateTable(
            name: "employees",
            schema: "hris",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                employee_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                job_title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                department_id = table.Column<Guid>(type: "uuid", nullable: false),
                joined_date = table.Column<DateOnly>(type: "date", nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_employees", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "inbox_messages",
            schema: "hris",
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
            schema: "hris",
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
            name: "payroll_runs",
            schema: "hris",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                period_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                processed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                total_basic_salary = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                total_allowances = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                total_deductions = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                net_pay_total = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_payroll_runs", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "attendance_logs",
            schema: "hris",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                clock_in_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                clock_out_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                latitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                longitude = table.Column<decimal>(type: "numeric(10,6)", precision: 10, scale: 6, nullable: false),
                photo_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_attendance_logs", x => x.id);
                table.ForeignKey(
                    name: "fk_attendance_logs_employees_employee_id",
                    column: x => x.employee_id,
                    principalSchema: "hris",
                    principalTable: "employees",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "ix_attendance_logs_employee_id",
            schema: "hris",
            table: "attendance_logs",
            column: "employee_id");

        migrationBuilder.CreateIndex(
            name: "ix_attendance_logs_tenant_id_clock_in_utc",
            schema: "hris",
            table: "attendance_logs",
            columns: new[] { "tenant_id", "clock_in_utc" });

        migrationBuilder.CreateIndex(
            name: "ix_attendance_logs_tenant_id_employee_id_clock_in_utc",
            schema: "hris",
            table: "attendance_logs",
            columns: new[] { "tenant_id", "employee_id", "clock_in_utc" });

        migrationBuilder.CreateIndex(
            name: "ix_attendance_logs_tenant_id_status",
            schema: "hris",
            table: "attendance_logs",
            columns: new[] { "tenant_id", "status" });

        migrationBuilder.CreateIndex(
            name: "ix_employees_tenant_id_department_id",
            schema: "hris",
            table: "employees",
            columns: new[] { "tenant_id", "department_id" });

        migrationBuilder.CreateIndex(
            name: "ix_employees_tenant_id_is_active",
            schema: "hris",
            table: "employees",
            columns: new[] { "tenant_id", "is_active" });

        migrationBuilder.CreateIndex(
            name: "uq_employees_tenant_id_email",
            schema: "hris",
            table: "employees",
            columns: new[] { "tenant_id", "email" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "uq_employees_tenant_id_employee_code",
            schema: "hris",
            table: "employees",
            columns: new[] { "tenant_id", "employee_code" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_inbox_messages_tenant_id_event_id",
            schema: "hris",
            table: "inbox_messages",
            columns: new[] { "tenant_id", "event_id" });

        migrationBuilder.CreateIndex(
            name: "ix_outbox_messages_occurred_on_utc_unprocessed",
            schema: "hris",
            table: "outbox_messages",
            column: "occurred_on_utc",
            filter: "processed_on_utc IS NULL");

        migrationBuilder.CreateIndex(
            name: "ix_outbox_messages_tenant_id_occurred_on_utc",
            schema: "hris",
            table: "outbox_messages",
            columns: new[] { "tenant_id", "occurred_on_utc" });

        migrationBuilder.CreateIndex(
            name: "ix_payroll_runs_tenant_id_processed_at_utc",
            schema: "hris",
            table: "payroll_runs",
            columns: new[] { "tenant_id", "processed_at_utc" });

        migrationBuilder.CreateIndex(
            name: "ix_payroll_runs_tenant_id_status",
            schema: "hris",
            table: "payroll_runs",
            columns: new[] { "tenant_id", "status" });

        migrationBuilder.CreateIndex(
            name: "uq_payroll_runs_tenant_id_period_code",
            schema: "hris",
            table: "payroll_runs",
            columns: new[] { "tenant_id", "period_code" },
            unique: true);

        migrationBuilder.EnableTenantRls("hris", "employees");
        migrationBuilder.EnableTenantRls("hris", "attendance_logs");
        migrationBuilder.EnableTenantRls("hris", "payroll_runs");
        migrationBuilder.EnableTenantRls("hris", "inbox_messages");
        migrationBuilder.EnableTenantRls("hris", "outbox_messages");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        throw new NotSupportedException(
            "OmniManage migrations are forward-only. Restore a backup or write a compensating migration.");
    }
}
