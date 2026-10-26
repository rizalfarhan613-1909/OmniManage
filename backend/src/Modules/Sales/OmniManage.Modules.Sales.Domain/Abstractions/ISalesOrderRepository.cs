using OmniManage.Modules.Sales.Domain.Orders;

namespace OmniManage.Modules.Sales.Domain.Abstractions;

public interface ISalesOrderRepository
{
    Task AddAsync(SalesOrder salesOrder, CancellationToken cancellationToken = default);

    Task<bool> OrderNumberExistsAsync(string orderNumber, CancellationToken cancellationToken = default);

    Task<SalesOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<SalesOrder?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<SalesOrder> SalesOrders, long TotalItems)> ListAsync(
        SalesOrderListCriteria criteria,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
