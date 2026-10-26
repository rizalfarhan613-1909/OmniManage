using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Application.Auditing;
using OmniManage.BuildingBlocks.Infrastructure.Persistence.Audit;
using OmniManage.Modules.Platform.Infrastructure.Persistence;

namespace OmniManage.Modules.Platform.Infrastructure.Auditing;

public sealed class AuditLogger(
    PlatformDbContext dbContext,
    IAuditContext auditContext,
    IGuidGenerator guidGenerator,
    TimeProvider clock,
    ILogger<AuditLogger> logger) : IAuditLogger
{
    public async Task LogAsync(
        AuditEntry entry,
        bool failClosed = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        try
        {
            var tenantId = entry.TenantId ?? auditContext.TenantId;
            var prevHash = await GetPreviousHashAsync(tenantId, cancellationToken).ConfigureAwait(false);
            var log = AuditLogFactory.Create(
                guidGenerator.NewGuid(),
                entry,
                auditContext,
                clock.GetUtcNow(),
                prevHash);

            dbContext.AuditLogs.Add(log);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (!failClosed && exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Audit write failed for action {AuditAction}", entry.Action);
        }
    }

    private Task<byte[]?> GetPreviousHashAsync(
        Guid? tenantId,
        CancellationToken cancellationToken)
    {
        if (!tenantId.HasValue)
        {
            return Task.FromResult<byte[]?>(null);
        }

        return dbContext.AuditLogs
            .AsNoTracking()
            .Where(log => log.TenantId == tenantId)
            .OrderByDescending(log => log.OccurredAt)
            .ThenByDescending(log => log.Id)
            .Select(log => log.RowHash)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
