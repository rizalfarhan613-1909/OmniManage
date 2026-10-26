using System.Globalization;
using OmniManage.Modules.Manufacturing.Domain.WorkOrders;

namespace OmniManage.Modules.Manufacturing.Application.WorkOrders;

public sealed record WorkOrderDto(
    Guid Id,
    string OrderNumber,
    Guid ProductItemId,
    Guid BillOfMaterialId,
    string BillOfMaterialCode,
    string Status,
    string QuantityPlanned,
    string QuantityProduced,
    DateOnly? DueDate,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt)
{
    internal static WorkOrderDto From(WorkOrder workOrder)
    {
        ArgumentNullException.ThrowIfNull(workOrder);

        return new WorkOrderDto(
            workOrder.Id,
            workOrder.OrderNumber,
            workOrder.ProductItemId,
            workOrder.BillOfMaterialId,
            workOrder.BillOfMaterialCode,
            workOrder.Status.ToString(),
            FormatQuantity(workOrder.QuantityPlanned),
            FormatQuantity(workOrder.QuantityProduced),
            workOrder.DueDate,
            workOrder.Notes,
            workOrder.CreatedAt,
            workOrder.UpdatedAt);
    }

    internal static string FormatQuantity(decimal quantity) =>
        quantity.ToString("0.0000", CultureInfo.InvariantCulture);
}
