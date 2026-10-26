using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Inventory.Application.Common;

namespace OmniManage.Modules.Inventory.Application.Warehouses.ListWarehouses;

public sealed record ListWarehousesQuery(
    int Page = 1,
    int PageSize = Pagination.DefaultPageSize,
    string? Search = null,
    bool? IsActive = null,
    string? Sort = null) : IQuery<Result<PagedResult<WarehouseDto>>>;
