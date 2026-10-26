using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniManage.Modules.MasterData.Domain.Items;

namespace OmniManage.Modules.MasterData.Infrastructure.Persistence.Configurations;

public sealed class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("items", MasterDataDbContext.Schema);
        builder.HasKey(item => item.Id).HasName("pk_items");

        builder.Property(item => item.Sku)
            .IsRequired()
            .HasMaxLength(Item.SkuMaxLength);

        builder.Property(item => item.Name)
            .IsRequired()
            .HasMaxLength(Item.NameMaxLength);

        builder.Property(item => item.UomCode)
            .IsRequired()
            .HasMaxLength(Item.UomCodeMaxLength);

        builder.Property(item => item.Category)
            .HasMaxLength(Item.CategoryMaxLength);

        builder.Property(item => item.BasePrice)
            .IsRequired()
            .HasPrecision(19, 4);

        builder.Property(item => item.CostPrice)
            .IsRequired()
            .HasPrecision(19, 4);

        builder.Property(item => item.IsStockTracked)
            .IsRequired();

        builder.Property(item => item.IsActive)
            .IsRequired();

        builder.Property(item => item.CreatedAt)
            .IsRequired();

        builder.Property(item => item.CreatedBy)
            .IsRequired();

        builder.HasIndex(item => new { item.TenantId, item.Sku })
            .IsUnique()
            .HasDatabaseName("uq_items_tenant_id_sku");

        builder.HasIndex(item => new { item.TenantId, item.Name })
            .HasDatabaseName("ix_items_tenant_id_name");

        builder.HasIndex(item => new { item.TenantId, item.Category })
            .HasDatabaseName("ix_items_tenant_id_category");
    }
}
