using OmniManage.BuildingBlocks.Application;

namespace OmniManage.Modules.MasterData.Application.Items.CreateItem;

public sealed record CreateItemCommand(
    string? Sku,
    string? Name,
    string? UomCode,
    string? Category,
    decimal BasePrice,
    decimal CostPrice,
    bool IsStockTracked) : ICommand<ItemDto>;
