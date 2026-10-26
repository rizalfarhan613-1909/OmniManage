using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniManage.Modules.AssetMaintenance.Domain.Assets;
using OmniManage.Modules.AssetMaintenance.Domain.Maintenance;

namespace OmniManage.Modules.AssetMaintenance.Infrastructure.Persistence.Configurations;

public sealed class MaintenanceOrderConfiguration : IEntityTypeConfiguration<MaintenanceOrder>
{
    public void Configure(EntityTypeBuilder<MaintenanceOrder> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("maintenance_orders", AssetMaintenanceDbContext.Schema);
        builder.HasKey(order => order.Id).HasName("pk_maintenance_orders");
        builder.Property(order => order.Id).ValueGeneratedNever();

        builder.Property(order => order.AssetId)
            .IsRequired();

        builder.Property(order => order.RequestNumber)
            .IsRequired()
            .HasMaxLength(MaintenanceOrder.RequestNumberMaxLength);

        builder.Property(order => order.Cost)
            .IsRequired()
            .HasPrecision(19, 4);

        builder.Property(order => order.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(order => order.Description)
            .IsRequired()
            .HasMaxLength(MaintenanceOrder.DescriptionMaxLength);

        builder.HasOne<Asset>()
            .WithMany()
            .HasForeignKey(order => order.AssetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(order => new { order.TenantId, order.RequestNumber })
            .IsUnique()
            .HasDatabaseName("uq_maintenance_orders_tenant_id_request_number");

        builder.HasIndex(order => new { order.TenantId, order.AssetId, order.Status })
            .HasDatabaseName("ix_maintenance_orders_tenant_id_asset_id_status");

        builder.HasIndex(order => new { order.TenantId, order.ScheduledDate })
            .HasDatabaseName("ix_maintenance_orders_tenant_id_scheduled_date");
    }
}
