using Microsoft.EntityFrameworkCore;
using OmniManage.Modules.Sales.Domain.Abstractions;
using OmniManage.Modules.Sales.Domain.Orders;

namespace OmniManage.Modules.Sales.Infrastructure.Persistence.Repositories;

public sealed class SalesOrderRepository(SalesDbContext dbContext) : ISalesOrderRepository
{
    public Task AddAsync(SalesOrder salesOrder, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(salesOrder);

        return dbContext.SalesOrders.AddAsync(salesOrder, cancellationToken).AsTask();
    }

    public Task<bool> OrderNumberExistsAsync(
        string orderNumber,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderNumber);

        return dbContext.SalesOrders.AnyAsync(
            salesOrder => salesOrder.OrderNumber == orderNumber,
            cancellationToken);
    }

    public Task<SalesOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.SalesOrders
            .AsNoTracking()
            .Include(salesOrder => salesOrder.Lines)
            .FirstOrDefaultAsync(salesOrder => salesOrder.Id == id, cancellationToken);

    public Task<SalesOrder?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.SalesOrders
            .Include(salesOrder => salesOrder.Lines)
            .FirstOrDefaultAsync(salesOrder => salesOrder.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<SalesOrder> SalesOrders, long TotalItems)> ListAsync(
        SalesOrderListCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        IQueryable<SalesOrder> query = dbContext.SalesOrders
            .AsNoTracking()
            .Include(salesOrder => salesOrder.Lines);

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            var term = criteria.Search.Trim().ToLowerInvariant();

            query = query.Where(salesOrder =>
                salesOrder.OrderNumber.ToLower().Contains(term));
        }

        if (criteria.Status is { } status)
        {
            query = query.Where(salesOrder => salesOrder.Status == status);
        }

        var totalItems = await query.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var orders = await ApplySort(query, criteria)
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return (orders, totalItems);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);

    private static IQueryable<SalesOrder> ApplySort(
        IQueryable<SalesOrder> query,
        SalesOrderListCriteria criteria)
    {
        var option = criteria.Sort.Count == 0
            ? new SalesOrderSortOption(SalesOrderSortField.CreatedAt, true)
            : criteria.Sort[0];

        IOrderedQueryable<SalesOrder> ordered = option.Field switch
        {
            SalesOrderSortField.OrderNumber => option.Descending
                ? query.OrderByDescending(salesOrder => salesOrder.OrderNumber)
                : query.OrderBy(salesOrder => salesOrder.OrderNumber),
            SalesOrderSortField.Status => option.Descending
                ? query.OrderByDescending(salesOrder => salesOrder.Status)
                : query.OrderBy(salesOrder => salesOrder.Status),
            SalesOrderSortField.TotalAmount => option.Descending
                ? query.OrderByDescending(salesOrder => salesOrder.TotalAmount)
                : query.OrderBy(salesOrder => salesOrder.TotalAmount),
            SalesOrderSortField.CreatedAt => option.Descending
                ? query.OrderByDescending(salesOrder => salesOrder.CreatedAt)
                : query.OrderBy(salesOrder => salesOrder.CreatedAt),
            _ => query.OrderByDescending(salesOrder => salesOrder.CreatedAt),
        };

        return ordered.ThenBy(salesOrder => salesOrder.Id);
    }
}
