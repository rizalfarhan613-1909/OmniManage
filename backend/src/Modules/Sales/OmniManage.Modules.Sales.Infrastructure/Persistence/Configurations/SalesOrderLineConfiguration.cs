using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniManage.Modules.Sales.Domain.Orders;

namespace OmniManage.Modules.Sales.Infrastructure.Persistence.Configurations;

public sealed class SalesOrderLineConfiguration : IEntityTypeConfiguration<SalesOrderLine>
{
    public void Configure(EntityTypeBuilder<SalesOrderLine> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("sales_order_lines", SalesDbContext.Schema);
        builder.HasKey(line => line.Id).HasName("pk_sales_order_lines");

        builder.Property(line => line.SalesOrderId)
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

        builder.HasIndex(line => new { line.SalesOrderId, line.ItemId })
            .HasDatabaseName("ix_sales_order_lines_sales_order_id_item_id");
    }
}
