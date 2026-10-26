using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniManage.Modules.Platform.Domain.Identity;
using OmniManage.Modules.Platform.Domain.Tenancy;

namespace OmniManage.Modules.Platform.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("users", PlatformDbContext.Schema);
        builder.HasKey(user => user.Id).HasName("pk_users");

        builder.Property(user => user.Email)
            .IsRequired()
            .HasMaxLength(User.EmailMaxLength);

        builder.Property(user => user.PasswordHash)
            .IsRequired()
            .HasMaxLength(User.PasswordHashMaxLength);

        builder.Property(user => user.IsActive)
            .IsRequired();

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(user => user.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(user => user.UserRoles)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(user => new { user.TenantId, user.Email })
            .IsUnique()
            .HasDatabaseName("uq_users_tenant_id_email");
    }
}
