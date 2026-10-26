using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniManage.Modules.Platform.Domain.Tenancy;

namespace OmniManage.Modules.Platform.Infrastructure.Persistence.Configurations;

public sealed class TenantModuleConfiguration : IEntityTypeConfiguration<TenantModule>
{
    public void Configure(EntityTypeBuilder<TenantModule> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("tenant_modules", PlatformDbContext.Schema);
        builder.HasKey(tenantModule => new { tenantModule.TenantId, tenantModule.ModuleCode })
            .HasName("pk_tenant_modules");

        builder.Property(tenantModule => tenantModule.ModuleCode)
            .IsRequired()
            .HasMaxLength(TenantModule.ModuleCodeMaxLength);

        builder.Property(tenantModule => tenantModule.IsActive)
            .IsRequired();

        builder.Property(tenantModule => tenantModule.ChangedBy)
            .IsRequired();

        builder.Property(tenantModule => tenantModule.ChangeReason)
            .IsRequired()
            .HasMaxLength(TenantModule.ChangeReasonMaxLength);

        builder.Property(tenantModule => tenantModule.Version)
            .IsRequired()
            .IsConcurrencyToken();

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(tenantModule => tenantModule.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(tenantModule => tenantModule.TenantId)
            .HasDatabaseName("ix_tenant_modules_tenant_id_active")
            .HasFilter("is_active");
    }
}
