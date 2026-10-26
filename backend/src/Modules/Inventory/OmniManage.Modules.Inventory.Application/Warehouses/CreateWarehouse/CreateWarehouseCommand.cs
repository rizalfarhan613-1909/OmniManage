using OmniManage.BuildingBlocks.Application;

namespace OmniManage.Modules.Inventory.Application.Warehouses.CreateWarehouse;

public sealed record CreateWarehouseCommand(
    string? Code,
    string? Name,
    string? Address) : ICommand<WarehouseDto>;
