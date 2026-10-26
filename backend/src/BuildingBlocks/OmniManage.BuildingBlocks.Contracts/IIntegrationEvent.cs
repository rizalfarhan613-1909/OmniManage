namespace OmniManage.BuildingBlocks.Contracts;

public interface IIntegrationEvent
{
    Guid EventId { get; }

    Guid TenantId { get; }

    DateTime OccurredOnUtc { get; }

    int SchemaVersion { get; }

    string? CorrelationId { get; }
}
