namespace OmniManage.Modules.Sales.Presentation.Controllers;

public sealed class ListSalesOrdersRequest
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 25;

    public string? Search { get; set; }

    public string? Status { get; set; }

    public string? Sort { get; set; }
}
