using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniManage.Modules.Platform.Domain.Identity;
using OmniManage.Modules.Platform.Domain.Tenancy;

namespace OmniManage.Modules.Platform.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("roles", PlatformDbContext.Schema);
        builder.HasKey(role => role.Id).HasName("pk_roles");

        builder.Property(role => role.Name)
            .IsRequired()
            .HasMaxLength(Role.NameMaxLength);

        builder.Property(role => role.IsSystem)
            .IsRequired();

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(role => role.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(role => role.RolePermissions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(role => new { role.TenantId, role.Name })
            .IsUnique()
            .HasDatabaseName("uq_roles_tenant_id_name");
    }
}
