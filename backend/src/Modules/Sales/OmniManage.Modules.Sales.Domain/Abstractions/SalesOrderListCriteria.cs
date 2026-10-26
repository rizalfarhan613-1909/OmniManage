using OmniManage.Modules.Sales.Domain.Orders;

namespace OmniManage.Modules.Sales.Domain.Abstractions;

public sealed record SalesOrderListCriteria(
    int Skip,
    int Take,
    string? Search,
    SalesOrderStatus? Status,
    IReadOnlyList<SalesOrderSortOption> Sort);

public sealed record SalesOrderSortOption(SalesOrderSortField Field, bool Descending);

public enum SalesOrderSortField
{
    OrderNumber = 1,
    Status = 2,
    TotalAmount = 3,
    CreatedAt = 4,
}
