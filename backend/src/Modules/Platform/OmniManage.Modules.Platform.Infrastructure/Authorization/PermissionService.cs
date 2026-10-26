using OmniManage.BuildingBlocks.Application;
using OmniManage.Modules.Platform.Application.Auth;
using OmniManage.Modules.Platform.Domain.Abstractions;

namespace OmniManage.Modules.Platform.Infrastructure.Authorization;

public sealed class PermissionService(
    IUserRepository users,
    ITenantModuleCache licenses) : IPermissionService
{
    public async Task<bool> HasPermissionAsync(
        Guid tenantId,
        Guid userId,
        string permissionCode,
        CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty
            || userId == Guid.Empty
            || string.IsNullOrWhiteSpace(permissionCode)
            || !PermissionModuleResolver.TryResolveModuleCode(permissionCode, out var moduleCode))
        {
            return false;
        }

        var snapshot = await licenses.GetAsync(tenantId, cancellationToken).ConfigureAwait(false);

        if (!snapshot.IsActive(moduleCode))
        {
            return false;
        }

        var permissionCodes = await users
            .GetPermissionCodesAsync(tenantId, userId, cancellationToken)
            .ConfigureAwait(false);

        return permissionCodes.Contains(permissionCode, StringComparer.Ordinal);
    }
}
