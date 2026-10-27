using Microsoft.EntityFrameworkCore;
using OmniManage.Modules.Platform.Domain.Abstractions;
using OmniManage.Modules.Platform.Domain.Identity;

namespace OmniManage.Modules.Platform.Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenRepository(PlatformDbContext dbContext) : IRefreshTokenRepository
{
    public Task AddAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(refreshToken);

        return dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken).AsTask();
    }

    public Task<RefreshToken?> GetByHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default) =>
        dbContext.RefreshTokens.FirstOrDefaultAsync(
            refreshToken => refreshToken.TokenHash == tokenHash,
            cancellationToken);

    public async Task<IReadOnlyList<RefreshToken>> GetSessionTokensAsync(
        Guid tenantId,
        Guid sessionId,
        CancellationToken cancellationToken = default) =>
        await dbContext.RefreshTokens
            .Where(refreshToken => refreshToken.TenantId == tenantId
                && refreshToken.SessionId == sessionId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<ActiveSessionProjection>> ListActiveSessionsAsync(
        Guid tenantId,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        var tokens = await dbContext.RefreshTokens
            .AsNoTracking()
            .Where(refreshToken => refreshToken.TenantId == tenantId
                && refreshToken.RevokedAt == null
                && refreshToken.ExpiresAt > now)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (tokens.Count == 0)
        {
            return [];
        }

        var sessions = tokens
            .GroupBy(token => token.SessionId)
            .Select(group =>
            {
                var latest = group
                    .OrderByDescending(token => token.IssuedAt)
                    .ThenByDescending(token => token.Id)
                    .First();

                return new
                {
                    SessionId = group.Key,
                    latest.UserId,
                    StartedAt = group.Min(token => token.IssuedAt),
                    LastSeenAt = group.Max(token => token.IssuedAt),
                    latest.IpAddress,
                    latest.UserAgent,
                };
            })
            .ToArray();

        var userIds = sessions
            .Select(session => session.UserId)
            .Distinct()
            .ToArray();

        var users = await dbContext.Users
            .AsNoTracking()
            .Where(user => user.TenantId == tenantId && userIds.Contains(user.Id))
            .Select(user => new { user.Id, user.Email })
            .ToDictionaryAsync(user => user.Id, user => user.Email, cancellationToken)
            .ConfigureAwait(false);

        var roleRows = await (from userRole in dbContext.UserRoles.AsNoTracking()
                              join role in dbContext.Roles.AsNoTracking()
                                  on userRole.RoleId equals role.Id
                              where userRole.TenantId == tenantId
                                  && role.TenantId == tenantId
                                  && userIds.Contains(userRole.UserId)
                              select new { userRole.UserId, role.Name })
            .Distinct()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var rolesByUser = roleRows
            .GroupBy(row => row.UserId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)group
                    .Select(row => row.Name)
                    .Order(StringComparer.Ordinal)
                    .ToArray());

        return sessions
            .Select(session => new ActiveSessionProjection(
                session.SessionId,
                session.UserId,
                users.TryGetValue(session.UserId, out var email) ? email : string.Empty,
                rolesByUser.TryGetValue(session.UserId, out var roles) ? roles : [],
                session.IpAddress ?? string.Empty,
                session.UserAgent,
                session.StartedAt,
                session.LastSeenAt))
            .ToArray();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
