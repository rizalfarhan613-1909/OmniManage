using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniManage.Modules.Finance.Domain.Gl;

namespace OmniManage.Modules.Finance.Infrastructure.Persistence.Configurations;

public sealed class GlAccountConfiguration : IEntityTypeConfiguration<GlAccount>
{
    public void Configure(EntityTypeBuilder<GlAccount> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("gl_accounts", FinanceDbContext.Schema);
        builder.HasKey(account => account.Id).HasName("pk_gl_accounts");

        builder.Property(account => account.AccountCode)
            .IsRequired()
            .HasMaxLength(GlAccount.CodeMaxLength);

        builder.Property(account => account.AccountName)
            .IsRequired()
            .HasMaxLength(GlAccount.NameMaxLength);

        builder.Property(account => account.Type)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(account => account.IsActive)
            .IsRequired();

        builder.Property(account => account.CreatedAt)
            .IsRequired();

        builder.Property(account => account.CreatedBy)
            .IsRequired();

        builder.Property(account => account.UpdatedAt);

        builder.Property(account => account.UpdatedBy);

        builder.HasIndex(account => new { account.TenantId, account.AccountCode })
            .IsUnique()
            .HasDatabaseName("uq_gl_accounts_tenant_id_account_code");

        builder.HasIndex(account => new { account.TenantId, account.Type })
            .HasDatabaseName("ix_gl_accounts_tenant_id_type");
    }
}
