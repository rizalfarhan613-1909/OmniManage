using OmniManage.BuildingBlocks.Application;
using OmniManage.Modules.Platform.Domain.Identity;
using OmniManage.Modules.Platform.Domain.Abstractions;

namespace OmniManage.Modules.Platform.Application.Auth;

public sealed class RefreshTokenSessionRevoker(
    IRefreshTokenRepository refreshTokens,
    TimeProvider clock)
{
    public async Task RevokeAsync(
        RefreshToken refreshToken,
        string reason,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(refreshToken);

        var now = clock.GetUtcNow();
        var sessionTokens = await refreshTokens
            .GetSessionTokensAsync(refreshToken.TenantId, refreshToken.SessionId, cancellationToken)
            .ConfigureAwait(false);

        foreach (var token in sessionTokens)
        {
            token.Revoke(now, reason);
        }

        await refreshTokens.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
