namespace OmniManage.Modules.Procurement.Presentation.Controllers;

public sealed class ListPurchaseOrdersRequest
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 25;

    public string? Search { get; set; }

    public string? Status { get; set; }

    public Guid? SupplierId { get; set; }

    public string? Sort { get; set; }
}
