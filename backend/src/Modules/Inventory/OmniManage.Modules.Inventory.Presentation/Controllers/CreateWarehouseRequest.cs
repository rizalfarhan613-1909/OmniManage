namespace OmniManage.Modules.Inventory.Presentation.Controllers;

public sealed record CreateWarehouseRequest(
    string? Code,
    string? Name,
    string? Address);
