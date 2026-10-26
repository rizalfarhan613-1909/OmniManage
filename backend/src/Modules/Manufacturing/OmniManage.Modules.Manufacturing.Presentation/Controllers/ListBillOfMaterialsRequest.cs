namespace OmniManage.Modules.Manufacturing.Presentation.Controllers;

public sealed class ListBillOfMaterialsRequest
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 25;

    public string? Search { get; set; }

    public Guid? ProductItemId { get; set; }

    public bool? IsActive { get; set; }

    public string? Sort { get; set; }
}
