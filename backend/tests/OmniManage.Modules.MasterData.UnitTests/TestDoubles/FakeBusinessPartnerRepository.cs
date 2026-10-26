using OmniManage.Modules.MasterData.Domain.Abstractions;
using OmniManage.Modules.MasterData.Domain.BusinessPartners;

namespace OmniManage.Modules.MasterData.UnitTests.TestDoubles;

internal sealed class FakeBusinessPartnerRepository : IBusinessPartnerRepository
{
    public List<BusinessPartner> Partners { get; } = [];

    public bool CodeExists { get; set; }

    public BusinessPartnerListCriteria? LastCriteria { get; private set; }

    public int SaveCount { get; private set; }

    public Task AddAsync(BusinessPartner businessPartner, CancellationToken cancellationToken = default)
    {
        Partners.Add(businessPartner);
        return Task.CompletedTask;
    }

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default) =>
        Task.FromResult(CodeExists);

    public Task<(IReadOnlyList<BusinessPartner> Items, long TotalItems)> ListAsync(
        BusinessPartnerListCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        LastCriteria = criteria;

        IReadOnlyList<BusinessPartner> items = Partners
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .ToArray();

        return Task.FromResult((items, (long)Partners.Count));
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;
        return Task.CompletedTask;
    }
}
