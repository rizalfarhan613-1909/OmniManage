namespace OmniManage.Modules.Procurement.Domain.Abstractions;

public sealed record SupplierListCriteria(
    int Skip,
    int Take,
    string? Search,
    bool? IsActive,
    IReadOnlyList<SupplierSortOption> Sort);

public sealed record SupplierSortOption(SupplierSortField Field, bool Descending);

public enum SupplierSortField
{
    Code = 1,
    Name = 2,
    CreatedAt = 3,
}
