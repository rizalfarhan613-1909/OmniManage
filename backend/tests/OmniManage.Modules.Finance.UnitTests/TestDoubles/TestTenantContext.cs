using OmniManage.BuildingBlocks.Application;

namespace OmniManage.Modules.Finance.UnitTests.TestDoubles;

internal sealed class TestTenantContext(Guid tenantId) : ITenantContext
{
    public Guid TenantId { get; } = tenantId;

    public bool IsSuperadminContext => false;
}
