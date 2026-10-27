using Microsoft.EntityFrameworkCore;
using OmniManage.BuildingBlocks.Application;
using OmniManage.Modules.Sales.Domain.Orders;
using OmniManage.Modules.Sales.Infrastructure.Persistence;

namespace OmniManage.Api.Seeding;

/// <summary>
/// Seeds a six-month sales history. Roughly three out of every four orders are
/// confirmed so the executive dashboard has revenue for the current and prior
/// month; order numbers are stable to keep the seed idempotent.
/// </summary>
internal static class SalesDataSeeder
{
    private const int HistoryMonths = 6;
    private const int OrdersPerMonth = 4;

    public static async Task<IReadOnlyList<SeedSalesOrder>> SeedAsync(
        SalesDbContext context,
        Guid tenantId,
        Guid createdBy,
        IReadOnlyList<Guid> customerIds,
        IReadOnlyList<SeedItem> items,
        IGuidGenerator guidGenerator,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(customerIds);
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(guidGenerator);

        if (customerIds.Count == 0)
        {
            throw new InvalidOperationException(
                "The sales demo seed requires at least one customer.");
        }

        if (items.Count == 0)
        {
            throw new InvalidOperationException(
                "The sales demo seed requires at least one catalogue item.");
        }

        var existingOrderNumbers = await context.SalesOrders
            .Select(order => order.OrderNumber)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var knownOrderNumbers = existingOrderNumbers.ToHashSet(StringComparer.Ordinal);

        for (var monthOffset = HistoryMonths - 1; monthOffset >= 0; monthOffset--)
        {
            for (var index = 0; index < OrdersPerMonth; index++)
            {
                var createdAt = BuildCreatedAt(now, monthOffset, index);
                var orderNumber = $"SO-{createdAt:yyyyMM}-{index + 1:D2}";

                if (!knownOrderNumbers.Add(orderNumber))
                {
                    continue;
                }

                var customerId = customerIds[(monthOffset + index) % customerIds.Count];
                var item = items[(monthOffset + index) % items.Count];
                var quantity = 1m + ((monthOffset + index) % 5);
                var line = new SalesOrderLineInput(
                    guidGenerator.NewGuid(),
                    item.Id,
                    quantity,
                    item.BasePrice);

                var order = EnsureSuccess(SalesOrder.Create(
                    guidGenerator.NewGuid(),
                    tenantId,
                    orderNumber,
                    customerId,
                    [line],
                    createdAt,
                    createdBy));

                var isConfirmed = ((monthOffset + index) % OrdersPerMonth) != OrdersPerMonth - 1;

                if (isConfirmed)
                {
                    EnsureSuccess(order.Confirm(
                        guidGenerator.NewGuid(),
                        createdAt.AddMinutes(45),
                        createdBy));
                }

                context.SalesOrders.Add(order);
            }
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var seededOrders = await context.SalesOrders
            .AsNoTracking()
            .Where(order => order.OrderNumber.StartsWith("SO-")
                && order.CreatedAt >= now.AddMonths(-(HistoryMonths + 1)))
            .OrderBy(order => order.CreatedAt)
            .Select(order => new
            {
                order.Id,
                order.CustomerId,
                order.OrderNumber,
                order.TotalAmount,
                order.CreatedAt,
                order.Status,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return seededOrders
            .Select(order => new SeedSalesOrder(
                order.Id,
                order.CustomerId,
                order.OrderNumber,
                order.TotalAmount,
                order.CreatedAt,
                order.Status == SalesOrderStatus.Confirmed))
            .ToArray();
    }

    private static DateTimeOffset BuildCreatedAt(
        DateTimeOffset now,
        int monthOffset,
        int index)
    {
        if (monthOffset == 0)
        {
            var daysBack = Math.Min(index * 2, Math.Max(0, now.Day - 1));
            var candidate = new DateTimeOffset(now.UtcDateTime.Date, TimeSpan.Zero)
                .AddDays(-daysBack)
                .AddHours(9 + index);

            return candidate > now ? now.AddMinutes(-30 - (index * 10)) : candidate;
        }

        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero)
            .AddMonths(-monthOffset);

        return monthStart.AddDays(3 + (index * 6)).AddHours(9 + index);
    }

    private static T EnsureSuccess<T>(OmniManage.BuildingBlocks.Domain.Result<T> result)
    {
        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                $"The sales demo seed could not be created: {result.Error.Code}.");
        }

        return result.Value;
    }

    private static void EnsureSuccess(OmniManage.BuildingBlocks.Domain.Result result)
    {
        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                $"The sales demo seed could not be created: {result.Error.Code}.");
        }
    }
}
