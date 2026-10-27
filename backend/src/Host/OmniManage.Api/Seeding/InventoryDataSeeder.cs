using Microsoft.EntityFrameworkCore;
using OmniManage.BuildingBlocks.Application;
using OmniManage.Modules.Inventory.Domain.Stock;
using OmniManage.Modules.Inventory.Domain.Warehouses;
using OmniManage.Modules.Inventory.Infrastructure.Persistence;

namespace OmniManage.Api.Seeding;

/// <summary>
/// Seeds the warehouse master data and a six-month stock ledger so inventory
/// balances and the executive stock-value metric are non-zero. Ledger entries
/// use stable reference numbers and are only appended when they are missing.
/// </summary>
internal static class InventoryDataSeeder
{
    private const int HistoryMonths = 6;

    private static readonly WarehouseSeed[] Warehouses =
    [
        new("WH-01", "Main Distribution Warehouse", "1 Logistics Way, Jakarta"),
        new("WH-02", "West Jakarta Fulfilment Hub", "88 Artery Road, Jakarta"),
        new("WH-03", "Surabaya Regional Depot", "12 Harbour Avenue, Surabaya"),
    ];

    public static async Task<IReadOnlyList<SeedWarehouse>> SeedAsync(
        InventoryDbContext context,
        Guid tenantId,
        Guid createdBy,
        IReadOnlyList<SeedItem> items,
        IGuidGenerator guidGenerator,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(guidGenerator);

        var warehouseCodes = Warehouses.Select(warehouse => warehouse.Code).ToArray();
        var existingWarehouseCodes = await context.Warehouses
            .Select(warehouse => warehouse.Code)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var knownWarehouseCodes = existingWarehouseCodes.ToHashSet(StringComparer.Ordinal);

        foreach (var warehouse in Warehouses)
        {
            if (knownWarehouseCodes.Contains(warehouse.Code))
            {
                continue;
            }

            context.Warehouses.Add(EnsureSuccess(Warehouse.Create(
                guidGenerator.NewGuid(),
                tenantId,
                warehouse.Code,
                warehouse.Name,
                warehouse.Address,
                now,
                createdBy)));
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var seededWarehouses = await context.Warehouses
            .AsNoTracking()
            .Where(warehouse => warehouseCodes.Contains(warehouse.Code))
            .OrderBy(warehouse => warehouse.Code)
            .Select(warehouse => new SeedWarehouse(warehouse.Id, warehouse.Code, warehouse.Name))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var stockItems = items.Where(item => item.IsStockTracked).ToArray();
        var existingReferences = await context.StockLedgerEntries
            .Where(entry => entry.ReferenceNumber.StartsWith("SEED-"))
            .Select(entry => entry.ReferenceNumber)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var knownReferences = existingReferences.ToHashSet(StringComparer.Ordinal);

        for (var monthOffset = HistoryMonths - 1; monthOffset >= 0; monthOffset--)
        {
            foreach (var warehouse in seededWarehouses)
            {
                var warehouseIndex = Array.FindIndex(
                    Warehouses,
                    candidate => string.Equals(candidate.Code, warehouse.Code, StringComparison.Ordinal));

                foreach (var item in stockItems)
                {
                    var receiveReference = BuildReference(
                        "RECEIVE",
                        item.Sku,
                        warehouse.Code,
                        now.AddMonths(-monthOffset));

                    if (knownReferences.Add(receiveReference))
                    {
                        var receivedQuantity = 30m + (warehouseIndex * 10m) + (monthOffset * 3m);

                        context.StockLedgerEntries.Add(EnsureSuccess(StockLedger.Create(
                            guidGenerator.NewGuid(),
                            tenantId,
                            warehouse.Id,
                            item.Id,
                            receivedQuantity,
                            MovementType.Receive.ToString(),
                            receiveReference,
                            MonthOccurredAt(now, monthOffset, day: 2 + warehouseIndex, hour: 8))));
                    }

                    var issueReference = BuildReference(
                        "ISSUE",
                        item.Sku,
                        warehouse.Code,
                        now.AddMonths(-monthOffset));

                    if (knownReferences.Add(issueReference))
                    {
                        var issuedQuantity = -(5m + (warehouseIndex * 2m) + monthOffset);

                        context.StockLedgerEntries.Add(EnsureSuccess(StockLedger.Create(
                            guidGenerator.NewGuid(),
                            tenantId,
                            warehouse.Id,
                            item.Id,
                            issuedQuantity,
                            MovementType.Issue.ToString(),
                            issueReference,
                            MonthOccurredAt(now, monthOffset, day: 18 + warehouseIndex, hour: 15))));
                    }
                }
            }
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return seededWarehouses;
    }

    private static DateTimeOffset MonthOccurredAt(
        DateTimeOffset now,
        int monthOffset,
        int day,
        int hour)
    {
        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero)
            .AddMonths(-monthOffset);
        var candidate = monthStart.AddDays(day - 1).AddHours(hour);

        return candidate > now ? now.AddMinutes(-15 - day) : candidate;
    }

    private static string BuildReference(
        string movement,
        string sku,
        string warehouseCode,
        DateTimeOffset occurredAt) =>
        $"SEED-{movement}-{sku}-{warehouseCode}-{occurredAt:yyyyMM}";

    private static T EnsureSuccess<T>(OmniManage.BuildingBlocks.Domain.Result<T> result)
    {
        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                $"The inventory demo seed could not be created: {result.Error.Code}.");
        }

        return result.Value;
    }

    private sealed record WarehouseSeed(string Code, string Name, string Address);
}
