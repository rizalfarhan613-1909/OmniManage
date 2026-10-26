using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.Modules.Sales.Domain.Orders.Events;

public sealed record SalesOrderConfirmedDomainEvent(
    Guid EventId,
    Guid SalesOrderId,
    Guid TenantId,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
