using OmniManage.BuildingBlocks.Contracts;

namespace OmniManage.BuildingBlocks.Application.IntegrationEvents;

/// <summary>
/// Publishes an integration event to every registered consumer in the current
/// process. Returns <see langword="false"/> when at least one consumer failed;
/// the outbox row is then retained for replay.
/// </summary>
public interface IIntegrationEventDispatcher
{
    Task<bool> DispatchAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default);
}
