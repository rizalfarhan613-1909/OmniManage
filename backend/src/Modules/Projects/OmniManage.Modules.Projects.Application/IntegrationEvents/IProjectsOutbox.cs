using OmniManage.BuildingBlocks.Application.IntegrationEvents;

namespace OmniManage.Modules.Projects.Application.IntegrationEvents;

/// <summary>
/// Projects-owned outbox. Implemented in Infrastructure so the command
/// handlers stay independent of Entity Framework Core.
/// </summary>
public interface IProjectsOutbox : IIntegrationEventOutbox
{
}
