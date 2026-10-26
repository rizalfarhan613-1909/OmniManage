using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Sales.Application.Common;

namespace OmniManage.Modules.Sales.Application.Orders.ListSalesOrders;

public sealed record ListSalesOrdersQuery(
    int Page = 1,
    int PageSize = Pagination.DefaultPageSize,
    string? Search = null,
    string? Status = null,
    string? Sort = null) : IQuery<Result<PagedResult<SalesOrderDto>>>;
