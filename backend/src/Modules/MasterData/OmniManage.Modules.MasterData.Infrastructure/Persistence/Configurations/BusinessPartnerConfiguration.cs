using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniManage.Modules.MasterData.Domain.BusinessPartners;

namespace OmniManage.Modules.MasterData.Infrastructure.Persistence.Configurations;

public sealed class BusinessPartnerConfiguration : IEntityTypeConfiguration<BusinessPartner>
{
    public void Configure(EntityTypeBuilder<BusinessPartner> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("business_partners", MasterDataDbContext.Schema);
        builder.HasKey(businessPartner => businessPartner.Id).HasName("pk_business_partners");

        builder.Property(businessPartner => businessPartner.Code)
            .IsRequired()
            .HasMaxLength(BusinessPartner.CodeMaxLength);

        builder.Property(businessPartner => businessPartner.Name)
            .IsRequired()
            .HasMaxLength(BusinessPartner.NameMaxLength);

        builder.Property(businessPartner => businessPartner.PartnerType)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(businessPartner => businessPartner.TaxId)
            .HasMaxLength(BusinessPartner.TaxIdMaxLength);

        builder.Property(businessPartner => businessPartner.CreditLimit)
            .IsRequired()
            .HasPrecision(19, 4);

        builder.Property(businessPartner => businessPartner.IsActive)
            .IsRequired();

        builder.Property(businessPartner => businessPartner.CreatedAt)
            .IsRequired();

        builder.Property(businessPartner => businessPartner.CreatedBy)
            .IsRequired();

        builder.HasIndex(businessPartner => new { businessPartner.TenantId, businessPartner.Code })
            .IsUnique()
            .HasDatabaseName("uq_business_partners_tenant_id_code");

        builder.HasIndex(businessPartner => new { businessPartner.TenantId, businessPartner.Name })
            .HasDatabaseName("ix_business_partners_tenant_id_name");
    }
}
