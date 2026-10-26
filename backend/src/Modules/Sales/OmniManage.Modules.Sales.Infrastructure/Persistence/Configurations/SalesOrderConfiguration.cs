using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniManage.Modules.Sales.Domain.Orders;

namespace OmniManage.Modules.Sales.Infrastructure.Persistence.Configurations;

public sealed class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("sales_orders", SalesDbContext.Schema);
        builder.HasKey(salesOrder => salesOrder.Id).HasName("pk_sales_orders");

        builder.Property(salesOrder => salesOrder.OrderNumber)
            .IsRequired()
            .HasMaxLength(SalesOrder.OrderNumberMaxLength);

        builder.Property(salesOrder => salesOrder.CustomerId)
            .IsRequired();

        builder.Property(salesOrder => salesOrder.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(salesOrder => salesOrder.TotalAmount)
            .IsRequired()
            .HasPrecision(19, 4);

        builder.Property(salesOrder => salesOrder.CreatedAt)
            .IsRequired();

        builder.Property(salesOrder => salesOrder.CreatedBy)
            .IsRequired();

        builder.Property(salesOrder => salesOrder.UpdatedAt);

        builder.Property(salesOrder => salesOrder.UpdatedBy);

        builder
            .HasMany(salesOrder => salesOrder.Lines)
            .WithOne()
            .HasForeignKey(line => line.SalesOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(salesOrder => salesOrder.Lines)
            .HasField("_lines")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(salesOrder => new { salesOrder.TenantId, salesOrder.OrderNumber })
            .IsUnique()
            .HasDatabaseName("uq_sales_orders_tenant_id_order_number");

        builder.HasIndex(salesOrder => new { salesOrder.TenantId, salesOrder.Status })
            .HasDatabaseName("ix_sales_orders_tenant_id_status");

        builder.HasIndex(salesOrder => new { salesOrder.TenantId, salesOrder.CreatedAt })
            .HasDatabaseName("ix_sales_orders_tenant_id_created_at");
    }
}
