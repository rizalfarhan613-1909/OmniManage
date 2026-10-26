using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniManage.Modules.Procurement.Domain.Suppliers;

namespace OmniManage.Modules.Procurement.Infrastructure.Persistence.Configurations;

public sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("suppliers", ProcurementDbContext.Schema);
        builder.HasKey(supplier => supplier.Id).HasName("pk_suppliers");

        builder.Property(supplier => supplier.Code)
            .IsRequired()
            .HasMaxLength(Supplier.CodeMaxLength);

        builder.Property(supplier => supplier.Name)
            .IsRequired()
            .HasMaxLength(Supplier.NameMaxLength);

        builder.Property(supplier => supplier.ContactName)
            .HasMaxLength(Supplier.ContactNameMaxLength);

        builder.Property(supplier => supplier.Email)
            .HasMaxLength(Supplier.EmailMaxLength);

        builder.Property(supplier => supplier.Phone)
            .HasMaxLength(Supplier.PhoneMaxLength);

        builder.Property(supplier => supplier.PaymentTermsDays)
            .IsRequired();

        builder.Property(supplier => supplier.IsActive)
            .IsRequired();

        builder.Property(supplier => supplier.CreatedAt)
            .IsRequired();

        builder.Property(supplier => supplier.CreatedBy)
            .IsRequired();

        builder.Property(supplier => supplier.UpdatedAt);

        builder.Property(supplier => supplier.UpdatedBy);

        builder.HasIndex(supplier => new { supplier.TenantId, supplier.Code })
            .IsUnique()
            .HasDatabaseName("uq_suppliers_tenant_id_code");

        builder.HasIndex(supplier => new { supplier.TenantId, supplier.Name })
            .HasDatabaseName("ix_suppliers_tenant_id_name");

        builder.HasIndex(supplier => new { supplier.TenantId, supplier.IsActive })
            .HasDatabaseName("ix_suppliers_tenant_id_is_active");
    }
}
