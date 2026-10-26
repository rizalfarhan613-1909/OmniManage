using OmniManage.BuildingBlocks.Application;

namespace OmniManage.Modules.Sales.Application.Orders.CreateSalesOrder;

public sealed record CreateSalesOrderCommand(
    string? OrderNumber,
    Guid CustomerId,
    IReadOnlyList<CreateSalesOrderLine>? Lines) : ICommand<SalesOrderDto>;

public sealed record CreateSalesOrderLine(
    Guid ItemId,
    decimal Quantity,
    decimal UnitPrice);
