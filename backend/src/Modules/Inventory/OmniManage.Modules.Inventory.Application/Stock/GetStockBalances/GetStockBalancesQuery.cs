using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Inventory.Application.Common;

namespace OmniManage.Modules.Inventory.Application.Stock.GetStockBalances;

public sealed record GetStockBalancesQuery(
    int Page = 1,
    int PageSize = Pagination.DefaultPageSize,
    Guid? WarehouseId = null,
    Guid? ItemId = null) : IQuery<Result<PagedResult<StockBalanceDto>>>;
