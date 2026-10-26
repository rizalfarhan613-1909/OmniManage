using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Manufacturing.Application.Common;
using OmniManage.Modules.Manufacturing.Domain.Abstractions;

namespace OmniManage.Modules.Manufacturing.Application.BillOfMaterials.GetBillOfMaterials;

public sealed class GetBillOfMaterialsQueryHandler(IBillOfMaterialRepository billOfMaterials)
    : IQueryHandler<GetBillOfMaterialsQuery, Result<PagedResult<BillOfMaterialDto>>>
{
    private static readonly IReadOnlyDictionary<string, BillOfMaterialSortField> SortFields =
        new Dictionary<string, BillOfMaterialSortField>(StringComparer.OrdinalIgnoreCase)
        {
            ["code"] = BillOfMaterialSortField.Code,
            ["name"] = BillOfMaterialSortField.Name,
            ["version"] = BillOfMaterialSortField.Version,
            ["isActive"] = BillOfMaterialSortField.IsActive,
            ["createdAt"] = BillOfMaterialSortField.CreatedAt,
        };

    public async Task<Result<PagedResult<BillOfMaterialDto>>> Handle(
        GetBillOfMaterialsQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var (page, pageSize, skip) = Pagination.Normalize(query.Page, query.PageSize);
        var sort = SortParser
            .Parse(query.Sort, SortFields, BillOfMaterialSortField.Code)
            .Select(option => new BillOfMaterialSortOption(option.Field, option.Descending))
            .ToArray();

        var criteria = new BillOfMaterialListCriteria(
            skip,
            pageSize,
            query.Search?.Trim(),
            query.ProductItemId == Guid.Empty ? null : query.ProductItemId,
            query.IsActive,
            sort);

        var (items, totalItems) = await billOfMaterials
            .ListAsync(criteria, cancellationToken)
            .ConfigureAwait(false);

        var dtos = items.Select(BillOfMaterialDto.From).ToArray();

        return new PagedResult<BillOfMaterialDto>(dtos, page, pageSize, totalItems);
    }
}
