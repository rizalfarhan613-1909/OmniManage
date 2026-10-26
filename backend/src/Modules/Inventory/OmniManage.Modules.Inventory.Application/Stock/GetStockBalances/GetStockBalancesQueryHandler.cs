using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Inventory.Application.Common;
using OmniManage.Modules.Inventory.Domain.Abstractions;

namespace OmniManage.Modules.Inventory.Application.Stock.GetStockBalances;

public sealed class GetStockBalancesQueryHandler(IStockLedgerRepository stockLedger)
    : IQueryHandler<GetStockBalancesQuery, Result<PagedResult<StockBalanceDto>>>
{
    public async Task<Result<PagedResult<StockBalanceDto>>> Handle(
        GetStockBalancesQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var (page, pageSize, skip) = Pagination.Normalize(query.Page, query.PageSize);
        var criteria = new StockBalanceCriteria(skip, pageSize, query.WarehouseId, query.ItemId);

        var (balances, totalItems) = await stockLedger
            .GetBalancesAsync(criteria, cancellationToken)
            .ConfigureAwait(false);

        var dtos = balances.Select(StockBalanceDto.From).ToArray();

        return new PagedResult<StockBalanceDto>(dtos, page, pageSize, totalItems);
    }
}
