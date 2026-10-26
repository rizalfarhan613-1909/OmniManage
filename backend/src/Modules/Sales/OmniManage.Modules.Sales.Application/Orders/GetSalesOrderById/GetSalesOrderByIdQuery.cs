using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.Modules.Sales.Application.Orders.GetSalesOrderById;

public sealed record GetSalesOrderByIdQuery(Guid SalesOrderId) : IQuery<Result<SalesOrderDto>>;
