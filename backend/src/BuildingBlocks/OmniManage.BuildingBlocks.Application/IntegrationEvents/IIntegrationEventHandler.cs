using OmniManage.BuildingBlocks.Contracts;

namespace OmniManage.BuildingBlocks.Application.IntegrationEvents;

/// <summary>
/// Non-generic contract used by the in-process dispatcher to address handlers
/// without knowing their concrete event type at compile time.
/// </summary>
public interface IIntegrationEventHandler
{
    string Name { get; }

    string RequiredModuleCode { get; }

    Type EventType { get; }

    Task HandleAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// A consumer of a single integration event. The consumer guard (license check
/// and inbox idempotency) is implemented by the handler itself so that the
/// guard runs inside the consumer's own module transaction.
/// </summary>
public interface IIntegrationEventHandler<in TEvent> : IIntegrationEventHandler
    where TEvent : IIntegrationEvent
{
    Task HandleAsync(TEvent integrationEvent, CancellationToken cancellationToken = default);

    string IIntegrationEventHandler.Name => GetType().FullName ?? GetType().Name;

    Type IIntegrationEventHandler.EventType => typeof(TEvent);

    Task IIntegrationEventHandler.HandleAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken) =>
        HandleAsync((TEvent)integrationEvent, cancellationToken);
}
