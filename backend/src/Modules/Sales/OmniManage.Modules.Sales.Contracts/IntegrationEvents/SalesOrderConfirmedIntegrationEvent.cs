using OmniManage.BuildingBlocks.Contracts;

namespace OmniManage.Modules.Sales.Contracts.IntegrationEvents;

/// <summary>
/// Raised when a draft sales order is confirmed. Consumers must treat the
/// event as immutable and idempotent; new fields are additive within
/// <see cref="SchemaVersion"/> 1.
/// </summary>
public sealed record SalesOrderConfirmedIntegrationEvent(
    Guid EventId,
    Guid TenantId,
    DateTime OccurredOnUtc,
    Guid SalesOrderId,
    Guid CustomerId,
    IReadOnlyList<SalesOrderConfirmedLine> Lines) : IIntegrationEvent
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion => CurrentSchemaVersion;

    public string? CorrelationId => null;
}

public sealed record SalesOrderConfirmedLine(
    Guid ItemId,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal);
