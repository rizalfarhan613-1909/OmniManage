using Microsoft.EntityFrameworkCore;
using OmniManage.Modules.Platform.Domain.Abstractions;
using OmniManage.Modules.Platform.Domain.Tenancy;

namespace OmniManage.Modules.Platform.Infrastructure.Persistence.Repositories;

public sealed class TenantRepository(PlatformDbContext dbContext) : ITenantRepository
{
    public Task<Tenant?> GetByCodeAsync(
        string normalizedCode,
        CancellationToken cancellationToken = default) =>
        dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(tenant => tenant.Code == normalizedCode, cancellationToken);

    public Task<Tenant?> GetByIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default) =>
        dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(tenant => tenant.Id == tenantId, cancellationToken);

    public Task<Tenant?> GetByIdForUpdateAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default) =>
        dbContext.Tenants
            .FirstOrDefaultAsync(tenant => tenant.Id == tenantId, cancellationToken);

    public async Task<(IReadOnlyList<TenantSummaryProjection> Items, long TotalItems)> ListAsync(
        TenantListCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        IQueryable<Tenant> query = dbContext.Tenants.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            var term = criteria.Search.Trim().ToLowerInvariant();

            query = query.Where(tenant =>
                tenant.Code.ToLower().Contains(term)
                || tenant.Name.ToLower().Contains(term)
                || tenant.PlanCode.ToLower().Contains(term));
        }

        var totalItems = await query.LongCountAsync(cancellationToken).ConfigureAwait(false);

        var page = await ApplySort(query, criteria)
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .Select(tenant => new
            {
                tenant.Id,
                tenant.Code,
                tenant.Name,
                tenant.Status,
                tenant.PlanCode,
                tenant.CreatedAt,
                ActiveUserCount = dbContext.Users.Count(user =>
                    user.TenantId == tenant.Id && user.IsActive),
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var tenantIds = page.Select(tenant => tenant.Id).ToArray();

        var modules = tenantIds.Length == 0
            ? []
            : await dbContext.TenantModules
                .AsNoTracking()
                .Where(tenantModule => tenantIds.Contains(tenantModule.TenantId)
                    && tenantModule.IsActive)
                .Select(tenantModule => new
                {
                    tenantModule.TenantId,
                    tenantModule.ModuleCode,
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

        var modulesByTenant = modules
            .GroupBy(module => module.TenantId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)group
                    .Select(module => module.ModuleCode)
                    .Order(StringComparer.Ordinal)
                    .ToArray());

        var items = page
            .Select(tenant => new TenantSummaryProjection(
                tenant.Id,
                tenant.Code,
                tenant.Name,
                tenant.Status,
                tenant.PlanCode,
                tenant.ActiveUserCount,
                modulesByTenant.TryGetValue(tenant.Id, out var licensed)
                    ? licensed
                    : [],
                tenant.CreatedAt))
            .ToArray();

        return (items, totalItems);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);

    private static IQueryable<Tenant> ApplySort(
        IQueryable<Tenant> query,
        TenantListCriteria criteria)
    {
        var option = criteria.Sort.Count == 0
            ? new TenantSortOption(TenantSortField.Name, false)
            : criteria.Sort[0];

        IOrderedQueryable<Tenant> ordered = option.Field switch
        {
            TenantSortField.Code => option.Descending
                ? query.OrderByDescending(tenant => tenant.Code)
                : query.OrderBy(tenant => tenant.Code),
            TenantSortField.CreatedAt => option.Descending
                ? query.OrderByDescending(tenant => tenant.CreatedAt)
                : query.OrderBy(tenant => tenant.CreatedAt),
            TenantSortField.Name => option.Descending
                ? query.OrderByDescending(tenant => tenant.Name)
                : query.OrderBy(tenant => tenant.Name),
            _ => query.OrderBy(tenant => tenant.Name),
        };

        return ordered.ThenBy(tenant => tenant.Id);
    }
}
