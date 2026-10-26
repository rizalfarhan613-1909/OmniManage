using Microsoft.EntityFrameworkCore;
using OmniManage.Modules.Manufacturing.Domain.Abstractions;
using OmniManage.Modules.Manufacturing.Domain.WorkOrders;

namespace OmniManage.Modules.Manufacturing.Infrastructure.Persistence.Repositories;

public sealed class WorkOrderRepository(ManufacturingDbContext dbContext) : IWorkOrderRepository
{
    public Task AddAsync(WorkOrder workOrder, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workOrder);

        return dbContext.WorkOrders.AddAsync(workOrder, cancellationToken).AsTask();
    }

    public Task<bool> OrderNumberExistsAsync(
        string orderNumber,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderNumber);

        return dbContext.WorkOrders.AnyAsync(
            workOrder => workOrder.OrderNumber == orderNumber,
            cancellationToken);
    }

    public Task<WorkOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.WorkOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(workOrder => workOrder.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<WorkOrder> WorkOrders, long TotalItems)> ListAsync(
        WorkOrderListCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        IQueryable<WorkOrder> query = dbContext.WorkOrders.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            var term = criteria.Search.Trim().ToLowerInvariant();

            query = query.Where(workOrder =>
                workOrder.OrderNumber.ToLower().Contains(term)
                || workOrder.BillOfMaterialCode.ToLower().Contains(term));
        }

        if (criteria.Status is { } status)
        {
            query = query.Where(workOrder => workOrder.Status == status);
        }

        if (criteria.ProductItemId is { } productItemId)
        {
            query = query.Where(workOrder => workOrder.ProductItemId == productItemId);
        }

        if (criteria.BillOfMaterialId is { } billOfMaterialId)
        {
            query = query.Where(workOrder => workOrder.BillOfMaterialId == billOfMaterialId);
        }

        var totalItems = await query.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var workOrders = await ApplySort(query, criteria)
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return (workOrders, totalItems);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);

    private static IQueryable<WorkOrder> ApplySort(
        IQueryable<WorkOrder> query,
        WorkOrderListCriteria criteria)
    {
        var option = criteria.Sort.Count == 0
            ? new WorkOrderSortOption(WorkOrderSortField.CreatedAt, true)
            : criteria.Sort[0];

        IOrderedQueryable<WorkOrder> ordered = option.Field switch
        {
            WorkOrderSortField.OrderNumber => option.Descending
                ? query.OrderByDescending(workOrder => workOrder.OrderNumber)
                : query.OrderBy(workOrder => workOrder.OrderNumber),
            WorkOrderSortField.Status => option.Descending
                ? query.OrderByDescending(workOrder => workOrder.Status)
                : query.OrderBy(workOrder => workOrder.Status),
            WorkOrderSortField.ProductItemId => option.Descending
                ? query.OrderByDescending(workOrder => workOrder.ProductItemId)
                : query.OrderBy(workOrder => workOrder.ProductItemId),
            WorkOrderSortField.QuantityPlanned => option.Descending
                ? query.OrderByDescending(workOrder => workOrder.QuantityPlanned)
                : query.OrderBy(workOrder => workOrder.QuantityPlanned),
            WorkOrderSortField.DueDate => option.Descending
                ? query.OrderByDescending(workOrder => workOrder.DueDate)
                : query.OrderBy(workOrder => workOrder.DueDate),
            WorkOrderSortField.CreatedAt => option.Descending
                ? query.OrderByDescending(workOrder => workOrder.CreatedAt)
                : query.OrderBy(workOrder => workOrder.CreatedAt),
            _ => query.OrderByDescending(workOrder => workOrder.CreatedAt),
        };

        return ordered.ThenBy(workOrder => workOrder.Id);
    }
}
