using System;
using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmniManage.Modules.Platform.Infrastructure.Migrations;

/// <inheritdoc />
public partial class PlatformCore_Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "audit");

        migrationBuilder.EnsureSchema(
            name: "platform");

        migrationBuilder.CreateTable(
            name: "audit_logs",
            schema: "audit",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                actor_id = table.Column<Guid>(type: "uuid", nullable: true),
                actor_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                impersonated_by = table.Column<Guid>(type: "uuid", nullable: true),
                action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                module_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                entity_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                entity_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                before_data = table.Column<string>(type: "jsonb", nullable: true),
                after_data = table.Column<string>(type: "jsonb", nullable: true),
                ip_address = table.Column<IPAddress>(type: "inet", nullable: true),
                user_agent = table.Column<string>(type: "text", nullable: true),
                trace_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                prev_hash = table.Column<byte[]>(type: "bytea", nullable: true),
                row_hash = table.Column<byte[]>(type: "bytea", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_audit_logs", x => new { x.id, x.occurred_at });
            });

        migrationBuilder.CreateTable(
            name: "permissions",
            schema: "platform",
            columns: table => new
            {
                id = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_permissions", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "platform_users",
            schema: "platform",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                password_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                is_mfa_enabled = table.Column<bool>(type: "boolean", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_platform_users", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "tenants",
            schema: "platform",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                plan_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tenants", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "roles",
            schema: "platform",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                is_system = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_roles", x => x.id);
                table.ForeignKey(
                    name: "fk_roles_tenants_tenant_id",
                    column: x => x.tenant_id,
                    principalSchema: "platform",
                    principalTable: "tenants",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "tenant_modules",
            schema: "platform",
            columns: table => new
            {
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                module_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                activated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                changed_by = table.Column<Guid>(type: "uuid", nullable: false),
                change_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                version = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tenant_modules", x => new { x.tenant_id, x.module_code });
                table.ForeignKey(
                    name: "fk_tenant_modules_tenants_tenant_id",
                    column: x => x.tenant_id,
                    principalSchema: "platform",
                    principalTable: "tenants",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "users",
            schema: "platform",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                password_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_users", x => x.id);
                table.ForeignKey(
                    name: "fk_users_tenants_tenant_id",
                    column: x => x.tenant_id,
                    principalSchema: "platform",
                    principalTable: "tenants",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "role_permissions",
            schema: "platform",
            columns: table => new
            {
                role_id = table.Column<Guid>(type: "uuid", nullable: false),
                permission_id = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_role_permissions", x => new { x.role_id, x.permission_id });
                table.ForeignKey(
                    name: "fk_role_permissions_permissions_permission_id",
                    column: x => x.permission_id,
                    principalSchema: "platform",
                    principalTable: "permissions",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_role_permissions_roles_role_id",
                    column: x => x.role_id,
                    principalSchema: "platform",
                    principalTable: "roles",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_role_permissions_tenants_tenant_id",
                    column: x => x.tenant_id,
                    principalSchema: "platform",
                    principalTable: "tenants",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "refresh_tokens",
            schema: "platform",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                session_id = table.Column<Guid>(type: "uuid", nullable: false),
                token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                device_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                issued_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                revoked_reason = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                replaced_by = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_refresh_tokens", x => x.id);
                table.ForeignKey(
                    name: "fk_refresh_tokens_refresh_tokens_replaced_by",
                    column: x => x.replaced_by,
                    principalSchema: "platform",
                    principalTable: "refresh_tokens",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_refresh_tokens_tenants_tenant_id",
                    column: x => x.tenant_id,
                    principalSchema: "platform",
                    principalTable: "tenants",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_refresh_tokens_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "platform",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "user_roles",
            schema: "platform",
            columns: table => new
            {
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                role_id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_roles", x => new { x.user_id, x.role_id });
                table.ForeignKey(
                    name: "fk_user_roles_roles_role_id",
                    column: x => x.role_id,
                    principalSchema: "platform",
                    principalTable: "roles",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_user_roles_tenants_tenant_id",
                    column: x => x.tenant_id,
                    principalSchema: "platform",
                    principalTable: "tenants",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_user_roles_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "platform",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_audit_logs_actor_id_occurred_at",
            schema: "audit",
            table: "audit_logs",
            columns: new[] { "actor_id", "occurred_at" });

        migrationBuilder.CreateIndex(
            name: "ix_audit_logs_entity_type_entity_id",
            schema: "audit",
            table: "audit_logs",
            columns: new[] { "entity_type", "entity_id" });

        migrationBuilder.CreateIndex(
            name: "ix_audit_logs_tenant_id_occurred_at",
            schema: "audit",
            table: "audit_logs",
            columns: new[] { "tenant_id", "occurred_at" });

        migrationBuilder.CreateIndex(
            name: "uq_platform_users_email",
            schema: "platform",
            table: "platform_users",
            column: "email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_refresh_tokens_replaced_by",
            schema: "platform",
            table: "refresh_tokens",
            column: "replaced_by");

        migrationBuilder.CreateIndex(
            name: "ix_refresh_tokens_tenant_id_expires_at",
            schema: "platform",
            table: "refresh_tokens",
            columns: new[] { "tenant_id", "expires_at" });

        migrationBuilder.CreateIndex(
            name: "ix_refresh_tokens_tenant_id_session_id",
            schema: "platform",
            table: "refresh_tokens",
            columns: new[] { "tenant_id", "session_id" });

        migrationBuilder.CreateIndex(
            name: "ix_refresh_tokens_tenant_id_user_id",
            schema: "platform",
            table: "refresh_tokens",
            columns: new[] { "tenant_id", "user_id" });

        migrationBuilder.CreateIndex(
            name: "ix_refresh_tokens_user_id",
            schema: "platform",
            table: "refresh_tokens",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "uq_refresh_tokens_token_hash",
            schema: "platform",
            table: "refresh_tokens",
            column: "token_hash",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_role_permissions_permission_id",
            schema: "platform",
            table: "role_permissions",
            column: "permission_id");

        migrationBuilder.CreateIndex(
            name: "ix_role_permissions_tenant_id_role_id",
            schema: "platform",
            table: "role_permissions",
            columns: new[] { "tenant_id", "role_id" });

        migrationBuilder.CreateIndex(
            name: "uq_roles_tenant_id_name",
            schema: "platform",
            table: "roles",
            columns: new[] { "tenant_id", "name" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_tenant_modules_tenant_id_active",
            schema: "platform",
            table: "tenant_modules",
            column: "tenant_id",
            filter: "is_active");

        migrationBuilder.CreateIndex(
            name: "uq_tenants_code",
            schema: "platform",
            table: "tenants",
            column: "code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_user_roles_role_id",
            schema: "platform",
            table: "user_roles",
            column: "role_id");

        migrationBuilder.CreateIndex(
            name: "ix_user_roles_tenant_id_user_id",
            schema: "platform",
            table: "user_roles",
            columns: new[] { "tenant_id", "user_id" });

        migrationBuilder.CreateIndex(
            name: "uq_users_tenant_id_email",
            schema: "platform",
            table: "users",
            columns: new[] { "tenant_id", "email" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        throw new NotSupportedException(
            "OmniManage migrations are forward-only. Restore a backup or write a compensating migration.");
    }
}

