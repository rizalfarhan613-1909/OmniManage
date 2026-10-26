using OmniManage.Modules.AssetMaintenance.Domain.Maintenance;

namespace OmniManage.Modules.AssetMaintenance.Domain.Abstractions;

public interface IMaintenanceOrderRepository
{
    Task AddAsync(MaintenanceOrder maintenanceOrder, CancellationToken cancellationToken = default);

    Task<bool> RequestNumberExistsAsync(
        string requestNumber,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
