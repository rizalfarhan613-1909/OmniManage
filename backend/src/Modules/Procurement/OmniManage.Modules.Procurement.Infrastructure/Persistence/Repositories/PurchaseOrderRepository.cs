using Microsoft.EntityFrameworkCore;
using OmniManage.Modules.Procurement.Domain.Abstractions;
using OmniManage.Modules.Procurement.Domain.PurchaseOrders;

namespace OmniManage.Modules.Procurement.Infrastructure.Persistence.Repositories;

public sealed class PurchaseOrderRepository(ProcurementDbContext dbContext) : IPurchaseOrderRepository
{
    public Task AddAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(purchaseOrder);

        return dbContext.PurchaseOrders.AddAsync(purchaseOrder, cancellationToken).AsTask();
    }

    public Task<bool> OrderNumberExistsAsync(
        string orderNumber,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderNumber);

        return dbContext.PurchaseOrders.AnyAsync(
            purchaseOrder => purchaseOrder.OrderNumber == orderNumber,
            cancellationToken);
    }

    public Task<PurchaseOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.PurchaseOrders
            .AsNoTracking()
            .Include(purchaseOrder => purchaseOrder.Lines)
            .FirstOrDefaultAsync(purchaseOrder => purchaseOrder.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<PurchaseOrder> PurchaseOrders, long TotalItems)> ListAsync(
        PurchaseOrderListCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        IQueryable<PurchaseOrder> query = dbContext.PurchaseOrders
            .AsNoTracking()
            .Include(purchaseOrder => purchaseOrder.Lines);

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            var term = criteria.Search.Trim().ToLowerInvariant();

            query = query.Where(purchaseOrder =>
                purchaseOrder.OrderNumber.ToLower().Contains(term)
                || purchaseOrder.SupplierName.ToLower().Contains(term));
        }

        if (criteria.Status is { } status)
        {
            query = query.Where(purchaseOrder => purchaseOrder.Status == status);
        }

        if (criteria.SupplierId is { } supplierId)
        {
            query = query.Where(purchaseOrder => purchaseOrder.SupplierId == supplierId);
        }

        var totalItems = await query.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var purchaseOrders = await ApplySort(query, criteria)
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return (purchaseOrders, totalItems);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);

    private static IQueryable<PurchaseOrder> ApplySort(
        IQueryable<PurchaseOrder> query,
        PurchaseOrderListCriteria criteria)
    {
        var option = criteria.Sort.Count == 0
            ? new PurchaseOrderSortOption(PurchaseOrderSortField.CreatedAt, true)
            : criteria.Sort[0];

        IOrderedQueryable<PurchaseOrder> ordered = option.Field switch
        {
            PurchaseOrderSortField.OrderNumber => option.Descending
                ? query.OrderByDescending(purchaseOrder => purchaseOrder.OrderNumber)
                : query.OrderBy(purchaseOrder => purchaseOrder.OrderNumber),
            PurchaseOrderSortField.SupplierName => option.Descending
                ? query.OrderByDescending(purchaseOrder => purchaseOrder.SupplierName)
                : query.OrderBy(purchaseOrder => purchaseOrder.SupplierName),
            PurchaseOrderSortField.Status => option.Descending
                ? query.OrderByDescending(purchaseOrder => purchaseOrder.Status)
                : query.OrderBy(purchaseOrder => purchaseOrder.Status),
            PurchaseOrderSortField.TotalAmount => option.Descending
                ? query.OrderByDescending(purchaseOrder => purchaseOrder.TotalAmount)
                : query.OrderBy(purchaseOrder => purchaseOrder.TotalAmount),
            PurchaseOrderSortField.ExpectedDeliveryDate => option.Descending
                ? query.OrderByDescending(purchaseOrder => purchaseOrder.ExpectedDeliveryDate)
                : query.OrderBy(purchaseOrder => purchaseOrder.ExpectedDeliveryDate),
            PurchaseOrderSortField.CreatedAt => option.Descending
                ? query.OrderByDescending(purchaseOrder => purchaseOrder.CreatedAt)
                : query.OrderBy(purchaseOrder => purchaseOrder.CreatedAt),
            _ => query.OrderByDescending(purchaseOrder => purchaseOrder.CreatedAt),
        };

        return ordered.ThenBy(purchaseOrder => purchaseOrder.Id);
    }
}
