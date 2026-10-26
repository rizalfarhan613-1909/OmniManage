using Microsoft.EntityFrameworkCore;
using OmniManage.Modules.MasterData.Domain.Abstractions;
using OmniManage.Modules.MasterData.Domain.BusinessPartners;
using OmniManage.Modules.MasterData.Infrastructure.Persistence;

namespace OmniManage.Modules.MasterData.Infrastructure.Persistence.Repositories;

public sealed class BusinessPartnerRepository(MasterDataDbContext dbContext) : IBusinessPartnerRepository
{
    public Task AddAsync(
        BusinessPartner businessPartner,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(businessPartner);

        return dbContext.BusinessPartners.AddAsync(businessPartner, cancellationToken).AsTask();
    }

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        return dbContext.BusinessPartners.AnyAsync(
            businessPartner => businessPartner.Code == code,
            cancellationToken);
    }

    public async Task<(IReadOnlyList<BusinessPartner> Items, long TotalItems)> ListAsync(
        BusinessPartnerListCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        IQueryable<BusinessPartner> query = dbContext.BusinessPartners.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            var term = criteria.Search.Trim().ToLowerInvariant();

            query = query.Where(businessPartner =>
                businessPartner.Code.ToLower().Contains(term)
                || businessPartner.Name.ToLower().Contains(term));
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

    private static IQueryable<BusinessPartner> ApplySort(
        IQueryable<BusinessPartner> query,
        BusinessPartnerListCriteria criteria)
    {
        var option = criteria.Sort.Count == 0
            ? new BusinessPartnerSortOption(BusinessPartnerSortField.Code, false)
            : criteria.Sort[0];

        IOrderedQueryable<BusinessPartner> ordered = option.Field switch
        {
            BusinessPartnerSortField.Code => option.Descending
                ? query.OrderByDescending(businessPartner => businessPartner.Code)
                : query.OrderBy(businessPartner => businessPartner.Code),
            BusinessPartnerSortField.Name => option.Descending
                ? query.OrderByDescending(businessPartner => businessPartner.Name)
                : query.OrderBy(businessPartner => businessPartner.Name),
            BusinessPartnerSortField.CreatedAt => option.Descending
                ? query.OrderByDescending(businessPartner => businessPartner.CreatedAt)
                : query.OrderBy(businessPartner => businessPartner.CreatedAt),
            BusinessPartnerSortField.CreditLimit => option.Descending
                ? query.OrderByDescending(businessPartner => businessPartner.CreditLimit)
                : query.OrderBy(businessPartner => businessPartner.CreditLimit),
            _ => query.OrderBy(businessPartner => businessPartner.Code),
        };

        return ordered.ThenBy(businessPartner => businessPartner.Id);
    }
}
