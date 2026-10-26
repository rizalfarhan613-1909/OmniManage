using OmniManage.BuildingBlocks.Application;

namespace OmniManage.Api.IntegrationTests.Licensing;

internal sealed class FakeTenantModuleCache : ITenantModuleCache
{
    private readonly LicenseSnapshot _snapshot;

    public FakeTenantModuleCache(IEnumerable<string> activeModules)
    {
        _snapshot = new LicenseSnapshot(activeModules);
    }

    public Task<LicenseSnapshot> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_snapshot);

    public Task InvalidateAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
