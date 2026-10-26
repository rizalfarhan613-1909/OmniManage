namespace OmniManage.Modules.Inventory.Presentation.Controllers;

public sealed class ListWarehousesRequest
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 25;

    public string? Search { get; set; }

    public bool? IsActive { get; set; }

    public string? Sort { get; set; }
}
