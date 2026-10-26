using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniManage.Modules.Manufacturing.Domain.BillOfMaterials;

namespace OmniManage.Modules.Manufacturing.Infrastructure.Persistence.Configurations;

public sealed class BillOfMaterialComponentConfiguration : IEntityTypeConfiguration<BillOfMaterialComponent>
{
    public void Configure(EntityTypeBuilder<BillOfMaterialComponent> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("bill_of_material_components", ManufacturingDbContext.Schema);
        builder.HasKey(component => component.Id).HasName("pk_bill_of_material_components");

        builder.Property(component => component.BillOfMaterialId)
            .IsRequired();

        builder.Property(component => component.ItemId)
            .IsRequired();

        builder.Property(component => component.QuantityPerUnit)
            .IsRequired()
            .HasPrecision(18, 6);

        builder.HasIndex(component => new { component.BillOfMaterialId, component.ItemId })
            .IsUnique()
            .HasDatabaseName("uq_bill_of_material_components_bom_id_item_id");
    }
}
