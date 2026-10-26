using OmniManage.BuildingBlocks.Application;

namespace OmniManage.Modules.Platform.UnitTests.TestDoubles;

internal sealed class FakeTenantModuleCache : ITenantModuleCache
{
    public LicenseSnapshot Snapshot { get; set; } = LicenseSnapshot.Empty;

    public int InvalidateCallCount { get; private set; }

    public Task<LicenseSnapshot> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Snapshot);

    public Task InvalidateAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        InvalidateCallCount++;
        return Task.CompletedTask;
    }
}
