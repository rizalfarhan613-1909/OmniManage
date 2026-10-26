using Microsoft.Extensions.Logging;
using OmniManage.BuildingBlocks.Application.IntegrationEvents;
using OmniManage.BuildingBlocks.Contracts;

namespace OmniManage.BuildingBlocks.Infrastructure.IntegrationEvents;

public sealed class IntegrationEventDispatcher(
    IEnumerable<IIntegrationEventHandler> handlers,
    ILogger<IntegrationEventDispatcher> logger) : IIntegrationEventDispatcher
{
    public async Task<bool> DispatchAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        var consumers = handlers
            .Where(handler => handler.EventType == integrationEvent.GetType())
            .ToArray();

        var succeeded = true;

        foreach (var consumer in consumers)
        {
            try
            {
                await consumer
                    .HandleAsync(integrationEvent, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                succeeded = false;

                logger.LogError(
                    exception,
                    "Integration event consumer {Consumer} failed for {EventType} {EventId}",
                    consumer.Name,
                    integrationEvent.GetType().Name,
                    integrationEvent.EventId);
            }
        }

        return succeeded;
    }
}
