using OmniManage.BuildingBlocks.Application.IntegrationEvents;
using OmniManage.BuildingBlocks.Contracts;

namespace OmniManage.Modules.Sales.UnitTests.TestDoubles;

internal sealed class FakeIntegrationEventOutbox : IIntegrationEventOutbox
{
    public List<IIntegrationEvent> Enqueued { get; } = [];

    public List<Guid> Processed { get; } = [];

    public List<(Guid EventId, string Error)> Failed { get; } = [];

    public Task EnqueueAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        Enqueued.Add(integrationEvent);
        return Task.CompletedTask;
    }

    public Task MarkProcessedAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        Processed.Add(eventId);
        return Task.CompletedTask;
    }

    public Task MarkFailedAsync(
        Guid eventId,
        string error,
        CancellationToken cancellationToken = default)
    {
        Failed.Add((eventId, error));
        return Task.CompletedTask;
    }
}
