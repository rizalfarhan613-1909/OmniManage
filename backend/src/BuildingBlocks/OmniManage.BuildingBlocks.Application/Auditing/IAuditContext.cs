namespace OmniManage.BuildingBlocks.Application.Auditing;

public interface IAuditContext
{
    Guid? TenantId { get; }

    Guid? ActorId { get; }

    string ActorType { get; }

    Guid? ImpersonatedBy { get; }

    string? IpAddress { get; }

    string? UserAgent { get; }

    string? TraceId { get; }
}
