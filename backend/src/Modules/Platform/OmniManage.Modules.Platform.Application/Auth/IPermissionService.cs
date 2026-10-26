namespace OmniManage.Modules.Platform.Application.Auth;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(
        Guid tenantId,
        Guid userId,
        string permissionCode,
        CancellationToken cancellationToken = default);
}
