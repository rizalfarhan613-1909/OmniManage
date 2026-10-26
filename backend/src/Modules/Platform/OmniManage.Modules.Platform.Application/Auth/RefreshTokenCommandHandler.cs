using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Application.Auditing;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Platform.Domain.Abstractions;
using OmniManage.Modules.Platform.Domain.Errors;
using OmniManage.Modules.Platform.Domain.Identity;

namespace OmniManage.Modules.Platform.Application.Auth;

public sealed class RefreshTokenCommandHandler(
    IRefreshTokenRepository refreshTokens,
    AuthSessionBuilder sessionBuilder,
    IJwtTokenGenerator tokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    IGuidGenerator guidGenerator,
    RefreshTokenOptions refreshTokenOptions,
    RefreshTokenSessionRevoker sessionRevoker,
    TimeProvider clock,
    IAuditLogger auditLogger)
    : ICommandHandler<RefreshTokenCommand, LoginResponse>
{
    public async Task<Result<LoginResponse>> Handle(
        RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            return RefreshTokenErrors.InvalidRefreshToken;
        }

        var now = clock.GetUtcNow();
        var tokenHash = refreshTokenGenerator.Hash(command.RefreshToken);
        var stored = await refreshTokens
            .GetByHashAsync(tokenHash, cancellationToken)
            .ConfigureAwait(false);

        if (stored is null)
        {
            return RefreshTokenErrors.InvalidRefreshToken;
        }

        if (stored.RevokedAt is not null)
        {
            await sessionRevoker
                .RevokeAsync(stored, RefreshTokenRevokedReasons.ReuseDetected, cancellationToken)
                .ConfigureAwait(false);
            await WriteReuseAuditAsync(stored, cancellationToken).ConfigureAwait(false);

            return RefreshTokenErrors.InvalidRefreshToken;
        }

        if (!stored.IsActive(now))
        {
            stored.Revoke(now, RefreshTokenRevokedReasons.Expired);
            await refreshTokens.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return RefreshTokenErrors.InvalidRefreshToken;
        }

        var session = await sessionBuilder
            .BuildAsync(stored.TenantId, stored.UserId, cancellationToken)
            .ConfigureAwait(false);

        if (session.IsFailure)
        {
            await sessionRevoker
                .RevokeAsync(stored, RefreshTokenRevokedReasons.UserDisabled, cancellationToken)
                .ConfigureAwait(false);

            return RefreshTokenErrors.InvalidRefreshToken;
        }

        var replacementValue = refreshTokenGenerator.Generate();
        var replacementId = guidGenerator.NewGuid();
        var expiresAt = now.AddDays(refreshTokenOptions.LifetimeDays);

        var rotation = stored.Rotate(replacementId, replacementValue.Hash, now, expiresAt);

        if (rotation.IsFailure)
        {
            return rotation.Error;
        }

        var replacement = RefreshToken.Issue(
            replacementId,
            stored.TenantId,
            stored.UserId,
            stored.SessionId,
            replacementValue.Hash,
            command.DeviceId ?? stored.DeviceId,
            command.IpAddress ?? stored.IpAddress,
            command.UserAgent ?? stored.UserAgent,
            now,
            expiresAt);

        if (replacement.IsFailure)
        {
            return replacement.Error;
        }

        await refreshTokens.AddAsync(replacement.Value, cancellationToken).ConfigureAwait(false);
        await refreshTokens.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var accessToken = tokenGenerator.Generate(
            stored.UserId,
            stored.TenantId,
            session.Value.User.Roles,
            stored.SessionId);

        return new LoginResponse(
            accessToken.Token,
            "Bearer",
            accessToken.ExpiresAt,
            session.Value,
            replacementValue.Token,
            expiresAt);
    }

    private Task WriteReuseAuditAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken) =>
        auditLogger.LogAsync(
            new AuditEntry(
                Action: "auth.refresh_token.reuse",
                EntityType: nameof(RefreshToken),
                EntityId: refreshToken.Id.ToString(),
                TenantId: refreshToken.TenantId,
                ActorId: refreshToken.UserId,
                ActorType: "User"),
            failClosed: true,
            cancellationToken);
}
