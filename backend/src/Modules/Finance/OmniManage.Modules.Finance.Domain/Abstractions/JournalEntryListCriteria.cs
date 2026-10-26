namespace OmniManage.Modules.Finance.Domain.Abstractions;

public sealed record JournalEntryListCriteria(
    int Skip,
    int Take,
    string? Search,
    DateTimeOffset? PostedFromUtc,
    DateTimeOffset? PostedToUtc,
    IReadOnlyList<JournalEntrySortOption> Sort);

public sealed record JournalEntrySortOption(JournalEntrySortField Field, bool Descending);

public enum JournalEntrySortField
{
    EntryNumber = 1,
    PostedDateUtc = 2,
    CreatedAt = 3,
}
