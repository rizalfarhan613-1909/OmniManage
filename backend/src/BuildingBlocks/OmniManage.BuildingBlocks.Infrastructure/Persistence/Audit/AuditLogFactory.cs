using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using OmniManage.BuildingBlocks.Application.Auditing;

namespace OmniManage.BuildingBlocks.Infrastructure.Persistence.Audit;

public static class AuditLogFactory
{
    public static AuditLog Create(
        Guid id,
        AuditEntry entry,
        IAuditContext context,
        DateTimeOffset occurredAt,
        byte[]? prevHash = null)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(context);

        var log = new AuditLog
        {
            Id = id,
            TenantId = entry.TenantId ?? context.TenantId,
            OccurredAt = occurredAt,
            ActorId = entry.ActorId ?? context.ActorId,
            ActorType = entry.ActorType ?? context.ActorType,
            ImpersonatedBy = entry.ImpersonatedBy ?? context.ImpersonatedBy,
            Action = entry.Action,
            ModuleCode = entry.ModuleCode,
            EntityType = entry.EntityType,
            EntityId = entry.EntityId,
            BeforeData = AuditJson.Serialize(entry.Before),
            AfterData = AuditJson.Serialize(entry.After),
            IpAddress = ParseIpAddress(entry.IpAddress ?? context.IpAddress),
            UserAgent = entry.UserAgent ?? context.UserAgent,
            TraceId = entry.TraceId ?? context.TraceId,
            PrevHash = prevHash,
        };

        log.RowHash = ComputeRowHash(log, prevHash);

        return log;
    }

    private static byte[] ComputeRowHash(AuditLog log, byte[]? prevHash)
    {
        var canonical = string.Join(
            '|',
            log.Id.ToString("D", CultureInfo.InvariantCulture),
            log.TenantId?.ToString("D", CultureInfo.InvariantCulture) ?? string.Empty,
            log.OccurredAt.ToString("O", CultureInfo.InvariantCulture),
            log.ActorId?.ToString("D", CultureInfo.InvariantCulture) ?? string.Empty,
            log.ActorType,
            log.ImpersonatedBy?.ToString("D", CultureInfo.InvariantCulture) ?? string.Empty,
            log.Action,
            log.ModuleCode ?? string.Empty,
            log.EntityType ?? string.Empty,
            log.EntityId ?? string.Empty,
            log.BeforeData ?? string.Empty,
            log.AfterData ?? string.Empty,
            log.IpAddress?.ToString() ?? string.Empty,
            log.UserAgent ?? string.Empty,
            log.TraceId ?? string.Empty);

        var payload = Encoding.UTF8.GetBytes(canonical);

        if (prevHash is null || prevHash.Length == 0)
        {
            return SHA256.HashData(payload);
        }

        var chained = new byte[prevHash.Length + payload.Length];
        Buffer.BlockCopy(prevHash, 0, chained, 0, prevHash.Length);
        Buffer.BlockCopy(payload, 0, chained, prevHash.Length, payload.Length);

        return SHA256.HashData(chained);
    }

    private static System.Net.IPAddress? ParseIpAddress(string? value) =>
        System.Net.IPAddress.TryParse(value, out var address) ? address : null;
}
