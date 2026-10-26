using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Platform.Domain.Errors;

namespace OmniManage.Modules.Platform.Domain.Identity;

public sealed class Role : AggregateRoot<Guid>, ITenantEntity
{
    public const int NameMaxLength = 100;

    private readonly List<RolePermission> _rolePermissions = [];

    private Role()
    {
    }

    private Role(Guid id, Guid tenantId, string name, bool isSystem)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        IsSystem = isSystem;
    }

    public Guid TenantId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public bool IsSystem { get; private set; }

    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    public static Result<Role> Create(Guid id, Guid tenantId, string? name, bool isSystem)
    {
        if (id == Guid.Empty)
        {
            return RoleErrors.InvalidId;
        }

        if (tenantId == Guid.Empty)
        {
            return RoleErrors.InvalidTenantId;
        }

        if (!TryNormalizeName(name, out var normalizedName))
        {
            return RoleErrors.InvalidName;
        }

        return new Role(id, tenantId, normalizedName, isSystem);
    }

    public Result GrantPermission(Permission permission)
    {
        ArgumentNullException.ThrowIfNull(permission);

        if (_rolePermissions.Exists(rolePermission => rolePermission.PermissionId == permission.Id))
        {
            return Result.Success();
        }

        _rolePermissions.Add(new RolePermission(TenantId, Id, permission.Id));
        return Result.Success();
    }

    public Result RevokePermission(string? permissionCode)
    {
        if (string.IsNullOrWhiteSpace(permissionCode))
        {
            return PermissionErrors.InvalidCode;
        }

        var normalizedCode = permissionCode.Trim().ToLowerInvariant();
        var removed = _rolePermissions.RemoveAll(rolePermission => rolePermission.PermissionId == normalizedCode);

        return removed == 0 ? RoleErrors.PermissionNotGranted : Result.Success();
    }

    private static bool TryNormalizeName(string? value, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim();

        if (candidate.Length > NameMaxLength)
        {
            return false;
        }

        normalized = candidate;
        return true;
    }
}
