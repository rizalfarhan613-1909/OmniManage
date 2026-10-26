using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using OmniManage.BuildingBlocks.Application;
using OmniManage.Modules.Platform.Application.Auth;

namespace OmniManage.Modules.Platform.Infrastructure.Authorization;

public sealed class PermissionAuthorizationHandler(
    IPermissionService permissions,
    ITenantContext tenantContext) : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(requirement);

        var userIdValue = context.User.FindFirstValue("sub");

        if (!Guid.TryParse(userIdValue, out var userId) || userId == Guid.Empty)
        {
            return;
        }

        var tenantId = tenantContext.TenantId;

        if (tenantId == Guid.Empty)
        {
            return;
        }

        var granted = await permissions
            .HasPermissionAsync(tenantId, userId, requirement.Permission, CancellationToken.None)
            .ConfigureAwait(false);

        if (granted)
        {
            context.Succeed(requirement);
        }
    }
}
