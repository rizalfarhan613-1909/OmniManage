using OmniManage.Modules.Inventory.Domain.Warehouses;

namespace OmniManage.Modules.Inventory.Application.Warehouses;

public sealed record WarehouseDto(
    Guid Id,
    string Code,
    string Name,
    string? Address,
    bool IsActive,
    DateTimeOffset CreatedAt)
{
    internal static WarehouseDto From(Warehouse warehouse)
    {
        ArgumentNullException.ThrowIfNull(warehouse);

        return new WarehouseDto(
            warehouse.Id,
            warehouse.Code,
            warehouse.Name,
            warehouse.Address,
            warehouse.IsActive,
            warehouse.CreatedAt);
    }
}
