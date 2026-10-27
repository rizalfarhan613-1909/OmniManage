using OmniManage.Modules.AssetMaintenance.Domain.Assets;

namespace OmniManage.Modules.AssetMaintenance.Domain.Abstractions;

public interface IAssetRepository
{
    Task AddAsync(Asset asset, CancellationToken cancellationToken = default);

    Task<bool> AssetCodeExistsAsync(string assetCode, CancellationToken cancellationToken = default);

    Task<Asset?> GetByIdAsync(Guid assetId, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Asset> Assets, long TotalItems)> ListAsync(
        AssetListCriteria criteria,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
