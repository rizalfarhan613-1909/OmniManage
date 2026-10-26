namespace OmniManage.Modules.MasterData.Presentation.Controllers;

public sealed class ListBusinessPartnersRequest
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 25;

    public string? Search { get; set; }

    public string? Sort { get; set; }
}
