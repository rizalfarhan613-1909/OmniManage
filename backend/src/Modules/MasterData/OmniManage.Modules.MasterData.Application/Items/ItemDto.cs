using System.Globalization;
using OmniManage.Modules.MasterData.Domain.Items;

namespace OmniManage.Modules.MasterData.Application.Items;

public sealed record ItemDto(
    Guid Id,
    string Sku,
    string Name,
    string UomCode,
    string? Category,
    string BasePrice,
    string CostPrice,
    bool IsStockTracked,
    bool IsActive,
    DateTimeOffset CreatedAt)
{
    internal static ItemDto From(Item item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return new ItemDto(
            item.Id,
            item.Sku,
            item.Name,
            item.UomCode,
            item.Category,
            item.BasePrice.ToString("0.0000", CultureInfo.InvariantCulture),
            item.CostPrice.ToString("0.0000", CultureInfo.InvariantCulture),
            item.IsStockTracked,
            item.IsActive,
            item.CreatedAt);
    }
}
