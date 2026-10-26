using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniManage.Modules.AssetMaintenance.Domain.Assets;

namespace OmniManage.Modules.AssetMaintenance.Infrastructure.Persistence.Configurations;

public sealed class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("assets", AssetMaintenanceDbContext.Schema);
        builder.HasKey(asset => asset.Id).HasName("pk_assets");
        builder.Property(asset => asset.Id).ValueGeneratedNever();

        builder.Property(asset => asset.AssetCode)
            .IsRequired()
            .HasMaxLength(Asset.AssetCodeMaxLength);

        builder.Property(asset => asset.Name)
            .IsRequired()
            .HasMaxLength(Asset.NameMaxLength);

        builder.Property(asset => asset.Category)
            .IsRequired()
            .HasMaxLength(Asset.CategoryMaxLength);

        builder.Property(asset => asset.SerialNumber)
            .HasMaxLength(Asset.SerialNumberMaxLength);

        builder.Property(asset => asset.QrCodeUrl)
            .HasMaxLength(Asset.QrCodeUrlMaxLength);

        builder.Property(asset => asset.PurchaseValue)
            .IsRequired()
            .HasPrecision(19, 4);

        builder.Property(asset => asset.AcquisitionDate)
            .IsRequired();

        builder.Property(asset => asset.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.HasIndex(asset => new { asset.TenantId, asset.AssetCode })
            .IsUnique()
            .HasDatabaseName("uq_assets_tenant_id_asset_code");

        builder.HasIndex(asset => new { asset.TenantId, asset.SerialNumber })
            .HasDatabaseName("ix_assets_tenant_id_serial_number");

        builder.HasIndex(asset => new { asset.TenantId, asset.Category })
            .HasDatabaseName("ix_assets_tenant_id_category");

        builder.HasIndex(asset => new { asset.TenantId, asset.Status })
            .HasDatabaseName("ix_assets_tenant_id_status");
    }
}
