namespace OmniManage.BuildingBlocks.Application;

public interface IModuleLicenseChecker
{
    Task<bool> IsActiveAsync(
        Guid tenantId,
        string moduleCode,
        CancellationToken cancellationToken = default);
}
