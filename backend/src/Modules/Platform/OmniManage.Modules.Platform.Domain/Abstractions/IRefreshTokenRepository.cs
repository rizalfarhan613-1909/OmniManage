using OmniManage.Modules.Platform.Domain.Identity;

namespace OmniManage.Modules.Platform.Domain.Abstractions;

public interface IRefreshTokenRepository
{
    Task AddAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default);

    Task<RefreshToken?> GetByHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RefreshToken>> GetSessionTokensAsync(
        Guid tenantId,
        Guid sessionId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ActiveSessionProjection>> ListActiveSessionsAsync(
        Guid tenantId,
        DateTimeOffset now,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
