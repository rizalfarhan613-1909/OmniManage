using OmniManage.Modules.Platform.Domain.Abstractions;
using OmniManage.Modules.Platform.Domain.Identity;

namespace OmniManage.Modules.Platform.UnitTests.TestDoubles;

internal sealed class FakeUserRepository : IUserRepository
{
    public User? UserByEmail { get; set; }

    public User? UserById { get; set; }

    public IReadOnlyList<string> Roles { get; set; } = [];

    public IReadOnlyList<string> PermissionCodes { get; set; } = [];

    public int SaveChangesCallCount { get; private set; }

    public Task<User?> GetByEmailAsync(
        Guid tenantId,
        string normalizedEmail,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(
            UserByEmail is not null
                && UserByEmail.TenantId == tenantId
                && UserByEmail.Email == normalizedEmail
                ? UserByEmail
                : null);

    public Task<User?> GetByIdAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(
            UserById is not null
                && UserById.TenantId == tenantId
                && UserById.Id == userId
                ? UserById
                : null);

    public Task<IReadOnlyList<string>> GetRoleNamesAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Roles);

    public Task<IReadOnlyList<string>> GetPermissionCodesAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(PermissionCodes);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCallCount++;
        return Task.CompletedTask;
    }
}
