using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.MasterData.Application.Common;
using OmniManage.Modules.MasterData.Domain.Abstractions;

namespace OmniManage.Modules.MasterData.Application.Items.ListItems;

public sealed class ListItemsQueryHandler(IItemRepository items)
    : IQueryHandler<ListItemsQuery, Result<PagedResult<ItemDto>>>
{
    private static readonly IReadOnlyDictionary<string, ItemSortField> SortFields =
        new Dictionary<string, ItemSortField>(StringComparer.OrdinalIgnoreCase)
        {
            ["sku"] = ItemSortField.Sku,
            ["name"] = ItemSortField.Name,
            ["category"] = ItemSortField.Category,
            ["createdAt"] = ItemSortField.CreatedAt,
            ["basePrice"] = ItemSortField.BasePrice,
            ["costPrice"] = ItemSortField.CostPrice,
        };

    public async Task<Result<PagedResult<ItemDto>>> Handle(
        ListItemsQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var (page, pageSize, skip) = Pagination.Normalize(query.Page, query.PageSize);
        var sort = SortParser
            .Parse(query.Sort, SortFields, ItemSortField.Sku)
            .Select(option => new ItemSortOption(option.Field, option.Descending))
            .ToArray();

        var criteria = new ItemListCriteria(skip, pageSize, query.Search?.Trim(), sort);
        var (itemsFound, totalItems) = await items
            .ListAsync(criteria, cancellationToken)
            .ConfigureAwait(false);

        var dtos = itemsFound.Select(ItemDto.From).ToArray();

        return new PagedResult<ItemDto>(dtos, page, pageSize, totalItems);
    }
}
