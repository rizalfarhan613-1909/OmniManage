using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniManage.Modules.Platform.Domain.Tenancy;

namespace OmniManage.Modules.Platform.Infrastructure.Persistence.Configurations;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("tenants", PlatformDbContext.Schema);
        builder.HasKey(tenant => tenant.Id).HasName("pk_tenants");

        builder.Property(tenant => tenant.Code)
            .IsRequired()
            .HasMaxLength(Tenant.CodeMaxLength);

        builder.Property(tenant => tenant.Name)
            .IsRequired()
            .HasMaxLength(Tenant.NameMaxLength);

        builder.Property(tenant => tenant.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(tenant => tenant.PlanCode)
            .IsRequired()
            .HasMaxLength(Tenant.PlanCodeMaxLength);

        builder.Property(tenant => tenant.CreatedAt)
            .IsRequired();

        builder.HasIndex(tenant => tenant.Code)
            .IsUnique()
            .HasDatabaseName("uq_tenants_code");
    }
}
