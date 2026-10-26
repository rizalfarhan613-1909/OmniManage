using OmniManage.BuildingBlocks.Application;

namespace OmniManage.Api.IntegrationTests.Licensing;

internal sealed class FakeTenantContext(Guid tenantId) : ITenantContext
{
    public Guid TenantId { get; } = tenantId;

    public bool IsSuperadminContext => false;
}
