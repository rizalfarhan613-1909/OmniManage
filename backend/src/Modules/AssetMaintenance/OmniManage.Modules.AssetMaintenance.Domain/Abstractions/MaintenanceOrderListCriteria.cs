using OmniManage.Modules.AssetMaintenance.Domain.Maintenance;

namespace OmniManage.Modules.AssetMaintenance.Domain.Abstractions;

public sealed record MaintenanceOrderListCriteria(
    int Skip,
    int Take,
    Guid? AssetId,
    MaintenanceStatus? Status,
    IReadOnlyList<MaintenanceOrderSortOption> Sort);

public sealed record MaintenanceOrderSortOption(
    MaintenanceOrderSortField Field,
    bool Descending);

public enum MaintenanceOrderSortField
{
    RequestNumber = 1,
    ScheduledDate = 2,
    Cost = 3,
}
