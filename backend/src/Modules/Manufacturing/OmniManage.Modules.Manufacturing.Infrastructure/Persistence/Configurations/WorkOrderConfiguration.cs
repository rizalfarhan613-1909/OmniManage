using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniManage.Modules.Manufacturing.Domain.WorkOrders;

namespace OmniManage.Modules.Manufacturing.Infrastructure.Persistence.Configurations;

public sealed class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("work_orders", ManufacturingDbContext.Schema);
        builder.HasKey(workOrder => workOrder.Id).HasName("pk_work_orders");

        builder.Property(workOrder => workOrder.OrderNumber)
            .IsRequired()
            .HasMaxLength(WorkOrder.OrderNumberMaxLength);

        builder.Property(workOrder => workOrder.ProductItemId)
            .IsRequired();

        builder.Property(workOrder => workOrder.BillOfMaterialId)
            .IsRequired();

        builder.Property(workOrder => workOrder.BillOfMaterialCode)
            .IsRequired()
            .HasMaxLength(WorkOrder.BillOfMaterialCodeMaxLength);

        builder.Property(workOrder => workOrder.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(workOrder => workOrder.QuantityPlanned)
            .IsRequired()
            .HasPrecision(18, 6);

        builder.Property(workOrder => workOrder.QuantityProduced)
            .IsRequired()
            .HasPrecision(18, 6);

        builder.Property(workOrder => workOrder.DueDate);

        builder.Property(workOrder => workOrder.Notes)
            .HasMaxLength(WorkOrder.NotesMaxLength);

        builder.Property(workOrder => workOrder.CreatedAt)
            .IsRequired();

        builder.Property(workOrder => workOrder.CreatedBy)
            .IsRequired();

        builder.Property(workOrder => workOrder.UpdatedAt);

        builder.Property(workOrder => workOrder.UpdatedBy);

        builder.HasIndex(workOrder => new { workOrder.TenantId, workOrder.OrderNumber })
            .IsUnique()
            .HasDatabaseName("uq_work_orders_tenant_id_order_number");

        builder.HasIndex(workOrder => new { workOrder.TenantId, workOrder.Status })
            .HasDatabaseName("ix_work_orders_tenant_id_status");

        builder.HasIndex(workOrder => new { workOrder.TenantId, workOrder.ProductItemId })
            .HasDatabaseName("ix_work_orders_tenant_id_product_item_id");

        builder.HasIndex(workOrder => new { workOrder.TenantId, workOrder.BillOfMaterialId })
            .HasDatabaseName("ix_work_orders_tenant_id_bill_of_material_id");

        builder.HasIndex(workOrder => new { workOrder.TenantId, workOrder.DueDate })
            .HasDatabaseName("ix_work_orders_tenant_id_due_date");

        builder.HasIndex(workOrder => new { workOrder.TenantId, workOrder.CreatedAt })
            .HasDatabaseName("ix_work_orders_tenant_id_created_at");
    }
}
