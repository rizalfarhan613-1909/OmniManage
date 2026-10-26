using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniManage.Modules.Finance.Domain.Gl;

namespace OmniManage.Modules.Finance.Infrastructure.Persistence.Configurations;

public sealed class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("journal_entries", FinanceDbContext.Schema);
        builder.HasKey(entry => entry.Id).HasName("pk_journal_entries");

        builder.Property(entry => entry.EntryNumber)
            .IsRequired()
            .HasMaxLength(JournalEntry.EntryNumberMaxLength);

        builder.Property(entry => entry.PostedDateUtc)
            .IsRequired();

        builder.Property(entry => entry.Description)
            .IsRequired()
            .HasMaxLength(JournalEntry.DescriptionMaxLength);

        builder.Property(entry => entry.CreatedAt)
            .IsRequired();

        builder.Property(entry => entry.CreatedBy)
            .IsRequired();

        builder.Property(entry => entry.UpdatedAt);

        builder.Property(entry => entry.UpdatedBy);

        builder
            .HasMany(entry => entry.Lines)
            .WithOne()
            .HasForeignKey(line => line.JournalEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(entry => entry.Lines)
            .HasField("_lines")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(entry => new { entry.TenantId, entry.EntryNumber })
            .IsUnique()
            .HasDatabaseName("uq_journal_entries_tenant_id_entry_number");

        builder.HasIndex(entry => new { entry.TenantId, entry.PostedDateUtc })
            .HasDatabaseName("ix_journal_entries_tenant_id_posted_date_utc");
    }
}

public sealed class JournalEntryLineConfiguration : IEntityTypeConfiguration<JournalEntryLine>
{
    public void Configure(EntityTypeBuilder<JournalEntryLine> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("journal_entry_lines", FinanceDbContext.Schema);
        builder.HasKey(line => line.Id).HasName("pk_journal_entry_lines");

        builder.Property(line => line.JournalEntryId)
            .IsRequired();

        builder.Property(line => line.GlAccountId)
            .IsRequired();

        builder.Property(line => line.Debit)
            .IsRequired()
            .HasPrecision(19, 4);

        builder.Property(line => line.Credit)
            .IsRequired()
            .HasPrecision(19, 4);

        builder.HasIndex(line => new { line.JournalEntryId, line.GlAccountId })
            .HasDatabaseName("ix_journal_entry_lines_journal_entry_id_gl_account_id");
    }
}
