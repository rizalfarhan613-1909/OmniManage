using OmniManage.Modules.Platform.Domain.Tenancy;

namespace OmniManage.Modules.Platform.Domain.Abstractions;

public interface ITenantRepository
{
    Task<Tenant?> GetByCodeAsync(
        string normalizedCode,
        CancellationToken cancellationToken = default);

    Task<Tenant?> GetByIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<Tenant?> GetByIdForUpdateAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<TenantSummaryProjection> Items, long TotalItems)> ListAsync(
        TenantListCriteria criteria,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
