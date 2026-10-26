namespace OmniManage.Modules.MasterData.Domain.Abstractions;

public sealed record BusinessPartnerListCriteria(
    int Skip,
    int Take,
    string? Search,
    IReadOnlyList<BusinessPartnerSortOption> Sort);

public sealed record BusinessPartnerSortOption(BusinessPartnerSortField Field, bool Descending);

public enum BusinessPartnerSortField
{
    Code = 1,
    Name = 2,
    CreatedAt = 3,
    CreditLimit = 4,
}
