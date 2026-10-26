using Microsoft.EntityFrameworkCore;
using OmniManage.Modules.Platform.Domain.Abstractions;
using OmniManage.Modules.Platform.Domain.Identity;

namespace OmniManage.Modules.Platform.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(PlatformDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByEmailAsync(
        Guid tenantId,
        string normalizedEmail,
        CancellationToken cancellationToken = default) =>
        dbContext.Users.FirstOrDefaultAsync(
            user => user.TenantId == tenantId && user.Email == normalizedEmail,
            cancellationToken);

    public Task<User?> GetByIdAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default) =>
        dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                user => user.TenantId == tenantId && user.Id == userId,
                cancellationToken);

    public async Task<IReadOnlyList<string>> GetRoleNamesAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await (from userRole in dbContext.UserRoles.AsNoTracking()
               join role in dbContext.Roles.AsNoTracking() on userRole.RoleId equals role.Id
               where userRole.TenantId == tenantId
                   && userRole.UserId == userId
                   && role.TenantId == tenantId
               select role.Name)
            .Distinct()
            .OrderBy(name => name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<string>> GetPermissionCodesAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await (from userRole in dbContext.UserRoles.AsNoTracking()
               join rolePermission in dbContext.RolePermissions.AsNoTracking()
                   on userRole.RoleId equals rolePermission.RoleId
               where userRole.TenantId == tenantId
                   && userRole.UserId == userId
                   && rolePermission.TenantId == tenantId
               select rolePermission.PermissionId)
            .Distinct()
            .OrderBy(code => code)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
