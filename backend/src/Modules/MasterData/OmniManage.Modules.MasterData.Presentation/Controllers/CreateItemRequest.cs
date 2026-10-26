namespace OmniManage.Modules.MasterData.Presentation.Controllers;

public sealed record CreateItemRequest(
    string? Sku,
    string? Name,
    string? UomCode,
    string? Category,
    decimal BasePrice,
    decimal CostPrice,
    bool IsStockTracked);
