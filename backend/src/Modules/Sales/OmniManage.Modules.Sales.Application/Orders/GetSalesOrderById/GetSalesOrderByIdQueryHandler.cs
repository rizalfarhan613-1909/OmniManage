using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Sales.Domain.Abstractions;
using OmniManage.Modules.Sales.Domain.Errors;

namespace OmniManage.Modules.Sales.Application.Orders.GetSalesOrderById;

public sealed class GetSalesOrderByIdQueryHandler(ISalesOrderRepository salesOrders)
    : IQueryHandler<GetSalesOrderByIdQuery, Result<SalesOrderDto>>
{
    public async Task<Result<SalesOrderDto>> Handle(
        GetSalesOrderByIdQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var salesOrder = await salesOrders
            .GetByIdAsync(query.SalesOrderId, cancellationToken)
            .ConfigureAwait(false);

        return salesOrder is null
            ? SalesOrderErrors.NotFound
            : SalesOrderDto.From(salesOrder);
    }
}
