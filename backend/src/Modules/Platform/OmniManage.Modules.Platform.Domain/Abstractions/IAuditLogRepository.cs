namespace OmniManage.Modules.Platform.Domain.Abstractions;

public sealed record AuditLogListCriteria(
    int Skip,
    int Take,
    string? Search,
    string? Action,
    string? ModuleCode,
    IReadOnlyList<AuditLogSortOption> Sort);

public sealed record AuditLogSortOption(AuditLogSortField Field, bool Descending);

public enum AuditLogSortField
{
    OccurredAt = 1,
    Action = 2,
}

public sealed record AuditLogProjection(
    Guid Id,
    Guid? TenantId,
    DateTimeOffset OccurredAt,
    Guid? ActorId,
    string? ActorName,
    string ActorType,
    string Action,
    string? ModuleCode,
    string? EntityType,
    string? EntityId,
    string? BeforeData,
    string? AfterData,
    string? IpAddress,
    string? UserAgent,
    string? TraceId,
    byte[]? RowHash,
    byte[]? PrevHash);

public interface IAuditLogRepository
{
    Task<(IReadOnlyList<AuditLogProjection> Items, long TotalItems)> ListAsync(
        Guid? tenantId,
        AuditLogListCriteria criteria,
        CancellationToken cancellationToken = default);
}
