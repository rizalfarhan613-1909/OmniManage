namespace OmniManage.Modules.Manufacturing.Domain.Abstractions;

public sealed record BillOfMaterialListCriteria(
    int Skip,
    int Take,
    string? Search,
    Guid? ProductItemId,
    bool? IsActive,
    IReadOnlyList<BillOfMaterialSortOption> Sort);

public sealed record BillOfMaterialSortOption(BillOfMaterialSortField Field, bool Descending);

public enum BillOfMaterialSortField
{
    Code = 1,
    Name = 2,
    Version = 3,
    IsActive = 4,
    CreatedAt = 5,
}
