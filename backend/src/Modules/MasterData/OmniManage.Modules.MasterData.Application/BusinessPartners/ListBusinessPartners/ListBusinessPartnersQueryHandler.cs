using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.MasterData.Application.Common;
using OmniManage.Modules.MasterData.Domain.Abstractions;

namespace OmniManage.Modules.MasterData.Application.BusinessPartners.ListBusinessPartners;

public sealed class ListBusinessPartnersQueryHandler(IBusinessPartnerRepository businessPartners)
    : IQueryHandler<ListBusinessPartnersQuery, Result<PagedResult<BusinessPartnerDto>>>
{
    private static readonly IReadOnlyDictionary<string, BusinessPartnerSortField> SortFields =
        new Dictionary<string, BusinessPartnerSortField>(StringComparer.OrdinalIgnoreCase)
        {
            ["code"] = BusinessPartnerSortField.Code,
            ["name"] = BusinessPartnerSortField.Name,
            ["createdAt"] = BusinessPartnerSortField.CreatedAt,
            ["creditLimit"] = BusinessPartnerSortField.CreditLimit,
        };

    public async Task<Result<PagedResult<BusinessPartnerDto>>> Handle(
        ListBusinessPartnersQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var (page, pageSize, skip) = Pagination.Normalize(query.Page, query.PageSize);
        var sort = SortParser
            .Parse(query.Sort, SortFields, BusinessPartnerSortField.Code)
            .Select(option => new BusinessPartnerSortOption(option.Field, option.Descending))
            .ToArray();

        var criteria = new BusinessPartnerListCriteria(skip, pageSize, query.Search?.Trim(), sort);
        var (items, totalItems) = await businessPartners
            .ListAsync(criteria, cancellationToken)
            .ConfigureAwait(false);

        var dtos = items.Select(BusinessPartnerDto.From).ToArray();

        return new PagedResult<BusinessPartnerDto>(dtos, page, pageSize, totalItems);
    }
}
