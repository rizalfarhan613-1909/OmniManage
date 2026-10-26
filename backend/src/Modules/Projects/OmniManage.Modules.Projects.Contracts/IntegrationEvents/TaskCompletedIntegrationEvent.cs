using OmniManage.BuildingBlocks.Contracts;

namespace OmniManage.Modules.Projects.Contracts.IntegrationEvents;

/// <summary>
/// Raised when a project task is completed. Consumed by the KPI Generator and
/// dashboards. Immutable; new fields are additive within schema version 1.
/// </summary>
public sealed record TaskCompletedIntegrationEvent(
    Guid EventId,
    Guid TenantId,
    DateTime OccurredOnUtc,
    Guid TaskId,
    Guid ProjectId,
    Guid CompletedByEmployeeId,
    string TaskTitle,
    DateTime CompletedAtUtc) : IIntegrationEvent
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion => CurrentSchemaVersion;

    public string? CorrelationId => null;
}
