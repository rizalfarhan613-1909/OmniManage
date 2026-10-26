using OmniManage.BuildingBlocks.Application.Auditing;

namespace OmniManage.BuildingBlocks.UnitTests.TestDoubles;

public sealed class TestAuditContext : IAuditContext
{
    public Guid? TenantId { get; init; }

    public Guid? ActorId { get; init; }

    public string ActorType { get; init; } = "User";

    public Guid? ImpersonatedBy { get; init; }

    public string? IpAddress { get; init; } = "203.0.113.10";

    public string? UserAgent { get; init; } = "xunit";

    public string? TraceId { get; init; } = "00-test-trace";
}
