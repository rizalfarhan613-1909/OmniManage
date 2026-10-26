using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Procurement.Application.Common;

namespace OmniManage.Modules.Procurement.Application.PurchaseOrders.GetPurchaseOrders;

public sealed record GetPurchaseOrdersQuery(
    int Page = 1,
    int PageSize = Pagination.DefaultPageSize,
    string? Search = null,
    string? Status = null,
    Guid? SupplierId = null,
    string? Sort = null) : IQuery<Result<PagedResult<PurchaseOrderDto>>>;
