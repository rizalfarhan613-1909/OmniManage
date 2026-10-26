using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Inventory.Application.Common;
using OmniManage.Modules.Inventory.Domain.Abstractions;

namespace OmniManage.Modules.Inventory.Application.Warehouses.ListWarehouses;

public sealed class ListWarehousesQueryHandler(IWarehouseRepository warehouses)
    : IQueryHandler<ListWarehousesQuery, Result<PagedResult<WarehouseDto>>>
{
    private static readonly IReadOnlyDictionary<string, WarehouseSortField> SortFields =
        new Dictionary<string, WarehouseSortField>(StringComparer.OrdinalIgnoreCase)
        {
            ["code"] = WarehouseSortField.Code,
            ["name"] = WarehouseSortField.Name,
            ["createdAt"] = WarehouseSortField.CreatedAt,
        };

    public async Task<Result<PagedResult<WarehouseDto>>> Handle(
        ListWarehousesQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var (page, pageSize, skip) = Pagination.Normalize(query.Page, query.PageSize);
        var sort = SortParser
            .Parse(query.Sort, SortFields, WarehouseSortField.Code)
            .Select(option => new WarehouseSortOption(option.Field, option.Descending))
            .ToArray();

        var criteria = new WarehouseListCriteria(skip, pageSize, query.Search?.Trim(), query.IsActive, sort);
        var (warehousesFound, totalItems) = await warehouses
            .ListAsync(criteria, cancellationToken)
            .ConfigureAwait(false);

        var dtos = warehousesFound.Select(WarehouseDto.From).ToArray();

        return new PagedResult<WarehouseDto>(dtos, page, pageSize, totalItems);
    }
}
