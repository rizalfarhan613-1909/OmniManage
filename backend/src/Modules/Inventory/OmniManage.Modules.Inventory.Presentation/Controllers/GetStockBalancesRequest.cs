namespace OmniManage.Modules.Inventory.Presentation.Controllers;

public sealed class GetStockBalancesRequest
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 25;

    public Guid? WarehouseId { get; set; }

    public Guid? ItemId { get; set; }
}
