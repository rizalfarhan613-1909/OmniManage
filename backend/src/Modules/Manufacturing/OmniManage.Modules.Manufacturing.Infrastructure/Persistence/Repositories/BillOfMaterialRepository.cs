using Microsoft.EntityFrameworkCore;
using OmniManage.Modules.Manufacturing.Domain.Abstractions;
using OmniManage.Modules.Manufacturing.Domain.BillOfMaterials;

namespace OmniManage.Modules.Manufacturing.Infrastructure.Persistence.Repositories;

public sealed class BillOfMaterialRepository(ManufacturingDbContext dbContext)
    : IBillOfMaterialRepository
{
    public Task AddAsync(BillOfMaterial billOfMaterial, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(billOfMaterial);

        return dbContext.BillOfMaterials.AddAsync(billOfMaterial, cancellationToken).AsTask();
    }

    public Task<bool> CodeExistsAsync(
        string code,
        int version,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        return dbContext.BillOfMaterials.AnyAsync(
            billOfMaterial => billOfMaterial.Code == code && billOfMaterial.Version == version,
            cancellationToken);
    }

    public Task<BillOfMaterial?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.BillOfMaterials
            .AsNoTracking()
            .Include(billOfMaterial => billOfMaterial.Components)
            .FirstOrDefaultAsync(billOfMaterial => billOfMaterial.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<BillOfMaterial> BillOfMaterials, long TotalItems)> ListAsync(
        BillOfMaterialListCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        IQueryable<BillOfMaterial> query = dbContext.BillOfMaterials
            .AsNoTracking()
            .Include(billOfMaterial => billOfMaterial.Components);

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            var term = criteria.Search.Trim().ToLowerInvariant();

            query = query.Where(billOfMaterial =>
                billOfMaterial.Code.ToLower().Contains(term)
                || billOfMaterial.Name.ToLower().Contains(term));
        }

        if (criteria.ProductItemId is { } productItemId)
        {
            query = query.Where(billOfMaterial => billOfMaterial.ProductItemId == productItemId);
        }

        if (criteria.IsActive is { } isActive)
        {
            query = query.Where(billOfMaterial => billOfMaterial.IsActive == isActive);
        }

        var totalItems = await query.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var billOfMaterials = await ApplySort(query, criteria)
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return (billOfMaterials, totalItems);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);

    private static IQueryable<BillOfMaterial> ApplySort(
        IQueryable<BillOfMaterial> query,
        BillOfMaterialListCriteria criteria)
    {
        var option = criteria.Sort.Count == 0
            ? new BillOfMaterialSortOption(BillOfMaterialSortField.Code, false)
            : criteria.Sort[0];

        IOrderedQueryable<BillOfMaterial> ordered = option.Field switch
        {
            BillOfMaterialSortField.Code => option.Descending
                ? query.OrderByDescending(billOfMaterial => billOfMaterial.Code)
                : query.OrderBy(billOfMaterial => billOfMaterial.Code),
            BillOfMaterialSortField.Name => option.Descending
                ? query.OrderByDescending(billOfMaterial => billOfMaterial.Name)
                : query.OrderBy(billOfMaterial => billOfMaterial.Name),
            BillOfMaterialSortField.Version => option.Descending
                ? query.OrderByDescending(billOfMaterial => billOfMaterial.Version)
                : query.OrderBy(billOfMaterial => billOfMaterial.Version),
            BillOfMaterialSortField.IsActive => option.Descending
                ? query.OrderByDescending(billOfMaterial => billOfMaterial.IsActive)
                : query.OrderBy(billOfMaterial => billOfMaterial.IsActive),
            BillOfMaterialSortField.CreatedAt => option.Descending
                ? query.OrderByDescending(billOfMaterial => billOfMaterial.CreatedAt)
                : query.OrderBy(billOfMaterial => billOfMaterial.CreatedAt),
            _ => query.OrderBy(billOfMaterial => billOfMaterial.Code),
        };

        return ordered.ThenBy(billOfMaterial => billOfMaterial.Id);
    }
}
