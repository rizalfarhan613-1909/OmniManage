using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Procurement.Application.Common;
using OmniManage.Modules.Procurement.Domain.Abstractions;

namespace OmniManage.Modules.Procurement.Application.Suppliers.GetSuppliers;

public sealed class GetSuppliersQueryHandler(ISupplierRepository suppliers)
    : IQueryHandler<GetSuppliersQuery, Result<PagedResult<SupplierDto>>>
{
    private static readonly IReadOnlyDictionary<string, SupplierSortField> SortFields =
        new Dictionary<string, SupplierSortField>(StringComparer.OrdinalIgnoreCase)
        {
            ["code"] = SupplierSortField.Code,
            ["name"] = SupplierSortField.Name,
            ["createdAt"] = SupplierSortField.CreatedAt,
        };

    public async Task<Result<PagedResult<SupplierDto>>> Handle(
        GetSuppliersQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var (page, pageSize, skip) = Pagination.Normalize(query.Page, query.PageSize);
        var sort = SortParser
            .Parse(query.Sort, SortFields, SupplierSortField.Name)
            .Select(option => new SupplierSortOption(option.Field, option.Descending))
            .ToArray();

        var criteria = new SupplierListCriteria(
            skip,
            pageSize,
            query.Search?.Trim(),
            query.IsActive,
            sort);

        var (suppliersInPage, totalItems) = await suppliers
            .ListAsync(criteria, cancellationToken)
            .ConfigureAwait(false);

        var dtos = suppliersInPage.Select(SupplierDto.From).ToArray();

        return new PagedResult<SupplierDto>(dtos, page, pageSize, totalItems);
    }
}
