using System.Globalization;
using OmniManage.Modules.Inventory.Domain.Stock;

namespace OmniManage.Modules.Inventory.Application.Stock;

public sealed record StockEntryDto(
    Guid Id,
    Guid WarehouseId,
    Guid ItemId,
    string QuantityDelta,
    string MovementType,
    string ReferenceNumber,
    DateTimeOffset OccurredAtUtc)
{
    internal static StockEntryDto From(StockLedger entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        return new StockEntryDto(
            entry.Id,
            entry.WarehouseId,
            entry.ItemId,
            entry.QuantityDelta.ToString("0.0000", CultureInfo.InvariantCulture),
            entry.MovementType.ToString(),
            entry.ReferenceNumber,
            entry.OccurredAtUtc);
    }
}
