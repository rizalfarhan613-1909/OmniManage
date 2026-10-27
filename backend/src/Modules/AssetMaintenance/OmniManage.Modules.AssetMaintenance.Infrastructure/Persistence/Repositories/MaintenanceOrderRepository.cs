using Microsoft.EntityFrameworkCore;
using OmniManage.Modules.AssetMaintenance.Domain.Abstractions;
using OmniManage.Modules.AssetMaintenance.Domain.Maintenance;
using OmniManage.Modules.AssetMaintenance.Infrastructure.Persistence;

namespace OmniManage.Modules.AssetMaintenance.Infrastructure.Persistence.Repositories;

public sealed class MaintenanceOrderRepository(AssetMaintenanceDbContext dbContext)
    : IMaintenanceOrderRepository
{
    public Task AddAsync(
        MaintenanceOrder maintenanceOrder,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(maintenanceOrder);

        return dbContext.MaintenanceOrders.AddAsync(maintenanceOrder, cancellationToken).AsTask();
    }

    public Task<bool> RequestNumberExistsAsync(
        string requestNumber,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestNumber);

        return dbContext.MaintenanceOrders.AnyAsync(
            maintenanceOrder => maintenanceOrder.RequestNumber == requestNumber,
            cancellationToken);
    }

    public async Task<(IReadOnlyList<MaintenanceOrder> Orders, long TotalItems)> ListAsync(
        MaintenanceOrderListCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        IQueryable<MaintenanceOrder> query = dbContext.MaintenanceOrders.AsNoTracking();

        if (criteria.AssetId is { } assetId)
        {
            query = query.Where(order => order.AssetId == assetId);
        }

        if (criteria.Status is { } status)
        {
            query = query.Where(order => order.Status == status);
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

    private static IQueryable<MaintenanceOrder> ApplySort(
        IQueryable<MaintenanceOrder> query,
        MaintenanceOrderListCriteria criteria)
    {
        var option = criteria.Sort.Count == 0
            ? new MaintenanceOrderSortOption(MaintenanceOrderSortField.ScheduledDate, true)
            : criteria.Sort[0];

        IOrderedQueryable<MaintenanceOrder> ordered = option.Field switch
        {
            MaintenanceOrderSortField.RequestNumber => option.Descending
                ? query.OrderByDescending(order => order.RequestNumber)
                : query.OrderBy(order => order.RequestNumber),
            MaintenanceOrderSortField.Cost => option.Descending
                ? query.OrderByDescending(order => order.Cost)
                : query.OrderBy(order => order.Cost),
            MaintenanceOrderSortField.ScheduledDate => option.Descending
                ? query.OrderByDescending(order => order.ScheduledDate)
                : query.OrderBy(order => order.ScheduledDate),
            _ => query.OrderByDescending(order => order.ScheduledDate),
        };

        return ordered.ThenBy(order => order.Id);
    }
}
