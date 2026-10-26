using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Finance.Application.Common;

namespace OmniManage.Modules.Finance.Application.Journal.ListJournalEntries;

public sealed record ListJournalEntriesQuery(
    int Page = 1,
    int PageSize = Pagination.DefaultPageSize,
    string? Search = null,
    DateTimeOffset? PostedFromUtc = null,
    DateTimeOffset? PostedToUtc = null,
    string? Sort = null) : IQuery<Result<PagedResult<JournalEntryDto>>>;
