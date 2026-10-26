using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniManage.Modules.Platform.Domain.Identity;

namespace OmniManage.Modules.Platform.Infrastructure.Persistence.Configurations;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("permissions", PlatformDbContext.Schema);
        builder.HasKey(permission => permission.Id).HasName("pk_permissions");

        builder.Property(permission => permission.Id)
            .IsRequired()
            .HasMaxLength(Permission.CodeMaxLength);
    }
}
