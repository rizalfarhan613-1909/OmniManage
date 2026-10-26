using OmniManage.BuildingBlocks.Contracts;

namespace OmniManage.Modules.Finance.Application.IntegrationEvents;

/// <summary>
/// Transactional outbox owned by the Finance module. A module-specific
/// interface is used because every module owns its own outbox table.
/// </summary>
public interface IFinanceOutbox
{
    Task EnqueueAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default);
}
