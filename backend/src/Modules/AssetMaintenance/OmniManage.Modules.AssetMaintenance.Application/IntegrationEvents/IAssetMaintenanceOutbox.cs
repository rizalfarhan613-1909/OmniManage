using OmniManage.BuildingBlocks.Application.IntegrationEvents;

namespace OmniManage.Modules.AssetMaintenance.Application.IntegrationEvents;

/// <summary>
/// Asset and maintenance-owned outbox. Implemented in Infrastructure so the
/// command handlers stay independent of Entity Framework Core.
/// </summary>
public interface IAssetMaintenanceOutbox : IIntegrationEventOutbox
{
}
