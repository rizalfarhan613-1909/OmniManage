using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniManage.Modules.Manufacturing.Domain.BillOfMaterials;

namespace OmniManage.Modules.Manufacturing.Infrastructure.Persistence.Configurations;

public sealed class BillOfMaterialConfiguration : IEntityTypeConfiguration<BillOfMaterial>
{
    public void Configure(EntityTypeBuilder<BillOfMaterial> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("bill_of_materials", ManufacturingDbContext.Schema);
        builder.HasKey(billOfMaterial => billOfMaterial.Id).HasName("pk_bill_of_materials");

        builder.Property(billOfMaterial => billOfMaterial.Code)
            .IsRequired()
            .HasMaxLength(BillOfMaterial.CodeMaxLength);

        builder.Property(billOfMaterial => billOfMaterial.Name)
            .IsRequired()
            .HasMaxLength(BillOfMaterial.NameMaxLength);

        builder.Property(billOfMaterial => billOfMaterial.ProductItemId)
            .IsRequired();

        builder.Property(billOfMaterial => billOfMaterial.Version)
            .IsRequired();

        builder.Property(billOfMaterial => billOfMaterial.IsActive)
            .IsRequired();

        builder.Property(billOfMaterial => billOfMaterial.Notes)
            .HasMaxLength(BillOfMaterial.NotesMaxLength);

        builder.Property(billOfMaterial => billOfMaterial.CreatedAt)
            .IsRequired();

        builder.Property(billOfMaterial => billOfMaterial.CreatedBy)
            .IsRequired();

        builder.Property(billOfMaterial => billOfMaterial.UpdatedAt);

        builder.Property(billOfMaterial => billOfMaterial.UpdatedBy);

        builder
            .HasMany(billOfMaterial => billOfMaterial.Components)
            .WithOne()
            .HasForeignKey(component => component.BillOfMaterialId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(billOfMaterial => billOfMaterial.Components)
            .HasField("_components")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(billOfMaterial => new
            {
                billOfMaterial.TenantId,
                billOfMaterial.Code,
                billOfMaterial.Version,
            })
            .IsUnique()
            .HasDatabaseName("uq_bill_of_materials_tenant_id_code_version");

        builder.HasIndex(billOfMaterial => new { billOfMaterial.TenantId, billOfMaterial.ProductItemId })
            .HasDatabaseName("ix_bill_of_materials_tenant_id_product_item_id");

        builder.HasIndex(billOfMaterial => new { billOfMaterial.TenantId, billOfMaterial.IsActive })
            .HasDatabaseName("ix_bill_of_materials_tenant_id_is_active");

        builder.HasIndex(billOfMaterial => new { billOfMaterial.TenantId, billOfMaterial.CreatedAt })
            .HasDatabaseName("ix_bill_of_materials_tenant_id_created_at");
    }
}
