using OmniManage.BuildingBlocks.Contracts;

namespace OmniManage.BuildingBlocks.Application.IntegrationEvents;

/// <summary>
/// Transactional outbox owned by a single module. Enqueue only stages the
/// message; the module's <c>SaveChanges</c> commits it together with the
/// business change in one database transaction.
/// </summary>
public interface IIntegrationEventOutbox
{
    Task EnqueueAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default);

    Task MarkProcessedAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);

    Task MarkFailedAsync(
        Guid eventId,
        string error,
        CancellationToken cancellationToken = default);
}
