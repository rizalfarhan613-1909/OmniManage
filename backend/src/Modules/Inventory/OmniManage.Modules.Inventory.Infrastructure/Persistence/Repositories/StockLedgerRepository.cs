using Microsoft.EntityFrameworkCore;
using OmniManage.Modules.Inventory.Domain.Abstractions;
using OmniManage.Modules.Inventory.Domain.Stock;
using OmniManage.Modules.Inventory.Infrastructure.Persistence;

namespace OmniManage.Modules.Inventory.Infrastructure.Persistence.Repositories;

public sealed class StockLedgerRepository(InventoryDbContext dbContext) : IStockLedgerRepository
{
    public Task AddAsync(StockLedger entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        return dbContext.StockLedgerEntries.AddAsync(entry, cancellationToken).AsTask();
    }

    public async Task<(IReadOnlyList<StockBalance> Balances, long TotalItems)> GetBalancesAsync(
        StockBalanceCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        IQueryable<StockLedger> query = dbContext.StockLedgerEntries.AsNoTracking();

        if (criteria.WarehouseId is { } warehouseId)
        {
            query = query.Where(entry => entry.WarehouseId == warehouseId);
        }

        if (criteria.ItemId is { } itemId)
        {
            query = query.Where(entry => entry.ItemId == itemId);
        }

        var grouped = query
            .GroupBy(entry => new { entry.WarehouseId, entry.ItemId })
            .Select(group => new
            {
                group.Key.WarehouseId,
                group.Key.ItemId,
                QuantityOnHand = group.Sum(entry => entry.QuantityDelta),
            });

        var totalItems = await grouped.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var rows = await grouped
            .OrderBy(row => row.WarehouseId)
            .ThenBy(row => row.ItemId)
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var balances = rows
            .Select(row => new StockBalance(row.ItemId, row.WarehouseId, row.QuantityOnHand))
            .ToArray();

        return (balances, totalItems);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
