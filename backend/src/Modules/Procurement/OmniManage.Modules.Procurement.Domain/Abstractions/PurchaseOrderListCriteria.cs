using OmniManage.Modules.Procurement.Domain.PurchaseOrders;

namespace OmniManage.Modules.Procurement.Domain.Abstractions;

public sealed record PurchaseOrderListCriteria(
    int Skip,
    int Take,
    string? Search,
    PurchaseOrderStatus? Status,
    Guid? SupplierId,
    IReadOnlyList<PurchaseOrderSortOption> Sort);

public sealed record PurchaseOrderSortOption(PurchaseOrderSortField Field, bool Descending);

public enum PurchaseOrderSortField
{
    OrderNumber = 1,
    SupplierName = 2,
    Status = 3,
    TotalAmount = 4,
    ExpectedDeliveryDate = 5,
    CreatedAt = 6,
}
