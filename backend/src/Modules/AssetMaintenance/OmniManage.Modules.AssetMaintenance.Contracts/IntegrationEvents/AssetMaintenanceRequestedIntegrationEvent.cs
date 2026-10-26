using OmniManage.BuildingBlocks.Contracts;

namespace OmniManage.Modules.AssetMaintenance.Contracts.IntegrationEvents;

/// <summary>
/// Raised when maintenance is requested for an asset. Manufacturing consumes
/// it to plan spare-part demand and Finance to accrue maintenance cost.
/// Immutable; new fields are additive within schema version 1.
/// </summary>
public sealed record AssetMaintenanceRequestedIntegrationEvent(
    Guid EventId,
    Guid TenantId,
    DateTime OccurredOnUtc,
    Guid WorkOrderId,
    Guid AssetId,
    string AssetCode,
    string MaintenanceType,
    string Priority,
    string? Description,
    DateTime RequestedAtUtc) : IIntegrationEvent
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion => CurrentSchemaVersion;

    public string? CorrelationId => null;
}
