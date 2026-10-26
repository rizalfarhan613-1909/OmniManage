using OmniManage.Modules.Platform.Domain.Identity;

namespace OmniManage.Modules.Platform.Domain.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(
        Guid tenantId,
        string normalizedEmail,
        CancellationToken cancellationToken = default);

    Task<User?> GetByIdAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetRoleNamesAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetPermissionCodesAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
