using Microsoft.EntityFrameworkCore;
using OmniManage.Modules.AssetMaintenance.Domain.Abstractions;
using OmniManage.Modules.AssetMaintenance.Domain.Maintenance;
using OmniManage.Modules.AssetMaintenance.Infrastructure.Persistence;

namespace OmniManage.Modules.AssetMaintenance.Infrastructure.Persistence.Repositories;

public sealed class MaintenanceOrderRepository(AssetMaintenanceDbContext dbContext)
    : IMaintenanceOrderRepository
{
    public Task AddAsync(
        MaintenanceOrder maintenanceOrder,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(maintenanceOrder);

        return dbContext.MaintenanceOrders.AddAsync(maintenanceOrder, cancellationToken).AsTask();
    }

    public Task<bool> RequestNumberExistsAsync(
        string requestNumber,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestNumber);

        return dbContext.MaintenanceOrders.AnyAsync(
            maintenanceOrder => maintenanceOrder.RequestNumber == requestNumber,
            cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
