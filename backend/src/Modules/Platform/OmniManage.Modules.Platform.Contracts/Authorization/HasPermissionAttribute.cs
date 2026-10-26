using Microsoft.AspNetCore.Authorization;

namespace OmniManage.Modules.Platform.Contracts.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "perm:";

    public HasPermissionAttribute(string permission)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);

        Permission = permission;
        Policy = PolicyPrefix + permission;
    }

    public string Permission { get; }
}
