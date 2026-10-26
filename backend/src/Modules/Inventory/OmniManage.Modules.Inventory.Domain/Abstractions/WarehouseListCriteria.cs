namespace OmniManage.Modules.Inventory.Domain.Abstractions;

public sealed record WarehouseListCriteria(
    int Skip,
    int Take,
    string? Search,
    bool? IsActive,
    IReadOnlyList<WarehouseSortOption> Sort);

public sealed record WarehouseSortOption(WarehouseSortField Field, bool Descending);

public enum WarehouseSortField
{
    Code = 1,
    Name = 2,
    CreatedAt = 3,
}
