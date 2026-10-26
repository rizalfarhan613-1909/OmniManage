using OmniManage.Modules.Platform.Domain.Abstractions;
using OmniManage.Modules.Platform.Domain.Identity;

namespace OmniManage.Modules.Platform.UnitTests.TestDoubles;

internal sealed class FakeRefreshTokenRepository : IRefreshTokenRepository
{
    public List<RefreshToken> Tokens { get; } = [];

    public int SaveChangesCallCount { get; private set; }

    public Task AddAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default)
    {
        Tokens.Add(refreshToken);
        return Task.CompletedTask;
    }

    public Task<RefreshToken?> GetByHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Tokens.FirstOrDefault(token => token.TokenHash == tokenHash));

    public Task<IReadOnlyList<RefreshToken>> GetSessionTokensAsync(
        Guid tenantId,
        Guid sessionId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<RefreshToken>>(
            Tokens
                .Where(token => token.TenantId == tenantId && token.SessionId == sessionId)
                .ToArray());

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCallCount++;
        return Task.CompletedTask;
    }
}
