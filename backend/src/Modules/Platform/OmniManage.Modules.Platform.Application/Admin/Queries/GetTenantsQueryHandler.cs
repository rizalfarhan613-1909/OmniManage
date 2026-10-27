using OmniManage.BuildingBlocks.Application;
using OmniManage.Modules.Platform.Application.Admin.Dtos;
using OmniManage.Modules.Platform.Application.Common;
using OmniManage.Modules.Platform.Domain.Abstractions;

namespace OmniManage.Modules.Platform.Application.Admin.Queries;

public sealed class GetTenantsQueryHandler(ITenantRepository tenants)
    : IQueryHandler<GetTenantsQuery, PagedResult<TenantSummaryDto>>
{
    private static readonly IReadOnlyDictionary<string, TenantSortField> SortFields =
        new Dictionary<string, TenantSortField>(StringComparer.OrdinalIgnoreCase)
        {
            ["name"] = TenantSortField.Name,
            ["code"] = TenantSortField.Code,
            ["createdAt"] = TenantSortField.CreatedAt,
        };

    public async Task<PagedResult<TenantSummaryDto>> Handle(
        GetTenantsQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var (page, pageSize, skip) = Pagination.Normalize(query.PageNumber, query.PageSize);
        var sort = SortParser
            .Parse(query.Sort, SortFields, TenantSortField.Name)
            .Select(option => new TenantSortOption(option.Field, option.Descending))
            .ToArray();

        var criteria = new TenantListCriteria(skip, pageSize, query.Search?.Trim(), sort);

        var (found, totalItems) = await tenants
            .ListAsync(criteria, cancellationToken)
            .ConfigureAwait(false);

        var items = found.Select(TenantSummaryDto.From).ToArray();

        return new PagedResult<TenantSummaryDto>(items, page, pageSize, totalItems);
    }
}
