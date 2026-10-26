using Microsoft.EntityFrameworkCore;
using OmniManage.Modules.MasterData.Domain.Abstractions;
using OmniManage.Modules.MasterData.Domain.Items;
using OmniManage.Modules.MasterData.Infrastructure.Persistence;

namespace OmniManage.Modules.MasterData.Infrastructure.Persistence.Repositories;

public sealed class ItemRepository(MasterDataDbContext dbContext) : IItemRepository
{
    public Task AddAsync(Item item, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);

        return dbContext.Items.AddAsync(item, cancellationToken).AsTask();
    }

    public Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);

        return dbContext.Items.AnyAsync(item => item.Sku == sku, cancellationToken);
    }

    public async Task<(IReadOnlyList<Item> Items, long TotalItems)> ListAsync(
        ItemListCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        IQueryable<Item> query = dbContext.Items.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            var term = criteria.Search.Trim().ToLowerInvariant();

            query = query.Where(item =>
                item.Sku.ToLower().Contains(term)
                || item.Name.ToLower().Contains(term)
                || (item.Category != null && item.Category.ToLower().Contains(term)));
        }

        var totalItems = await query.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var items = await ApplySort(query, criteria)
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return (items, totalItems);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);

    private static IQueryable<Item> ApplySort(IQueryable<Item> query, ItemListCriteria criteria)
    {
        var option = criteria.Sort.Count == 0
            ? new ItemSortOption(ItemSortField.Sku, false)
            : criteria.Sort[0];

        IOrderedQueryable<Item> ordered = option.Field switch
        {
            ItemSortField.Sku => option.Descending
                ? query.OrderByDescending(item => item.Sku)
                : query.OrderBy(item => item.Sku),
            ItemSortField.Name => option.Descending
                ? query.OrderByDescending(item => item.Name)
                : query.OrderBy(item => item.Name),
            ItemSortField.Category => option.Descending
                ? query.OrderByDescending(item => item.Category)
                : query.OrderBy(item => item.Category),
            ItemSortField.CreatedAt => option.Descending
                ? query.OrderByDescending(item => item.CreatedAt)
                : query.OrderBy(item => item.CreatedAt),
            ItemSortField.BasePrice => option.Descending
                ? query.OrderByDescending(item => item.BasePrice)
                : query.OrderBy(item => item.BasePrice),
            ItemSortField.CostPrice => option.Descending
                ? query.OrderByDescending(item => item.CostPrice)
                : query.OrderBy(item => item.CostPrice),
            _ => query.OrderBy(item => item.Sku),
        };

        return ordered.ThenBy(item => item.Id);
    }
}
