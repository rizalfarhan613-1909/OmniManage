using OmniManage.BuildingBlocks.Contracts;

namespace OmniManage.Modules.Finance.Contracts.IntegrationEvents;

/// <summary>
/// Raised after Finance has created an A/R invoice and posted the matching
/// journal entry. Immutable; new fields are additive within schema version 1.
/// </summary>
public sealed record ArInvoicePostedIntegrationEvent(
    Guid EventId,
    Guid TenantId,
    DateTime OccurredOnUtc,
    Guid ArInvoiceId,
    Guid SalesOrderId,
    Guid CustomerId,
    decimal TotalAmount) : IIntegrationEvent
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion => CurrentSchemaVersion;

    public string? CorrelationId => null;
}
