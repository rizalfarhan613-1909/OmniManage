using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.Modules.Inventory.Domain.Errors;

public static class StockErrors
{
    public static readonly Error InvalidId = new(
        "INVENTORY.STOCK.INVALID_ID",
        "A stock ledger entry identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidTenantId = new(
        "INVENTORY.STOCK.INVALID_TENANT_ID",
        "A tenant identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidWarehouseId = new(
        "INVENTORY.STOCK.INVALID_WAREHOUSE_ID",
        "A warehouse identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidItemId = new(
        "INVENTORY.STOCK.INVALID_ITEM_ID",
        "An item identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidQuantityDelta = new(
        "INVENTORY.STOCK.INVALID_QUANTITY_DELTA",
        "Quantity delta must be non-zero and within the supported range.",
        ErrorType.Validation);

    public static readonly Error InvalidMovementType = new(
        "INVENTORY.STOCK.INVALID_MOVEMENT_TYPE",
        "Movement type must be Receive, Issue, Transfer or Adjustment.",
        ErrorType.Validation);

    public static readonly Error InvalidReferenceNumber = new(
        "INVENTORY.STOCK.INVALID_REFERENCE_NUMBER",
        "Reference number is required and must not exceed 100 characters.",
        ErrorType.Validation);

    public static readonly Error InvalidOccurredAtUtc = new(
        "INVENTORY.STOCK.INVALID_OCCURRED_AT_UTC",
        "A valid occurrence time is required.",
        ErrorType.Validation);

    public static readonly Error LedgerIsAppendOnly = new(
        "INVENTORY.STOCK.LEDGER_IS_APPEND_ONLY",
        "Stock ledger entries are append-only and cannot be modified or deleted.",
        ErrorType.Conflict);
}
