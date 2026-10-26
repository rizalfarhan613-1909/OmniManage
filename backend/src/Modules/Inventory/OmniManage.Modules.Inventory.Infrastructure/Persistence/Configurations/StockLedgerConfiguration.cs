using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniManage.Modules.Inventory.Domain.Stock;

namespace OmniManage.Modules.Inventory.Infrastructure.Persistence.Configurations;

public sealed class StockLedgerConfiguration : IEntityTypeConfiguration<StockLedger>
{
    public void Configure(EntityTypeBuilder<StockLedger> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("stock_ledger", InventoryDbContext.Schema);
        builder.HasKey(entry => entry.Id).HasName("pk_stock_ledger");

        builder.Property(entry => entry.WarehouseId)
            .IsRequired();

        builder.Property(entry => entry.ItemId)
            .IsRequired();

        builder.Property(entry => entry.QuantityDelta)
            .IsRequired()
            .HasPrecision(19, 4);

        builder.Property(entry => entry.MovementType)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(entry => entry.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(StockLedger.ReferenceNumberMaxLength);

        builder.Property(entry => entry.OccurredAtUtc)
            .IsRequired();

        builder.HasIndex(entry => new { entry.TenantId, entry.WarehouseId, entry.ItemId })
            .HasDatabaseName("ix_stock_ledger_tenant_id_warehouse_id_item_id");

        builder.HasIndex(entry => new { entry.TenantId, entry.OccurredAtUtc })
            .HasDatabaseName("ix_stock_ledger_tenant_id_occurred_at_utc");
    }
}
