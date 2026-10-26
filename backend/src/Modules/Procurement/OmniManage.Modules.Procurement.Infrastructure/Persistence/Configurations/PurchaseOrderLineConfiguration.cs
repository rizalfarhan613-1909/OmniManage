using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniManage.Modules.Procurement.Domain.PurchaseOrders;

namespace OmniManage.Modules.Procurement.Infrastructure.Persistence.Configurations;

public sealed class PurchaseOrderLineConfiguration : IEntityTypeConfiguration<PurchaseOrderLine>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderLine> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("purchase_order_lines", ProcurementDbContext.Schema);
        builder.HasKey(line => line.Id).HasName("pk_purchase_order_lines");

        builder.Property(line => line.PurchaseOrderId)
            .IsRequired();

        builder.Property(line => line.ItemId)
            .IsRequired();

        builder.Property(line => line.Quantity)
            .IsRequired()
            .HasPrecision(18, 6);

        builder.Property(line => line.UnitPrice)
            .IsRequired()
            .HasPrecision(19, 4);

        builder.Property(line => line.LineTotal)
            .IsRequired()
            .HasPrecision(19, 4);

        builder.HasIndex(line => new { line.PurchaseOrderId, line.ItemId })
            .HasDatabaseName("ix_purchase_order_lines_purchase_order_id_item_id");
    }
}
