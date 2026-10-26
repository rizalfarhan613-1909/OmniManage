using OmniManage.BuildingBlocks.Application.IntegrationEvents;

namespace OmniManage.Modules.Hris.Application.IntegrationEvents;

/// <summary>
/// HRIS-owned outbox. Implemented in Infrastructure so the command handlers
/// stay independent of Entity Framework Core.
/// </summary>
public interface IHrisOutbox : IIntegrationEventOutbox
{
}
