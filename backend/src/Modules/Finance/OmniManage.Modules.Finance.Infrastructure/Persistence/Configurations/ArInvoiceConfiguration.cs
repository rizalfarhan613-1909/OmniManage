using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniManage.Modules.Finance.Domain.Ar;

namespace OmniManage.Modules.Finance.Infrastructure.Persistence.Configurations;

public sealed class ArInvoiceConfiguration : IEntityTypeConfiguration<ArInvoice>
{
    public void Configure(EntityTypeBuilder<ArInvoice> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("ar_invoices", FinanceDbContext.Schema);
        builder.HasKey(invoice => invoice.Id).HasName("pk_ar_invoices");

        builder.Property(invoice => invoice.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(ArInvoice.InvoiceNumberMaxLength);

        builder.Property(invoice => invoice.SalesOrderId)
            .IsRequired();

        builder.Property(invoice => invoice.CustomerId)
            .IsRequired();

        builder.Property(invoice => invoice.TotalAmount)
            .IsRequired()
            .HasPrecision(19, 4);

        builder.Property(invoice => invoice.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(invoice => invoice.CreatedAt)
            .IsRequired();

        builder.Property(invoice => invoice.CreatedBy)
            .IsRequired();

        builder.Property(invoice => invoice.UpdatedAt);

        builder.Property(invoice => invoice.UpdatedBy);

        builder.HasIndex(invoice => new { invoice.TenantId, invoice.InvoiceNumber })
            .IsUnique()
            .HasDatabaseName("uq_ar_invoices_tenant_id_invoice_number");

        // One A/R invoice per sales order: database-level defence for the
        // consumer's inbox idempotency.
        builder.HasIndex(invoice => new { invoice.TenantId, invoice.SalesOrderId })
            .IsUnique()
            .HasDatabaseName("uq_ar_invoices_tenant_id_sales_order_id");

        builder.HasIndex(invoice => new { invoice.TenantId, invoice.Status })
            .HasDatabaseName("ix_ar_invoices_tenant_id_status");

        builder.HasIndex(invoice => new { invoice.TenantId, invoice.CustomerId })
            .HasDatabaseName("ix_ar_invoices_tenant_id_customer_id");

        builder.HasIndex(invoice => new { invoice.TenantId, invoice.CreatedAt })
            .HasDatabaseName("ix_ar_invoices_tenant_id_created_at");
    }
}
