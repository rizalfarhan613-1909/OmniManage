using Microsoft.EntityFrameworkCore;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Procurement.Domain.PurchaseOrders;
using OmniManage.Modules.Procurement.Infrastructure.Persistence;

namespace OmniManage.Api.Seeding;

/// <summary>
/// Seeds purchase orders across the last three months using the suppliers and
/// catalogue items created by the master-data seed. Order numbers are stable.
/// </summary>
internal static class ProcurementDataSeeder
{
    private const int HistoryMonths = 3;
    private const int OrdersPerMonth = 4;

    public static async Task SeedAsync(
        ProcurementDbContext context,
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

        if (items.Count == 0)
        {
            throw new InvalidOperationException(
                "The procurement demo seed requires at least one catalogue item.");
        }

        var suppliers = await context.Suppliers
            .AsNoTracking()
            .OrderBy(supplier => supplier.Code)
            .Select(supplier => new { supplier.Id, supplier.Name })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (suppliers.Count == 0)
        {
            throw new InvalidOperationException(
                "The procurement demo seed requires at least one supplier.");
        }

        var purchaseItems = items.Where(item => item.CostPrice > 0m).ToArray();

        if (purchaseItems.Length == 0)
        {
            purchaseItems = items.ToArray();
        }

        var existingOrderNumbers = await context.PurchaseOrders
            .Select(order => order.OrderNumber)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var knownOrderNumbers = existingOrderNumbers.ToHashSet(StringComparer.Ordinal);

        for (var monthOffset = HistoryMonths - 1; monthOffset >= 0; monthOffset--)
        {
            for (var index = 0; index < OrdersPerMonth; index++)
            {
                var createdAt = BuildCreatedAt(now, monthOffset, index);
                var orderNumber = $"PO-{createdAt:yyyyMM}-{index + 1:D2}";

                if (!knownOrderNumbers.Add(orderNumber))
                {
                    continue;
                }

                var supplier = suppliers[(monthOffset + index) % suppliers.Count];
                var item = purchaseItems[(monthOffset + index) % purchaseItems.Length];
                var quantity = 10m + ((monthOffset + index) % 4 * 5m);
                var line = new PurchaseOrderLineInput(
                    guidGenerator.NewGuid(),
                    item.Id,
                    quantity,
                    item.CostPrice);

                var order = EnsureSuccess(PurchaseOrder.Create(
                    guidGenerator.NewGuid(),
                    tenantId,
                    orderNumber,
                    supplier.Id,
                    supplier.Name,
                    DateOnly.FromDateTime(createdAt.UtcDateTime).AddDays(14),
                    $"Replenishment for {item.Name}",
                    [line],
                    createdAt,
                    createdBy));

                context.PurchaseOrders.Add(order);
            }
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static DateTimeOffset BuildCreatedAt(
        DateTimeOffset now,
        int monthOffset,
        int index)
    {
        if (monthOffset == 0)
        {
            var daysBack = Math.Min(index * 3, Math.Max(0, now.Day - 1));
            var candidate = new DateTimeOffset(now.UtcDateTime.Date, TimeSpan.Zero)
                .AddDays(-daysBack)
                .AddHours(10 + index);

            return candidate > now ? now.AddMinutes(-20 - (index * 10)) : candidate;
        }

        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero)
            .AddMonths(-monthOffset);

        return monthStart.AddDays(2 + (index * 7)).AddHours(10 + index);
    }

    private static T EnsureSuccess<T>(Result<T> result)
    {
        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                $"The procurement demo seed could not be created: {result.Error.Code}.");
        }

        return result.Value;
    }
}
