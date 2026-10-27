using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Application.Auditing;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Platform.Application.Admin.Dtos;
using OmniManage.Modules.Platform.Domain.Abstractions;
using OmniManage.Modules.Platform.Domain.Identity;

namespace OmniManage.Modules.Platform.Application.Admin.Commands;

public sealed class TerminateSessionCommandHandler(
    IRefreshTokenRepository refreshTokens,
    ITenantContext tenantContext,
    IAuditLogger auditLogger,
    TimeProvider clock)
    : ICommandHandler<TerminateSessionCommand, TerminateSessionResultDto>
{
    private const string TerminatedReason = "ADMIN_TERMINATED";

    public async Task<Result<TerminateSessionResultDto>> Handle(
        TerminateSessionCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.SessionId == Guid.Empty)
        {
            return SessionErrors.InvalidSessionId;
        }

        var tokens = await refreshTokens
            .GetSessionTokensAsync(tenantContext.TenantId, command.SessionId, cancellationToken)
            .ConfigureAwait(false);

        if (tokens.Count == 0)
        {
            return SessionErrors.NotFound;
        }

        var now = clock.GetUtcNow();

        foreach (var token in tokens)
        {
            token.Revoke(now, TerminatedReason);
        }

        await refreshTokens.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await auditLogger.LogAsync(
            new AuditEntry(
                Action: "platform.session.terminate",
                ModuleCode: ModuleCodes.PlatformCore,
                EntityType: nameof(RefreshToken),
                EntityId: command.SessionId.ToString(),
                TenantId: tenantContext.TenantId,
                ActorType: "Superadmin"),
            failClosed: true,
            cancellationToken).ConfigureAwait(false);

        return new TerminateSessionResultDto(true);
    }
}
