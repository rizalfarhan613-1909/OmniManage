using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Manufacturing.Application.Common;

namespace OmniManage.Modules.Manufacturing.Application.BillOfMaterials.GetBillOfMaterials;

public sealed record GetBillOfMaterialsQuery(
    int Page = 1,
    int PageSize = Pagination.DefaultPageSize,
    string? Search = null,
    Guid? ProductItemId = null,
    bool? IsActive = null,
    string? Sort = null) : IQuery<Result<PagedResult<BillOfMaterialDto>>>;
