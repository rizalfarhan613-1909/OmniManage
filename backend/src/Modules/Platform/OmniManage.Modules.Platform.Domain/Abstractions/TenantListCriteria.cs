using OmniManage.Modules.Platform.Domain.Tenancy;

namespace OmniManage.Modules.Platform.Domain.Abstractions;

public sealed record TenantListCriteria(
    int Skip,
    int Take,
    string? Search,
    IReadOnlyList<TenantSortOption> Sort);

public sealed record TenantSortOption(TenantSortField Field, bool Descending);

public enum TenantSortField
{
    Name = 1,
    Code = 2,
    CreatedAt = 3,
}

public sealed record TenantSummaryProjection(
    Guid Id,
    string Code,
    string Name,
    TenantStatus Status,
    string PlanCode,
    int ActiveUserCount,
    IReadOnlyList<string> LicensedModules,
    DateTimeOffset CreatedAt);
