using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OmniManage.BuildingBlocks.Application;
using OmniManage.Modules.Platform.Infrastructure.Persistence;

namespace OmniManage.Modules.Platform.Infrastructure.Licensing;

public sealed class LicensingService(
    PlatformDbContext dbContext,
    IMemoryCache cache,
    TimeProvider clock) : ITenantModuleCache, IModuleLicenseChecker
{
    public const int CacheTtlSeconds = 60;

    public async Task<LicenseSnapshot> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
        {
            return LicenseSnapshot.Empty;
        }

        var cacheKey = CacheKey(tenantId);

        if (cache.TryGetValue(cacheKey, out LicenseSnapshot? cached) && cached is not null)
        {
            return cached;
        }

        try
        {
            var now = clock.GetUtcNow();
            var activeModuleCodes = await dbContext.TenantModules
                .AsNoTracking()
                .Where(tenantModule => tenantModule.TenantId == tenantId
                    && tenantModule.IsActive
                    && (tenantModule.ExpiresAt == null || tenantModule.ExpiresAt > now))
                .Select(tenantModule => tenantModule.ModuleCode)
                .Distinct()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var snapshot = new LicenseSnapshot(activeModuleCodes);
            cache.Set(cacheKey, snapshot, TimeSpan.FromSeconds(CacheTtlSeconds));

            return snapshot;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            throw new LicenseSourceUnavailableException(
                "The module license source is unavailable.",
                exception);
        }
    }

    public async Task<bool> IsActiveAsync(
        Guid tenantId,
        string moduleCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(moduleCode))
        {
            return false;
        }

        var snapshot = await GetAsync(tenantId, cancellationToken).ConfigureAwait(false);

        return snapshot.IsActive(moduleCode);
    }

    public Task InvalidateAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        if (tenantId != Guid.Empty)
        {
            cache.Remove(CacheKey(tenantId));
        }

        return Task.CompletedTask;
    }

    private static string CacheKey(Guid tenantId) => $"license:{tenantId:D}";
}
