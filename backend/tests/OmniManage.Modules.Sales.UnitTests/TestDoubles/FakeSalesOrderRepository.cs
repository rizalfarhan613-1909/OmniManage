using OmniManage.Modules.Sales.Domain.Abstractions;
using OmniManage.Modules.Sales.Domain.Orders;

namespace OmniManage.Modules.Sales.UnitTests.TestDoubles;

internal sealed class FakeSalesOrderRepository : ISalesOrderRepository
{
    public List<SalesOrder> Orders { get; } = [];

    public bool OrderNumberExists { get; set; }

    public SalesOrderListCriteria? LastCriteria { get; private set; }

    public int SaveCount { get; private set; }

    public Task AddAsync(SalesOrder salesOrder, CancellationToken cancellationToken = default)
    {
        Orders.Add(salesOrder);
        return Task.CompletedTask;
    }

    public Task<bool> OrderNumberExistsAsync(
        string orderNumber,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(OrderNumberExists);

    public Task<SalesOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Orders.SingleOrDefault(salesOrder => salesOrder.Id == id));

    public Task<SalesOrder?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Orders.SingleOrDefault(salesOrder => salesOrder.Id == id));

    public Task<(IReadOnlyList<SalesOrder> SalesOrders, long TotalItems)> ListAsync(
        SalesOrderListCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        LastCriteria = criteria;

        IEnumerable<SalesOrder> query = Orders;

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            query = query.Where(salesOrder => salesOrder.OrderNumber.Contains(
                criteria.Search,
                StringComparison.OrdinalIgnoreCase));
        }

        if (criteria.Status is { } status)
        {
            query = query.Where(salesOrder => salesOrder.Status == status);
        }

        IReadOnlyList<SalesOrder> page = query
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .ToArray();

        return Task.FromResult((page, (long)query.Count()));
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;
        return Task.CompletedTask;
    }
}
