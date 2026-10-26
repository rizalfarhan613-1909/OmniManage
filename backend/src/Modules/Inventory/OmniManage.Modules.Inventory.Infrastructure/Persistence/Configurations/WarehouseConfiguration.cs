using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniManage.Modules.Inventory.Domain.Warehouses;

namespace OmniManage.Modules.Inventory.Infrastructure.Persistence.Configurations;

public sealed class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("warehouses", InventoryDbContext.Schema);
        builder.HasKey(warehouse => warehouse.Id).HasName("pk_warehouses");

        builder.Property(warehouse => warehouse.Code)
            .IsRequired()
            .HasMaxLength(Warehouse.CodeMaxLength);

        builder.Property(warehouse => warehouse.Name)
            .IsRequired()
            .HasMaxLength(Warehouse.NameMaxLength);

        builder.Property(warehouse => warehouse.Address)
            .HasMaxLength(Warehouse.AddressMaxLength);

        builder.Property(warehouse => warehouse.IsActive)
            .IsRequired();

        builder.Property(warehouse => warehouse.CreatedAt)
            .IsRequired();

        builder.Property(warehouse => warehouse.CreatedBy)
            .IsRequired();

        builder.HasIndex(warehouse => new { warehouse.TenantId, warehouse.Code })
            .IsUnique()
            .HasDatabaseName("uq_warehouses_tenant_id_code");

        builder.HasIndex(warehouse => new { warehouse.TenantId, warehouse.Name })
            .HasDatabaseName("ix_warehouses_tenant_id_name");
    }
}
