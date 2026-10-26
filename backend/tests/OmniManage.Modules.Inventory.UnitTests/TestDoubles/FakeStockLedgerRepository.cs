using OmniManage.Modules.Inventory.Domain.Abstractions;
using OmniManage.Modules.Inventory.Domain.Stock;

namespace OmniManage.Modules.Inventory.UnitTests.TestDoubles;

internal sealed class FakeStockLedgerRepository : IStockLedgerRepository
{
    public List<StockLedger> Entries { get; } = [];

    public List<StockBalance> Balances { get; } = [];

    public StockBalanceCriteria? LastCriteria { get; private set; }

    public int SaveCount { get; private set; }

    public Task AddAsync(StockLedger entry, CancellationToken cancellationToken = default)
    {
        Entries.Add(entry);
        return Task.CompletedTask;
    }

    public Task<(IReadOnlyList<StockBalance> Balances, long TotalItems)> GetBalancesAsync(
        StockBalanceCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        LastCriteria = criteria;

        IReadOnlyList<StockBalance> balances = Balances
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .ToArray();

        return Task.FromResult((balances, (long)Balances.Count));
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;
        return Task.CompletedTask;
    }
}
