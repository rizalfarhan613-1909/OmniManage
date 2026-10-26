using OmniManage.Modules.Manufacturing.Domain.WorkOrders;

namespace OmniManage.Modules.Manufacturing.Domain.Abstractions;

public interface IWorkOrderRepository
{
    Task AddAsync(WorkOrder workOrder, CancellationToken cancellationToken = default);

    Task<bool> OrderNumberExistsAsync(
        string orderNumber,
        CancellationToken cancellationToken = default);

    Task<WorkOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<WorkOrder> WorkOrders, long TotalItems)> ListAsync(
        WorkOrderListCriteria criteria,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
