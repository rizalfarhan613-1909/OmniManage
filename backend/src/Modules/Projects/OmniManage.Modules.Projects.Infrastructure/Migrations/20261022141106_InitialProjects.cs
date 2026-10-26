using System;
using Microsoft.EntityFrameworkCore.Migrations;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Migrations;

#nullable disable

namespace OmniManage.Modules.Projects.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InitialProjects : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "projects");

        migrationBuilder.CreateTable(
            name: "inbox_messages",
            schema: "projects",
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
            schema: "projects",
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
            name: "projects",
            schema: "projects",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                start_date = table.Column<DateOnly>(type: "date", nullable: false),
                end_date = table.Column<DateOnly>(type: "date", nullable: true),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_projects", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "tasks",
            schema: "projects",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                project_id = table.Column<Guid>(type: "uuid", nullable: false),
                title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                assignee_employee_id = table.Column<Guid>(type: "uuid", nullable: true),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                estimated_hours = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                completed_hours = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tasks", x => x.id);
                table.ForeignKey(
                    name: "fk_tasks_projects_project_id",
                    column: x => x.project_id,
                    principalSchema: "projects",
                    principalTable: "projects",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "work_logs",
            schema: "projects",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                project_task_id = table.Column<Guid>(type: "uuid", nullable: false),
                employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                logged_date = table.Column<DateOnly>(type: "date", nullable: false),
                hours_spent = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_work_logs", x => x.id);
                table.ForeignKey(
                    name: "fk_work_logs_tasks_project_task_id",
                    column: x => x.project_task_id,
                    principalSchema: "projects",
                    principalTable: "tasks",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_inbox_messages_tenant_id_event_id",
            schema: "projects",
            table: "inbox_messages",
            columns: new[] { "tenant_id", "event_id" });

        migrationBuilder.CreateIndex(
            name: "ix_outbox_messages_occurred_on_utc_unprocessed",
            schema: "projects",
            table: "outbox_messages",
            column: "occurred_on_utc",
            filter: "processed_on_utc IS NULL");

        migrationBuilder.CreateIndex(
            name: "ix_outbox_messages_tenant_id_occurred_on_utc",
            schema: "projects",
            table: "outbox_messages",
            columns: new[] { "tenant_id", "occurred_on_utc" });

        migrationBuilder.CreateIndex(
            name: "ix_projects_tenant_id_customer_id",
            schema: "projects",
            table: "projects",
            columns: new[] { "tenant_id", "customer_id" });

        migrationBuilder.CreateIndex(
            name: "ix_projects_tenant_id_name",
            schema: "projects",
            table: "projects",
            columns: new[] { "tenant_id", "name" });

        migrationBuilder.CreateIndex(
            name: "ix_projects_tenant_id_start_date",
            schema: "projects",
            table: "projects",
            columns: new[] { "tenant_id", "start_date" });

        migrationBuilder.CreateIndex(
            name: "ix_projects_tenant_id_status",
            schema: "projects",
            table: "projects",
            columns: new[] { "tenant_id", "status" });

        migrationBuilder.CreateIndex(
            name: "uq_projects_tenant_id_code",
            schema: "projects",
            table: "projects",
            columns: new[] { "tenant_id", "code" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_tasks_project_id",
            schema: "projects",
            table: "tasks",
            column: "project_id");

        migrationBuilder.CreateIndex(
            name: "ix_tasks_tenant_id_assignee_employee_id",
            schema: "projects",
            table: "tasks",
            columns: new[] { "tenant_id", "assignee_employee_id" });

        migrationBuilder.CreateIndex(
            name: "ix_tasks_tenant_id_project_id",
            schema: "projects",
            table: "tasks",
            columns: new[] { "tenant_id", "project_id" });

        migrationBuilder.CreateIndex(
            name: "ix_tasks_tenant_id_status",
            schema: "projects",
            table: "tasks",
            columns: new[] { "tenant_id", "status" });

        migrationBuilder.CreateIndex(
            name: "ix_work_logs_project_task_id",
            schema: "projects",
            table: "work_logs",
            column: "project_task_id");

        migrationBuilder.CreateIndex(
            name: "ix_work_logs_tenant_id_employee_id_logged_date",
            schema: "projects",
            table: "work_logs",
            columns: new[] { "tenant_id", "employee_id", "logged_date" });

        migrationBuilder.CreateIndex(
            name: "ix_work_logs_tenant_id_project_task_id_logged_date",
            schema: "projects",
            table: "work_logs",
            columns: new[] { "tenant_id", "project_task_id", "logged_date" });

        migrationBuilder.EnableTenantRls("projects", "projects");
        migrationBuilder.EnableTenantRls("projects", "tasks");
        migrationBuilder.EnableTenantRls("projects", "work_logs");
        migrationBuilder.EnableTenantRls("projects", "inbox_messages");
        migrationBuilder.EnableTenantRls("projects", "outbox_messages");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        throw new NotSupportedException(
            "OmniManage migrations are forward-only. Restore a backup or write a compensating migration.");
    }
}
