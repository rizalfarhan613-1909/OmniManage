using OmniManage.Modules.MasterData.Domain.BusinessPartners;

namespace OmniManage.Modules.MasterData.Domain.Abstractions;

public interface IBusinessPartnerRepository
{
    Task AddAsync(BusinessPartner businessPartner, CancellationToken cancellationToken = default);

    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<BusinessPartner> Items, long TotalItems)> ListAsync(
        BusinessPartnerListCriteria criteria,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
