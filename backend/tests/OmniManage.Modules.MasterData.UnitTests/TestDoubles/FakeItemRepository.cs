using OmniManage.Modules.MasterData.Domain.Abstractions;
using OmniManage.Modules.MasterData.Domain.Items;

namespace OmniManage.Modules.MasterData.UnitTests.TestDoubles;

internal sealed class FakeItemRepository : IItemRepository
{
    public List<Item> Items { get; } = [];

    public bool SkuExists { get; set; }

    public ItemListCriteria? LastCriteria { get; private set; }

    public int SaveCount { get; private set; }

    public Task AddAsync(Item item, CancellationToken cancellationToken = default)
    {
        Items.Add(item);
        return Task.CompletedTask;
    }

    public Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken = default) =>
        Task.FromResult(SkuExists);

    public Task<(IReadOnlyList<Item> Items, long TotalItems)> ListAsync(
        ItemListCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        LastCriteria = criteria;

        IReadOnlyList<Item> items = Items
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .ToArray();

        return Task.FromResult((items, (long)Items.Count));
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;
        return Task.CompletedTask;
    }
}
