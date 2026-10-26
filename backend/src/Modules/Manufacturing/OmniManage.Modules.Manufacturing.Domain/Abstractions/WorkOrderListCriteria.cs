using OmniManage.Modules.Manufacturing.Domain.WorkOrders;

namespace OmniManage.Modules.Manufacturing.Domain.Abstractions;

public sealed record WorkOrderListCriteria(
    int Skip,
    int Take,
    string? Search,
    WorkOrderStatus? Status,
    Guid? ProductItemId,
    Guid? BillOfMaterialId,
    IReadOnlyList<WorkOrderSortOption> Sort);

public sealed record WorkOrderSortOption(WorkOrderSortField Field, bool Descending);

public enum WorkOrderSortField
{
    OrderNumber = 1,
    Status = 2,
    ProductItemId = 3,
    QuantityPlanned = 4,
    DueDate = 5,
    CreatedAt = 6,
}
