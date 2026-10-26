using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Manufacturing.Application.Common;

namespace OmniManage.Modules.Manufacturing.Application.WorkOrders.GetWorkOrders;

public sealed record GetWorkOrdersQuery(
    int Page = 1,
    int PageSize = Pagination.DefaultPageSize,
    string? Search = null,
    string? Status = null,
    Guid? ProductItemId = null,
    Guid? BillOfMaterialId = null,
    string? Sort = null) : IQuery<Result<PagedResult<WorkOrderDto>>>;
