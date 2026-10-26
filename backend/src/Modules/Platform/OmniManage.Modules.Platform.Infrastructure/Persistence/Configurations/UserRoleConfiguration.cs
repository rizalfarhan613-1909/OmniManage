using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniManage.Modules.Platform.Domain.Identity;
using OmniManage.Modules.Platform.Domain.Tenancy;

namespace OmniManage.Modules.Platform.Infrastructure.Persistence.Configurations;

public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("user_roles", PlatformDbContext.Schema);
        builder.HasKey(userRole => new { userRole.UserId, userRole.RoleId })
            .HasName("pk_user_roles");

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(userRole => userRole.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany(user => user.UserRoles)
            .HasForeignKey(userRole => userRole.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Role>()
            .WithMany()
            .HasForeignKey(userRole => userRole.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(userRole => new { userRole.TenantId, userRole.UserId })
            .HasDatabaseName("ix_user_roles_tenant_id_user_id");

        builder.HasIndex(userRole => userRole.RoleId)
            .HasDatabaseName("ix_user_roles_role_id");
    }
}
