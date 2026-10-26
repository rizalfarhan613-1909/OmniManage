namespace OmniManage.Modules.Sales.Presentation.Controllers;

public sealed record CreateSalesOrderRequest(
    string? OrderNumber,
    Guid CustomerId,
    IReadOnlyList<CreateSalesOrderLineRequest>? Lines);

public sealed record CreateSalesOrderLineRequest(
    Guid ItemId,
    decimal Quantity,
    decimal UnitPrice);
