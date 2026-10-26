using OmniManage.BuildingBlocks.Application;

namespace OmniManage.Modules.Inventory.UnitTests.TestDoubles;

internal sealed class FakeModuleLicenseChecker : IModuleLicenseChecker
{
    public bool IsActive { get; set; } = true;

    public List<(Guid TenantId, string ModuleCode)> Checks { get; } = [];

    public Task<bool> IsActiveAsync(
        Guid tenantId,
        string moduleCode,
        CancellationToken cancellationToken = default)
    {
        Checks.Add((tenantId, moduleCode));
        return Task.FromResult(IsActive);
    }
}
