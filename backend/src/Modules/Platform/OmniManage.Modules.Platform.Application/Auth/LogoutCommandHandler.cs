using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Application.Auditing;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Platform.Domain.Abstractions;
using OmniManage.Modules.Platform.Domain.Identity;

namespace OmniManage.Modules.Platform.Application.Auth;

public sealed class LogoutCommandHandler(
    IRefreshTokenRepository refreshTokens,
    IRefreshTokenGenerator refreshTokenGenerator,
    RefreshTokenSessionRevoker sessionRevoker,
    IAuditLogger auditLogger)
    : ICommandHandler<LogoutCommand, LogoutResponse>
{
    public async Task<Result<LogoutResponse>> Handle(
        LogoutCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            return new LogoutResponse(false);
        }

        var tokenHash = refreshTokenGenerator.Hash(command.RefreshToken);
        var stored = await refreshTokens
            .GetByHashAsync(tokenHash, cancellationToken)
            .ConfigureAwait(false);

        if (stored is null)
        {
            return new LogoutResponse(false);
        }

        await sessionRevoker
            .RevokeAsync(stored, RefreshTokenRevokedReasons.Logout, cancellationToken)
            .ConfigureAwait(false);

        await auditLogger.LogAsync(
            new AuditEntry(
                Action: "auth.logout",
                EntityType: nameof(RefreshToken),
                EntityId: stored.Id.ToString(),
                TenantId: stored.TenantId,
                ActorId: stored.UserId,
                ActorType: "User",
                IpAddress: command.IpAddress,
                UserAgent: command.UserAgent),
            failClosed: true,
            cancellationToken).ConfigureAwait(false);

        return new LogoutResponse(true);
    }
}
