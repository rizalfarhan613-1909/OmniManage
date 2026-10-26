namespace OmniManage.Modules.Finance.Presentation.Controllers;

public sealed class ListJournalEntriesRequest
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 25;

    public string? Search { get; set; }

    public DateTimeOffset? PostedFromUtc { get; set; }

    public DateTimeOffset? PostedToUtc { get; set; }

    public string? Sort { get; set; }
}
