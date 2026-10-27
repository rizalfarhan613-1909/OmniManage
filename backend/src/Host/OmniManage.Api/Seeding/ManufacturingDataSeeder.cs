using Microsoft.EntityFrameworkCore;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Manufacturing.Domain.BillOfMaterials;
using OmniManage.Modules.Manufacturing.Domain.WorkOrders;
using OmniManage.Modules.Manufacturing.Infrastructure.Persistence;

namespace OmniManage.Api.Seeding;

/// <summary>
/// Seeds an active bill of materials per stock-tracked product and work orders
/// released across the current and two prior months.
/// </summary>
internal static class ManufacturingDataSeeder
{
    private const int HistoryMonths = 3;
    private const int WorkOrdersPerBomPerMonth = 2;

    public static async Task SeedAsync(
        ManufacturingDbContext context,
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

        var products = items.Where(item => item.IsStockTracked).ToArray();

        if (products.Length == 0 || items.Count < 2)
        {
            return;
        }

        var existingBomCodes = await context.BillOfMaterials
            .Select(billOfMaterial => billOfMaterial.Code)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var knownBomCodes = existingBomCodes.ToHashSet(StringComparer.Ordinal);

        foreach (var product in products)
        {
            var bomCode = $"BOM-{product.Sku}-V1";

            if (!knownBomCodes.Add(bomCode))
            {
                continue;
            }

            var components = items
                .Where(item => item.Id != product.Id)
                .Take(2)
                .Select(item => new BillOfMaterialComponentInput(
                    guidGenerator.NewGuid(),
                    item.Id,
                    item.IsStockTracked ? 2m : 1m))
                .ToArray();

            if (components.Length == 0)
            {
                continue;
            }

            context.BillOfMaterials.Add(EnsureSuccess(BillOfMaterial.Create(
                guidGenerator.NewGuid(),
                tenantId,
                bomCode,
                $"{product.Name} standard build",
                product.Id,
                version: 1,
                "Demo standard build of materials",
                components,
                now,
                createdBy)));
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var boms = await context.BillOfMaterials
            .AsNoTracking()
            .Where(billOfMaterial => billOfMaterial.IsActive)
            .OrderBy(billOfMaterial => billOfMaterial.Code)
            .Select(billOfMaterial => new
            {
                billOfMaterial.Id,
                billOfMaterial.Code,
                billOfMaterial.ProductItemId,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (boms.Count == 0)
        {
            return;
        }

        var existingOrderNumbers = await context.WorkOrders
            .Select(order => order.OrderNumber)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var knownOrderNumbers = existingOrderNumbers.ToHashSet(StringComparer.Ordinal);

        for (var monthOffset = HistoryMonths - 1; monthOffset >= 0; monthOffset--)
        {
            for (var bomIndex = 0; bomIndex < boms.Count; bomIndex++)
            {
                for (var index = 0; index < WorkOrdersPerBomPerMonth; index++)
                {
                    var createdAt = BuildCreatedAt(now, monthOffset, index);
                    var orderNumber =
                        $"WO-{createdAt:yyyyMM}-{(bomIndex + 1):D2}{(index + 1):D2}";

                    if (!knownOrderNumbers.Add(orderNumber))
                    {
                        continue;
                    }

                    var bom = boms[bomIndex];
                    var quantity = 5m + (monthOffset * 5m) + (index * 2m);
                    var dueDate = DateOnly
                        .FromDateTime(createdAt.UtcDateTime)
                        .AddDays(7 + (index * 3));

                    context.WorkOrders.Add(EnsureSuccess(WorkOrder.Create(
                        guidGenerator.NewGuid(),
                        tenantId,
                        orderNumber,
                        bom.ProductItemId,
                        bom.Id,
                        bom.Code,
                        quantity,
                        dueDate,
                        "Demo production run",
                        createdAt,
                        createdBy)));
                }
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
            var daysBack = Math.Min(index * 4, Math.Max(0, now.Day - 1));
            var candidate = new DateTimeOffset(now.UtcDateTime.Date, TimeSpan.Zero)
                .AddDays(-daysBack)
                .AddHours(8 + index);

            return candidate > now ? now.AddMinutes(-25 - (index * 10)) : candidate;
        }

        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero)
            .AddMonths(-monthOffset);

        return monthStart.AddDays(4 + (index * 9)).AddHours(8 + index);
    }

    private static T EnsureSuccess<T>(Result<T> result)
    {
        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                $"The manufacturing demo seed could not be created: {result.Error.Code}.");
        }

        return result.Value;
    }
}
