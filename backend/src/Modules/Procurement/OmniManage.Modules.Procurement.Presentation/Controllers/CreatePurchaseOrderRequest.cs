namespace OmniManage.Modules.Procurement.Presentation.Controllers;

public sealed record CreatePurchaseOrderRequest(
    string? OrderNumber,
    Guid SupplierId,
    DateOnly? ExpectedDeliveryDate,
    string? Notes,
    IReadOnlyList<CreatePurchaseOrderLineRequest>? Lines);

public sealed record CreatePurchaseOrderLineRequest(
    Guid ItemId,
    decimal Quantity,
    decimal UnitPrice);
