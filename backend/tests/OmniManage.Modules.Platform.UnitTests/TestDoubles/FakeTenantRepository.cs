using OmniManage.Modules.Platform.Domain.Abstractions;
using OmniManage.Modules.Platform.Domain.Tenancy;

namespace OmniManage.Modules.Platform.UnitTests.TestDoubles;

internal sealed class FakeTenantRepository : ITenantRepository
{
    public Tenant? Tenant { get; set; }

    public Task<Tenant?> GetByCodeAsync(
        string normalizedCode,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Tenant is not null && Tenant.Code == normalizedCode ? Tenant : null);

    public Task<Tenant?> GetByIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Tenant is not null && Tenant.Id == tenantId ? Tenant : null);

    public Task<Tenant?> GetByIdForUpdateAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default) =>
        GetByIdAsync(tenantId, cancellationToken);

    public Task<(IReadOnlyList<TenantSummaryProjection> Items, long TotalItems)> ListAsync(
        TenantListCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        var tenants = Tenant is null ? new List<Tenant>() : new List<Tenant> { Tenant };

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            var term = criteria.Search.Trim().ToLowerInvariant();
            tenants = tenants
                .Where(tenant =>
                    tenant.Code.ToLowerInvariant().Contains(term)
                    || tenant.Name.ToLowerInvariant().Contains(term)
                    || tenant.PlanCode.ToLowerInvariant().Contains(term))
                .ToList();
        }

        var totalItems = tenants.Count;
        var ordered = criteria.Sort.Count == 0
            ? tenants.OrderBy(tenant => tenant.Name, StringComparer.Ordinal)
            : criteria.Sort[0].Field switch
            {
                TenantSortField.Code => criteria.Sort[0].Descending
                    ? tenants.OrderByDescending(tenant => tenant.Code, StringComparer.Ordinal)
                    : tenants.OrderBy(tenant => tenant.Code, StringComparer.Ordinal),
                TenantSortField.CreatedAt => criteria.Sort[0].Descending
                    ? tenants.OrderByDescending(tenant => tenant.CreatedAt)
                    : tenants.OrderBy(tenant => tenant.CreatedAt),
                _ => criteria.Sort[0].Descending
                    ? tenants.OrderByDescending(tenant => tenant.Name, StringComparer.Ordinal)
                    : tenants.OrderBy(tenant => tenant.Name, StringComparer.Ordinal),
            };

        IReadOnlyList<TenantSummaryProjection> items = ordered
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .Select(tenant => new TenantSummaryProjection(
                tenant.Id,
                tenant.Code,
                tenant.Name,
                tenant.Status,
                tenant.PlanCode,
                0,
                [],
                tenant.CreatedAt))
            .ToArray();

        return Task.FromResult((items, (long)totalItems));
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCallCount++;
        return Task.CompletedTask;
    }

    public int SaveChangesCallCount { get; private set; }
}
