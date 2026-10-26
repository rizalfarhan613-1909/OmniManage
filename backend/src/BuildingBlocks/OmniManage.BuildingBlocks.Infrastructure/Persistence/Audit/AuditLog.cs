using System.Net;

namespace OmniManage.BuildingBlocks.Infrastructure.Persistence.Audit;

public sealed class AuditLog
{
    public Guid Id { get; init; }

    public Guid? TenantId { get; init; }

    public DateTimeOffset OccurredAt { get; init; }

    public Guid? ActorId { get; init; }

    public string ActorType { get; init; } = string.Empty;

    public Guid? ImpersonatedBy { get; init; }

    public string Action { get; init; } = string.Empty;

    public string? ModuleCode { get; init; }

    public string? EntityType { get; init; }

    public string? EntityId { get; init; }

    public string? BeforeData { get; init; }

    public string? AfterData { get; init; }

    public IPAddress? IpAddress { get; init; }

    public string? UserAgent { get; init; }

    public string? TraceId { get; init; }

    public byte[]? PrevHash { get; init; }

    public byte[]? RowHash { get; set; }
}
