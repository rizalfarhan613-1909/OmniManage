using OmniManage.Modules.Procurement.Domain.PurchaseOrders;

namespace OmniManage.Modules.Procurement.Domain.Abstractions;

public interface IPurchaseOrderRepository
{
    Task AddAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken = default);

    Task<bool> OrderNumberExistsAsync(
        string orderNumber,
        CancellationToken cancellationToken = default);

    Task<PurchaseOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<PurchaseOrder> PurchaseOrders, long TotalItems)> ListAsync(
        PurchaseOrderListCriteria criteria,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
