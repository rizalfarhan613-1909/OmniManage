namespace OmniManage.Modules.MasterData.Domain.Abstractions;

public sealed record ItemListCriteria(
    int Skip,
    int Take,
    string? Search,
    IReadOnlyList<ItemSortOption> Sort);

public sealed record ItemSortOption(ItemSortField Field, bool Descending);

public enum ItemSortField
{
    Sku = 1,
    Name = 2,
    Category = 3,
    CreatedAt = 4,
    BasePrice = 5,
    CostPrice = 6,
}
