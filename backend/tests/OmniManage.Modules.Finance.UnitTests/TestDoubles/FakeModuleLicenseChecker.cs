using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Contracts;

namespace OmniManage.Modules.Finance.UnitTests.TestDoubles;

internal sealed class FakeModuleLicenseChecker : IModuleLicenseChecker
{
    public HashSet<string> ActiveModules { get; } =
        new(StringComparer.Ordinal) { ModuleCodes.FinAr, ModuleCodes.FinGl };

    public List<(Guid TenantId, string ModuleCode)> Checks { get; } = [];

    public Task<bool> IsActiveAsync(
        Guid tenantId,
        string moduleCode,
        CancellationToken cancellationToken = default)
    {
        Checks.Add((tenantId, moduleCode));
        return Task.FromResult(ActiveModules.Contains(moduleCode));
    }
}
