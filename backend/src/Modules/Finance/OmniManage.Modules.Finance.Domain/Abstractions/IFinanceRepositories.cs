using OmniManage.Modules.Finance.Domain.Ar;
using OmniManage.Modules.Finance.Domain.Gl;

namespace OmniManage.Modules.Finance.Domain.Abstractions;

public interface IGlAccountRepository
{
    Task<GlAccount?> GetByCodeAsync(string accountCode, CancellationToken cancellationToken = default);

    Task AddAsync(GlAccount account, CancellationToken cancellationToken = default);
}

public interface IJournalEntryRepository
{
    Task AddAsync(JournalEntry journalEntry, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<JournalEntry> JournalEntries, long TotalItems)> ListAsync(
        JournalEntryListCriteria criteria,
        CancellationToken cancellationToken = default);
}

public interface IArInvoiceRepository
{
    Task AddAsync(ArInvoice invoice, CancellationToken cancellationToken = default);

    Task<ArInvoice?> GetBySalesOrderIdAsync(Guid salesOrderId, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<ArInvoice> Invoices, long TotalItems)> ListAsync(
        ArInvoiceListCriteria criteria,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Commits every change staged by the Finance repositories, the inbox and the
/// outbox in one transaction.
/// </summary>
public interface IFinanceUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Idempotency store for integration-event consumers in the Finance module.
/// </summary>
public interface IFinanceInbox
{
    Task<bool> IsProcessedAsync(
        Guid eventId,
        string consumer,
        CancellationToken cancellationToken = default);

    Task MarkProcessedAsync(
        Guid eventId,
        string consumer,
        Guid tenantId,
        DateTimeOffset processedOnUtc,
        CancellationToken cancellationToken = default);

    Task MarkSkippedAsync(
        Guid eventId,
        string consumer,
        Guid tenantId,
        DateTimeOffset processedOnUtc,
        CancellationToken cancellationToken = default);

    Task MarkFailedAsync(
        Guid eventId,
        string consumer,
        Guid tenantId,
        DateTimeOffset processedOnUtc,
        CancellationToken cancellationToken = default);
}
