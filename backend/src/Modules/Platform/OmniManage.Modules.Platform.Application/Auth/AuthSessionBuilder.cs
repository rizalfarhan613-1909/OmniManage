using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Platform.Domain.Abstractions;
using OmniManage.Modules.Platform.Domain.Errors;
using OmniManage.Modules.Platform.Domain.Tenancy;

namespace OmniManage.Modules.Platform.Application.Auth;

public sealed class AuthSessionBuilder(
    IUserRepository users,
    ITenantRepository tenants,
    ITenantModuleCache licenses)
{
    public async Task<Result<AuthSessionDto>> BuildAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty || userId == Guid.Empty)
        {
            return AuthErrors.Unauthenticated;
        }

        var user = await users.GetByIdAsync(tenantId, userId, cancellationToken).ConfigureAwait(false);

        if (user is null || !user.IsActive)
        {
            return AuthErrors.Unauthenticated;
        }

        var tenant = await tenants.GetByIdAsync(tenantId, cancellationToken).ConfigureAwait(false);

        if (tenant is null || tenant.Status != TenantStatus.Active)
        {
            return AuthErrors.Unauthenticated;
        }

        var roles = await users.GetRoleNamesAsync(tenantId, userId, cancellationToken).ConfigureAwait(false);
        var permissionCodes = await users
            .GetPermissionCodesAsync(tenantId, userId, cancellationToken)
            .ConfigureAwait(false);
        var licensesSnapshot = await licenses.GetAsync(tenantId, cancellationToken).ConfigureAwait(false);

        var effectivePermissions = permissionCodes
            .Where(permissionCode => PermissionModuleResolver.IsEnabled(permissionCode, licensesSnapshot))
            .Order(StringComparer.Ordinal)
            .ToArray();

        return new AuthSessionDto(
            new AuthUserDto(user.Id, user.Email, roles),
            new AuthTenantDto(tenant.Id, tenant.Code, tenant.Name),
            licensesSnapshot.ActiveModuleCodes,
            effectivePermissions);
    }
}
