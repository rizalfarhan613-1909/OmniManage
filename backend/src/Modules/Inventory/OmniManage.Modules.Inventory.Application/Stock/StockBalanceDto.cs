using System.Globalization;
using OmniManage.Modules.Inventory.Domain.Abstractions;

namespace OmniManage.Modules.Inventory.Application.Stock;

public sealed record StockBalanceDto(
    Guid ItemId,
    Guid WarehouseId,
    string QuantityOnHand)
{
    internal static StockBalanceDto From(StockBalance balance)
    {
        ArgumentNullException.ThrowIfNull(balance);

        return new StockBalanceDto(
            balance.ItemId,
            balance.WarehouseId,
            balance.QuantityOnHand.ToString("0.0000", CultureInfo.InvariantCulture));
    }
}
