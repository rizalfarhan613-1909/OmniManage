using Microsoft.Extensions.Logging;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Application.IntegrationEvents;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Sales.Contracts.IntegrationEvents;
using OmniManage.Modules.Sales.Domain.Abstractions;
using OmniManage.Modules.Sales.Domain.Errors;
using OmniManage.Modules.Sales.Domain.Orders.Events;

namespace OmniManage.Modules.Sales.Application.Orders.ConfirmSalesOrder;

public sealed class ConfirmSalesOrderCommandHandler(
    ISalesOrderRepository salesOrders,
    IIntegrationEventOutbox outbox,
    IIntegrationEventDispatcher dispatcher,
    ICurrentUser currentUser,
    IGuidGenerator guidGenerator,
    TimeProvider clock,
    ILogger<ConfirmSalesOrderCommandHandler> logger)
    : ICommandHandler<ConfirmSalesOrderCommand, SalesOrderDto>
{
    public async Task<Result<SalesOrderDto>> Handle(
        ConfirmSalesOrderCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var salesOrder = await salesOrders
            .GetForUpdateAsync(command.SalesOrderId, cancellationToken)
            .ConfigureAwait(false);

        if (salesOrder is null)
        {
            return SalesOrderErrors.NotFound;
        }

        var confirmation = salesOrder.Confirm(
            guidGenerator.NewGuid(),
            clock.GetUtcNow(),
            currentUser.UserId);

        if (confirmation.IsFailure)
        {
            return confirmation.Error;
        }

        var domainEvent = salesOrder.DomainEvents
            .OfType<SalesOrderConfirmedDomainEvent>()
            .Single();

        salesOrder.ClearDomainEvents();

        var integrationEvent = new SalesOrderConfirmedIntegrationEvent(
            domainEvent.EventId,
            domainEvent.TenantId,
            domainEvent.OccurredOnUtc.UtcDateTime,
            salesOrder.Id,
            salesOrder.CustomerId,
            salesOrder.Lines
                .Select(line => new SalesOrderConfirmedLine(
                    line.ItemId,
                    line.Quantity,
                    line.UnitPrice,
                    line.LineTotal))
                .ToArray());

        // The order status and the outbox row are committed by the same
        // SaveChanges call, i.e. in one database transaction.
        await outbox.EnqueueAsync(integrationEvent, cancellationToken).ConfigureAwait(false);
        await salesOrders.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await DispatchAfterCommitAsync(integrationEvent, cancellationToken).ConfigureAwait(false);

        return SalesOrderDto.From(salesOrder);
    }

    private async Task DispatchAfterCommitAsync(
        SalesOrderConfirmedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken)
    {
        try
        {
            var dispatched = await dispatcher
                .DispatchAsync(integrationEvent, cancellationToken)
                .ConfigureAwait(false);

            if (dispatched)
            {
                await outbox
                    .MarkProcessedAsync(integrationEvent.EventId, cancellationToken)
                    .ConfigureAwait(false);
                return;
            }

            await outbox
                .MarkFailedAsync(
                    integrationEvent.EventId,
                    "One or more integration event consumers failed.",
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            // The business change is already committed. The unprocessed outbox
            // row remains available for the replay worker, so the request must
            // not fail after the order was confirmed.
            logger.LogError(
                exception,
                "Post-commit dispatch failed for {EventType} {EventId}",
                nameof(SalesOrderConfirmedIntegrationEvent),
                integrationEvent.EventId);
        }
    }
}
