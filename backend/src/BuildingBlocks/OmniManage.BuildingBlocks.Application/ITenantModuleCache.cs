namespace OmniManage.BuildingBlocks.Application;

public interface ITenantModuleCache
{
    Task<LicenseSnapshot> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task InvalidateAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);
}
