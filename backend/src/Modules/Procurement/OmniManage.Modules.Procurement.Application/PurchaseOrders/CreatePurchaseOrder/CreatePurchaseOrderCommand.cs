using OmniManage.BuildingBlocks.Application;

namespace OmniManage.Modules.Procurement.Application.PurchaseOrders.CreatePurchaseOrder;

public sealed record CreatePurchaseOrderCommand(
    string? OrderNumber,
    Guid SupplierId,
    DateOnly? ExpectedDeliveryDate,
    string? Notes,
    IReadOnlyList<CreatePurchaseOrderLine>? Lines) : ICommand<PurchaseOrderDto>;

public sealed record CreatePurchaseOrderLine(
    Guid ItemId,
    decimal Quantity,
    decimal UnitPrice);
