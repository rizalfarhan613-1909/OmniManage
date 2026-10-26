namespace OmniManage.BuildingBlocks.Application.Auditing;

public sealed record AuditEntry(
    string Action,
    string? ModuleCode = null,
    string? EntityType = null,
    string? EntityId = null,
    object? Before = null,
    object? After = null,
    Guid? TenantId = null,
    Guid? ActorId = null,
    string? ActorType = null,
    Guid? ImpersonatedBy = null,
    string? IpAddress = null,
    string? UserAgent = null,
    string? TraceId = null);
