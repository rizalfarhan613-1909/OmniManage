using OmniManage.BuildingBlocks.Application;

namespace OmniManage.Modules.Inventory.Application.Stock.AdjustStock;

public sealed record AdjustStockCommand(
    Guid WarehouseId,
    Guid ItemId,
    decimal QuantityDelta,
    string? MovementType,
    string? ReferenceNumber,
    DateTimeOffset? OccurredAtUtc = null) : ICommand<StockEntryDto>;
