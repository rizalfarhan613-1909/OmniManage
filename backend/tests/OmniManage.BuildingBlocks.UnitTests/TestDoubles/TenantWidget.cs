using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.BuildingBlocks.UnitTests.TestDoubles;

public sealed class TenantWidget : ITenantEntity
{
    private TenantWidget()
    {
    }

    public TenantWidget(Guid id, Guid tenantId, string name)
    {
        Id = id;
        TenantId = tenantId;
        Name = name;
    }

    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public string Name { get; set; } = string.Empty;
}
