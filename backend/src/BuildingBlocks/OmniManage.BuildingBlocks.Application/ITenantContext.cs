namespace OmniManage.BuildingBlocks.Application;

public interface ITenantContext
{
    Guid TenantId { get; }

    bool IsSuperadminContext { get; }
}
