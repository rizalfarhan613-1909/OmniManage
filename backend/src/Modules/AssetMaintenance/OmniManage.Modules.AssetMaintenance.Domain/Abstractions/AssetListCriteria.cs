using OmniManage.Modules.AssetMaintenance.Domain.Assets;

namespace OmniManage.Modules.AssetMaintenance.Domain.Abstractions;

public sealed record AssetListCriteria(
    int Skip,
    int Take,
    string? Search,
    string? Category,
    AssetStatus? Status,
    IReadOnlyList<AssetSortOption> Sort);

public sealed record AssetSortOption(AssetSortField Field, bool Descending);

public enum AssetSortField
{
    AssetCode = 1,
    Name = 2,
    AcquisitionDate = 3,
}
