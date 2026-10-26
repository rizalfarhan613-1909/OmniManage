using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Finance.Application.Common;
using OmniManage.Modules.Finance.Domain.Abstractions;
using OmniManage.Modules.Finance.Domain.Errors;

namespace OmniManage.Modules.Finance.Application.Journal.ListJournalEntries;

public sealed class ListJournalEntriesQueryHandler(IJournalEntryRepository journalEntries)
    : IQueryHandler<ListJournalEntriesQuery, Result<PagedResult<JournalEntryDto>>>
{
    private static readonly IReadOnlyDictionary<string, JournalEntrySortField> SortFields =
        new Dictionary<string, JournalEntrySortField>(StringComparer.OrdinalIgnoreCase)
        {
            ["entryNumber"] = JournalEntrySortField.EntryNumber,
            ["postedDateUtc"] = JournalEntrySortField.PostedDateUtc,
            ["createdAt"] = JournalEntrySortField.CreatedAt,
        };

    public async Task<Result<PagedResult<JournalEntryDto>>> Handle(
        ListJournalEntriesQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.PostedFromUtc is { } from
            && query.PostedToUtc is { } to
            && from > to)
        {
            return JournalEntryErrors.InvalidDateRange;
        }

        var (page, pageSize, skip) = Pagination.Normalize(query.Page, query.PageSize);
        var sort = SortParser
            .Parse(query.Sort, SortFields, JournalEntrySortField.PostedDateUtc)
            .Select(option => new JournalEntrySortOption(option.Field, option.Descending))
            .ToArray();

        var criteria = new JournalEntryListCriteria(
            skip,
            pageSize,
            query.Search?.Trim(),
            query.PostedFromUtc,
            query.PostedToUtc,
            sort);

        var (found, totalItems) = await journalEntries
            .ListAsync(criteria, cancellationToken)
            .ConfigureAwait(false);

        var dtos = found.Select(JournalEntryDto.From).ToArray();

        return new PagedResult<JournalEntryDto>(dtos, page, pageSize, totalItems);
    }
}
