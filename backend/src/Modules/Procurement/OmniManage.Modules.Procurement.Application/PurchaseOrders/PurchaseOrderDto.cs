using System.Globalization;
using OmniManage.Modules.Procurement.Domain.PurchaseOrders;

namespace OmniManage.Modules.Procurement.Application.PurchaseOrders;

public sealed record PurchaseOrderDto(
    Guid Id,
    string OrderNumber,
    Guid SupplierId,
    string SupplierName,
    string Status,
    string TotalAmount,
    DateOnly? ExpectedDeliveryDate,
    string? Notes,
    IReadOnlyList<PurchaseOrderLineDto> Lines,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt)
{
    internal static PurchaseOrderDto From(PurchaseOrder purchaseOrder)
    {
        ArgumentNullException.ThrowIfNull(purchaseOrder);

        return new PurchaseOrderDto(
            purchaseOrder.Id,
            purchaseOrder.OrderNumber,
            purchaseOrder.SupplierId,
            purchaseOrder.SupplierName,
            purchaseOrder.Status.ToString(),
            FormatAmount(purchaseOrder.TotalAmount),
            purchaseOrder.ExpectedDeliveryDate,
            purchaseOrder.Notes,
            purchaseOrder.Lines.Select(PurchaseOrderLineDto.From).ToArray(),
            purchaseOrder.CreatedAt,
            purchaseOrder.UpdatedAt);
    }

    internal static string FormatAmount(decimal amount) =>
        amount.ToString("0.0000", CultureInfo.InvariantCulture);
}

public sealed record PurchaseOrderLineDto(
    Guid Id,
    Guid ItemId,
    string Quantity,
    string UnitPrice,
    string LineTotal)
{
    internal static PurchaseOrderLineDto From(PurchaseOrderLine line)
    {
        ArgumentNullException.ThrowIfNull(line);

        return new PurchaseOrderLineDto(
            line.Id,
            line.ItemId,
            line.Quantity.ToString("0.0000", CultureInfo.InvariantCulture),
            PurchaseOrderDto.FormatAmount(line.UnitPrice),
            PurchaseOrderDto.FormatAmount(line.LineTotal));
    }
}
