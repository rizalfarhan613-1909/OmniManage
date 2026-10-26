using Microsoft.Extensions.Logging;
using OmniManage.BuildingBlocks.Contracts;

namespace OmniManage.BuildingBlocks.Application.IntegrationEvents;

/// <summary>
/// Post-commit dispatch shared by command handlers. The business change and
/// the outbox row are already committed; a dispatch failure must therefore be
/// logged and left in the outbox for replay instead of failing the request.
/// </summary>
public static class IntegrationEventDispatch
{
    public static async Task DispatchAfterCommitAsync(
        IIntegrationEventOutbox outbox,
        IIntegrationEventDispatcher dispatcher,
        IIntegrationEvent integrationEvent,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(outbox);
        ArgumentNullException.ThrowIfNull(dispatcher);
        ArgumentNullException.ThrowIfNull(integrationEvent);
        ArgumentNullException.ThrowIfNull(logger);

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
            logger.LogError(
                exception,
                "Post-commit dispatch failed for {EventType} {EventId}",
                integrationEvent.GetType().Name,
                integrationEvent.EventId);
        }
    }
}
