using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniManage.Modules.Procurement.Domain.PurchaseOrders;

namespace OmniManage.Modules.Procurement.Infrastructure.Persistence.Configurations;

public sealed class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("purchase_orders", ProcurementDbContext.Schema);
        builder.HasKey(purchaseOrder => purchaseOrder.Id).HasName("pk_purchase_orders");

        builder.Property(purchaseOrder => purchaseOrder.OrderNumber)
            .IsRequired()
            .HasMaxLength(PurchaseOrder.OrderNumberMaxLength);

        builder.Property(purchaseOrder => purchaseOrder.SupplierId)
            .IsRequired();

        builder.Property(purchaseOrder => purchaseOrder.SupplierName)
            .IsRequired()
            .HasMaxLength(PurchaseOrder.SupplierNameMaxLength);

        builder.Property(purchaseOrder => purchaseOrder.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(purchaseOrder => purchaseOrder.TotalAmount)
            .IsRequired()
            .HasPrecision(19, 4);

        builder.Property(purchaseOrder => purchaseOrder.ExpectedDeliveryDate);

        builder.Property(purchaseOrder => purchaseOrder.Notes)
            .HasMaxLength(PurchaseOrder.NotesMaxLength);

        builder.Property(purchaseOrder => purchaseOrder.CreatedAt)
            .IsRequired();

        builder.Property(purchaseOrder => purchaseOrder.CreatedBy)
            .IsRequired();

        builder.Property(purchaseOrder => purchaseOrder.UpdatedAt);

        builder.Property(purchaseOrder => purchaseOrder.UpdatedBy);

        builder
            .HasMany(purchaseOrder => purchaseOrder.Lines)
            .WithOne()
            .HasForeignKey(line => line.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(purchaseOrder => purchaseOrder.Lines)
            .HasField("_lines")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(purchaseOrder => new { purchaseOrder.TenantId, purchaseOrder.OrderNumber })
            .IsUnique()
            .HasDatabaseName("uq_purchase_orders_tenant_id_order_number");

        builder.HasIndex(purchaseOrder => new { purchaseOrder.TenantId, purchaseOrder.SupplierId })
            .HasDatabaseName("ix_purchase_orders_tenant_id_supplier_id");

        builder.HasIndex(purchaseOrder => new { purchaseOrder.TenantId, purchaseOrder.Status })
            .HasDatabaseName("ix_purchase_orders_tenant_id_status");

        builder.HasIndex(purchaseOrder => new { purchaseOrder.TenantId, purchaseOrder.CreatedAt })
            .HasDatabaseName("ix_purchase_orders_tenant_id_created_at");

        builder.HasIndex(purchaseOrder => new { purchaseOrder.TenantId, purchaseOrder.ExpectedDeliveryDate })
            .HasDatabaseName("ix_purchase_orders_tenant_id_expected_delivery_date");
    }
}
