using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniManage.Modules.Platform.Domain.Identity;

namespace OmniManage.Modules.Platform.Infrastructure.Persistence.Configurations;

public sealed class PlatformUserConfiguration : IEntityTypeConfiguration<PlatformUser>
{
    public void Configure(EntityTypeBuilder<PlatformUser> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("platform_users", PlatformDbContext.Schema);
        builder.HasKey(platformUser => platformUser.Id).HasName("pk_platform_users");

        builder.Property(platformUser => platformUser.Email)
            .IsRequired()
            .HasMaxLength(PlatformUser.EmailMaxLength);

        builder.Property(platformUser => platformUser.PasswordHash)
            .IsRequired()
            .HasMaxLength(PlatformUser.PasswordHashMaxLength);

        builder.Property(platformUser => platformUser.IsActive)
            .IsRequired();

        builder.Property(platformUser => platformUser.IsMfaEnabled)
            .IsRequired();

        builder.Property(platformUser => platformUser.CreatedAt)
            .IsRequired();

        builder.HasIndex(platformUser => platformUser.Email)
            .IsUnique()
            .HasDatabaseName("uq_platform_users_email");
    }
}
