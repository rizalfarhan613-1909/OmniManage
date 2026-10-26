using OmniManage.Modules.MasterData.Domain.Items;

namespace OmniManage.Modules.MasterData.Domain.Abstractions;

public interface IItemRepository
{
    Task AddAsync(Item item, CancellationToken cancellationToken = default);

    Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Item> Items, long TotalItems)> ListAsync(
        ItemListCriteria criteria,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
