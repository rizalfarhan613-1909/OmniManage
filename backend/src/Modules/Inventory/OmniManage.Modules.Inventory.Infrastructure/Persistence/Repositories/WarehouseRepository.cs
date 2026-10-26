using Microsoft.EntityFrameworkCore;
using OmniManage.Modules.Inventory.Domain.Abstractions;
using OmniManage.Modules.Inventory.Domain.Warehouses;
using OmniManage.Modules.Inventory.Infrastructure.Persistence;

namespace OmniManage.Modules.Inventory.Infrastructure.Persistence.Repositories;

public sealed class WarehouseRepository(InventoryDbContext dbContext) : IWarehouseRepository
{
    public Task AddAsync(Warehouse warehouse, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(warehouse);

        return dbContext.Warehouses.AddAsync(warehouse, cancellationToken).AsTask();
    }

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        return dbContext.Warehouses.AnyAsync(warehouse => warehouse.Code == code, cancellationToken);
    }

    public Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Warehouses
            .FirstOrDefaultAsync(warehouse => warehouse.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<Warehouse> Warehouses, long TotalItems)> ListAsync(
        WarehouseListCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        IQueryable<Warehouse> query = dbContext.Warehouses.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            var term = criteria.Search.Trim().ToLowerInvariant();

            query = query.Where(warehouse =>
                warehouse.Code.ToLower().Contains(term)
                || warehouse.Name.ToLower().Contains(term)
                || (warehouse.Address != null && warehouse.Address.ToLower().Contains(term)));
        }

        if (criteria.IsActive is { } isActive)
        {
            query = query.Where(warehouse => warehouse.IsActive == isActive);
        }

        var totalItems = await query.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var warehouses = await ApplySort(query, criteria)
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return (warehouses, totalItems);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);

    private static IQueryable<Warehouse> ApplySort(
        IQueryable<Warehouse> query,
        WarehouseListCriteria criteria)
    {
        var option = criteria.Sort.Count == 0
            ? new WarehouseSortOption(WarehouseSortField.Code, false)
            : criteria.Sort[0];

        IOrderedQueryable<Warehouse> ordered = option.Field switch
        {
            WarehouseSortField.Code => option.Descending
                ? query.OrderByDescending(warehouse => warehouse.Code)
                : query.OrderBy(warehouse => warehouse.Code),
            WarehouseSortField.Name => option.Descending
                ? query.OrderByDescending(warehouse => warehouse.Name)
                : query.OrderBy(warehouse => warehouse.Name),
            WarehouseSortField.CreatedAt => option.Descending
                ? query.OrderByDescending(warehouse => warehouse.CreatedAt)
                : query.OrderBy(warehouse => warehouse.CreatedAt),
            _ => query.OrderBy(warehouse => warehouse.Code),
        };

        return ordered.ThenBy(warehouse => warehouse.Id);
    }
}
