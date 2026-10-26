using Microsoft.EntityFrameworkCore;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Infrastructure.Persistence;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Inbox;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Outbox;
using OmniManage.Modules.Finance.Domain.Ar;
using OmniManage.Modules.Finance.Domain.Errors;
using OmniManage.Modules.Finance.Domain.Gl;

namespace OmniManage.Modules.Finance.Infrastructure.Persistence;

public sealed class FinanceDbContext(
    DbContextOptions<FinanceDbContext> options,
    ITenantContext tenantContext)
    : TenantDbContext(options, tenantContext)
{
    public const string Schema = "finance";

    public DbSet<GlAccount> GlAccounts => Set<GlAccount>();

    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();

    public DbSet<ArInvoice> ArInvoices => Set<ArInvoice>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        EnsureJournalIsAppendOnly();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        EnsureJournalIsAppendOnly();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.HasDefaultSchema(Schema);
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyOutboxConfiguration(Schema);
        modelBuilder.ApplyInboxConfiguration(Schema);
    }

    private void EnsureJournalIsAppendOnly()
    {
        var hasForbiddenJournalChanges = ChangeTracker
            .Entries<JournalEntry>()
            .Any(entry => entry.State is EntityState.Modified or EntityState.Deleted);

        var hasForbiddenLineChanges = ChangeTracker
            .Entries<JournalEntryLine>()
            .Any(entry => entry.State is EntityState.Modified or EntityState.Deleted);

        if (hasForbiddenJournalChanges || hasForbiddenLineChanges)
        {
            throw new InvalidOperationException(JournalEntryErrors.PostedEntriesAreImmutable.Message);
        }
    }
}
