using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Sales.Domain.Abstractions;
using OmniManage.Modules.Sales.Domain.Errors;
using OmniManage.Modules.Sales.Domain.Orders;

namespace OmniManage.Modules.Sales.Application.Orders.CreateSalesOrder;

public sealed class CreateSalesOrderCommandHandler(
    ISalesOrderRepository salesOrders,
    ITenantContext tenantContext,
    ICurrentUser currentUser,
    IGuidGenerator guidGenerator,
    TimeProvider clock)
    : ICommandHandler<CreateSalesOrderCommand, SalesOrderDto>
{
    public async Task<Result<SalesOrderDto>> Handle(
        CreateSalesOrderCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var lines = command.Lines?
            .Select(line => new SalesOrderLineInput(
                guidGenerator.NewGuid(),
                line.ItemId,
                line.Quantity,
                line.UnitPrice))
            .ToArray();

        var created = SalesOrder.Create(
            guidGenerator.NewGuid(),
            tenantContext.TenantId,
            command.OrderNumber,
            command.CustomerId,
            lines,
            clock.GetUtcNow(),
            currentUser.UserId);

        if (created.IsFailure)
        {
            return created.Error;
        }

        var salesOrder = created.Value;

        if (await salesOrders
                .OrderNumberExistsAsync(salesOrder.OrderNumber, cancellationToken)
                .ConfigureAwait(false))
        {
            return SalesOrderErrors.OrderNumberAlreadyExists;
        }

        await salesOrders.AddAsync(salesOrder, cancellationToken).ConfigureAwait(false);
        await salesOrders.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return SalesOrderDto.From(salesOrder);
    }
}
