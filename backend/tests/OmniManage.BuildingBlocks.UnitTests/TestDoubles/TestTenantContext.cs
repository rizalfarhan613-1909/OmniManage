using OmniManage.BuildingBlocks.Application;

namespace OmniManage.BuildingBlocks.UnitTests.TestDoubles;

public sealed class TestTenantContext(Guid tenantId) : ITenantContext
{
    public Guid TenantId { get; } = tenantId;

    public bool IsSuperadminContext => false;
}
