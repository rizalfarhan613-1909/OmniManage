namespace OmniManage.Modules.Inventory.Presentation.Controllers;

public sealed record AdjustStockRequest(
    Guid WarehouseId,
    Guid ItemId,
    decimal QuantityDelta,
    string? MovementType,
    string? ReferenceNumber,
    DateTimeOffset? OccurredAtUtc);
