using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Inventory.Domain.Errors;

namespace OmniManage.Modules.Inventory.Domain.Stock;

/// <summary>
/// Immutable, append-only stock movement entry. Balances are always derived by
/// aggregating the ledger; entries are never updated or deleted.
/// </summary>
public sealed class StockLedger : AggregateRoot<Guid>, ITenantEntity
{
    public const int ReferenceNumberMaxLength = 100;
    public const decimal QuantityDeltaMax = 999_999_999_999_999.9999m;

    private StockLedger()
    {
    }

    private StockLedger(
        Guid id,
        Guid tenantId,
        Guid warehouseId,
        Guid itemId,
        decimal quantityDelta,
        MovementType movementType,
        string referenceNumber,
        DateTimeOffset occurredAtUtc)
        : base(id)
    {
        TenantId = tenantId;
        WarehouseId = warehouseId;
        ItemId = itemId;
        QuantityDelta = quantityDelta;
        MovementType = movementType;
        ReferenceNumber = referenceNumber;
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid TenantId { get; private set; }

    public Guid WarehouseId { get; private set; }

    public Guid ItemId { get; private set; }

    public decimal QuantityDelta { get; private set; }

    public MovementType MovementType { get; private set; }

    public string ReferenceNumber { get; private set; } = string.Empty;

    public DateTimeOffset OccurredAtUtc { get; private set; }

    public static Result<StockLedger> Create(
        Guid id,
        Guid tenantId,
        Guid warehouseId,
        Guid itemId,
        decimal quantityDelta,
        string? movementType,
        string? referenceNumber,
        DateTimeOffset occurredAtUtc)
    {
        if (id == Guid.Empty)
        {
            return StockErrors.InvalidId;
        }

        if (tenantId == Guid.Empty)
        {
            return StockErrors.InvalidTenantId;
        }

        if (warehouseId == Guid.Empty)
        {
            return StockErrors.InvalidWarehouseId;
        }

        if (itemId == Guid.Empty)
        {
            return StockErrors.InvalidItemId;
        }

        if (quantityDelta is 0m or > QuantityDeltaMax or < -QuantityDeltaMax)
        {
            return StockErrors.InvalidQuantityDelta;
        }

        if (!TryParseMovementType(movementType, out var parsedMovementType))
        {
            return StockErrors.InvalidMovementType;
        }

        if (!TryNormalizeReferenceNumber(referenceNumber, out var normalizedReferenceNumber))
        {
            return StockErrors.InvalidReferenceNumber;
        }

        if (occurredAtUtc == default)
        {
            return StockErrors.InvalidOccurredAtUtc;
        }

        return new StockLedger(
            id,
            tenantId,
            warehouseId,
            itemId,
            quantityDelta,
            parsedMovementType,
            normalizedReferenceNumber,
            occurredAtUtc);
    }

    private static bool TryParseMovementType(string? value, out MovementType movementType)
    {
        movementType = default;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim();

        if (candidate.All(char.IsAsciiDigit)
            || !Enum.TryParse(candidate, ignoreCase: true, out movementType))
        {
            return false;
        }

        return Enum.IsDefined(movementType);
    }

    private static bool TryNormalizeReferenceNumber(string? value, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim();

        if (candidate.Length > ReferenceNumberMaxLength)
        {
            return false;
        }

        normalized = candidate;
        return true;
    }
}
