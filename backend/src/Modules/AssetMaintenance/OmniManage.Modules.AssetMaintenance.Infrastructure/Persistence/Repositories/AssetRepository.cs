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

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
