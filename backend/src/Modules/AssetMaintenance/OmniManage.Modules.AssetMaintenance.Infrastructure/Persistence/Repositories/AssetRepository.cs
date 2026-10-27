using Microsoft.EntityFrameworkCore;
using OmniManage.Modules.AssetMaintenance.Domain.Abstractions;
using OmniManage.Modules.AssetMaintenance.Domain.Assets;
using OmniManage.Modules.AssetMaintenance.Infrastructure.Persistence;

namespace OmniManage.Modules.AssetMaintenance.Infrastructure.Persistence.Repositories;

public sealed class AssetRepository(AssetMaintenanceDbContext dbContext) : IAssetRepository
{
    public Task AddAsync(Asset asset, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(asset);

        return dbContext.Assets.AddAsync(asset, cancellationToken).AsTask();
    }

    public Task<bool> AssetCodeExistsAsync(
        string assetCode,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(assetCode);

        return dbContext.Assets.AnyAsync(asset => asset.AssetCode == assetCode, cancellationToken);
    }

    public Task<Asset?> GetByIdAsync(Guid assetId, CancellationToken cancellationToken = default) =>
        dbContext.Assets
            .AsNoTracking()
            .FirstOrDefaultAsync(asset => asset.Id == assetId, cancellationToken);

    public async Task<(IReadOnlyList<Asset> Assets, long TotalItems)> ListAsync(
        AssetListCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        IQueryable<Asset> query = dbContext.Assets.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            var term = criteria.Search.Trim().ToLowerInvariant();

            query = query.Where(asset =>
                asset.AssetCode.ToLower().Contains(term)
                || asset.Name.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(criteria.Category))
        {
            var category = criteria.Category.Trim().ToLowerInvariant();

            query = query.Where(asset => asset.Category.ToLower() == category);
        }

        if (criteria.Status is { } status)
        {
            query = query.Where(asset => asset.Status == status);
        }

        var totalItems = await query.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var assets = await ApplySort(query, criteria)
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return (assets, totalItems);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);

    private static IQueryable<Asset> ApplySort(
        IQueryable<Asset> query,
        AssetListCriteria criteria)
    {
        var option = criteria.Sort.Count == 0
            ? new AssetSortOption(AssetSortField.AssetCode, false)
            : criteria.Sort[0];

        IOrderedQueryable<Asset> ordered = option.Field switch
        {
            AssetSortField.Name => option.Descending
                ? query.OrderByDescending(asset => asset.Name)
                : query.OrderBy(asset => asset.Name),
            AssetSortField.AcquisitionDate => option.Descending
                ? query.OrderByDescending(asset => asset.AcquisitionDate)
                : query.OrderBy(asset => asset.AcquisitionDate),
            AssetSortField.AssetCode => option.Descending
                ? query.OrderByDescending(asset => asset.AssetCode)
                : query.OrderBy(asset => asset.AssetCode),
            _ => query.OrderBy(asset => asset.AssetCode),
        };

        return ordered.ThenBy(asset => asset.Id);
    }
}
