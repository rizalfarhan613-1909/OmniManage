using Microsoft.EntityFrameworkCore;
using OmniManage.Modules.Finance.Domain.Abstractions;
using OmniManage.Modules.Finance.Domain.Gl;

namespace OmniManage.Modules.Finance.Infrastructure.Persistence.Repositories;

public sealed class JournalEntryRepository(FinanceDbContext dbContext) : IJournalEntryRepository
{
    public Task AddAsync(JournalEntry journalEntry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(journalEntry);

        return dbContext.JournalEntries.AddAsync(journalEntry, cancellationToken).AsTask();
    }

    public async Task<(IReadOnlyList<JournalEntry> JournalEntries, long TotalItems)> ListAsync(
        JournalEntryListCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        IQueryable<JournalEntry> query = dbContext.JournalEntries
            .AsNoTracking()
            .Include(journalEntry => journalEntry.Lines);

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            var term = criteria.Search.Trim().ToLowerInvariant();

            query = query.Where(journalEntry =>
                journalEntry.EntryNumber.ToLower().Contains(term)
                || journalEntry.Description.ToLower().Contains(term));
        }

        if (criteria.PostedFromUtc is { } from)
        {
            query = query.Where(journalEntry => journalEntry.PostedDateUtc >= from);
        }

        if (criteria.PostedToUtc is { } to)
        {
            query = query.Where(journalEntry => journalEntry.PostedDateUtc <= to);
        }

        var totalItems = await query.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var journalEntries = await ApplySort(query, criteria)
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return (journalEntries, totalItems);
    }

    private static IQueryable<JournalEntry> ApplySort(
        IQueryable<JournalEntry> query,
        JournalEntryListCriteria criteria)
    {
        var option = criteria.Sort.Count == 0
            ? new JournalEntrySortOption(JournalEntrySortField.PostedDateUtc, true)
            : criteria.Sort[0];

        IOrderedQueryable<JournalEntry> ordered = option.Field switch
        {
            JournalEntrySortField.EntryNumber => option.Descending
                ? query.OrderByDescending(journalEntry => journalEntry.EntryNumber)
                : query.OrderBy(journalEntry => journalEntry.EntryNumber),
            JournalEntrySortField.PostedDateUtc => option.Descending
                ? query.OrderByDescending(journalEntry => journalEntry.PostedDateUtc)
                : query.OrderBy(journalEntry => journalEntry.PostedDateUtc),
            JournalEntrySortField.CreatedAt => option.Descending
                ? query.OrderByDescending(journalEntry => journalEntry.CreatedAt)
                : query.OrderBy(journalEntry => journalEntry.CreatedAt),
            _ => query.OrderByDescending(journalEntry => journalEntry.PostedDateUtc),
        };

        return ordered.ThenBy(journalEntry => journalEntry.Id);
    }
}
