using OmniManage.Modules.Inventory.Domain.Stock;

namespace OmniManage.Modules.Inventory.Domain.Abstractions;

public interface IStockLedgerRepository
{
    Task AddAsync(StockLedger entry, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<StockBalance> Balances, long TotalItems)> GetBalancesAsync(
        StockBalanceCriteria criteria,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
