using OmniManage.BuildingBlocks.Application;

namespace OmniManage.Modules.Sales.Application.Orders.ConfirmSalesOrder;

public sealed record ConfirmSalesOrderCommand(Guid SalesOrderId) : ICommand<SalesOrderDto>;
