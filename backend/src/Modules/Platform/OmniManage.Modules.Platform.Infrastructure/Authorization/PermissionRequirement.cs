using Microsoft.AspNetCore.Authorization;

namespace OmniManage.Modules.Platform.Infrastructure.Authorization;

public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
