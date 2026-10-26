namespace OmniManage.Modules.Sales.Domain.Orders;

/// <summary>
/// Input used by the aggregate factory. Identifiers are supplied by the
/// application layer from <c>IGuidGenerator</c>; the domain never creates them.
/// </summary>
public sealed record SalesOrderLineInput(
    Guid Id,
    Guid ItemId,
    decimal Quantity,
    decimal UnitPrice);
