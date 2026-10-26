using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.Modules.Platform.Domain.Identity;

public sealed class RolePermission : ITenantEntity
{
    private RolePermission()
    {
    }

    internal RolePermission(Guid tenantId, Guid roleId, string permissionId)
    {
        TenantId = tenantId;
        RoleId = roleId;
        PermissionId = permissionId;
    }

    public Guid TenantId { get; private set; }

    public Guid RoleId { get; private set; }

    public string PermissionId { get; private set; } = string.Empty;
}
