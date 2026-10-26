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

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
