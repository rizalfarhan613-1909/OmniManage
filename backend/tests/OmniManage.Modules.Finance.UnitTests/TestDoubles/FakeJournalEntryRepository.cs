using OmniManage.Modules.Finance.Domain.Abstractions;
using OmniManage.Modules.Finance.Domain.Gl;

namespace OmniManage.Modules.Finance.UnitTests.TestDoubles;

internal sealed class FakeJournalEntryRepository : IJournalEntryRepository
{
    public List<JournalEntry> JournalEntries { get; } = [];

    public JournalEntryListCriteria? LastCriteria { get; private set; }

    public Task AddAsync(JournalEntry journalEntry, CancellationToken cancellationToken = default)
    {
        JournalEntries.Add(journalEntry);
        return Task.CompletedTask;
    }

    public Task<(IReadOnlyList<JournalEntry> JournalEntries, long TotalItems)> ListAsync(
        JournalEntryListCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        LastCriteria = criteria;

        IReadOnlyList<JournalEntry> journalEntries = JournalEntries
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .ToArray();

        return Task.FromResult((journalEntries, (long)JournalEntries.Count));
    }
}
