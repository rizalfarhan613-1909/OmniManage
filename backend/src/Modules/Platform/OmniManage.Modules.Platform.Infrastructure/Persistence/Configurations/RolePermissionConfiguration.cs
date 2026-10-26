using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniManage.Modules.Platform.Domain.Identity;
using OmniManage.Modules.Platform.Domain.Tenancy;

namespace OmniManage.Modules.Platform.Infrastructure.Persistence.Configurations;

public sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("role_permissions", PlatformDbContext.Schema);
        builder.HasKey(rolePermission => new { rolePermission.RoleId, rolePermission.PermissionId })
            .HasName("pk_role_permissions");

        builder.Property(rolePermission => rolePermission.PermissionId)
            .IsRequired()
            .HasMaxLength(Permission.CodeMaxLength);

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(rolePermission => rolePermission.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Role>()
            .WithMany(role => role.RolePermissions)
            .HasForeignKey(rolePermission => rolePermission.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Permission>()
            .WithMany()
            .HasForeignKey(rolePermission => rolePermission.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(rolePermission => new { rolePermission.TenantId, rolePermission.RoleId })
            .HasDatabaseName("ix_role_permissions_tenant_id_role_id");
    }
}
