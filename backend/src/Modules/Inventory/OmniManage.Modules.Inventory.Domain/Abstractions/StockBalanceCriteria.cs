namespace OmniManage.Modules.Inventory.Domain.Abstractions;

public sealed record StockBalanceCriteria(
    int Skip,
    int Take,
    Guid? WarehouseId,
    Guid? ItemId);

public sealed record StockBalance(Guid ItemId, Guid WarehouseId, decimal QuantityOnHand);
