using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.Modules.Procurement.Domain.Errors;

public static class PurchaseOrderErrors
{
    public static readonly Error InvalidId = new(
        "PROCUREMENT.PURCHASE_ORDER.INVALID_ID",
        "A purchase order identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidTenantId = new(
        "PROCUREMENT.PURCHASE_ORDER.INVALID_TENANT_ID",
        "A tenant identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidOrderNumber = new(
        "PROCUREMENT.PURCHASE_ORDER.INVALID_ORDER_NUMBER",
        "Purchase order number is required and must not exceed 50 characters.",
        ErrorType.Validation);

    public static readonly Error OrderNumberAlreadyExists = new(
        "PROCUREMENT.PURCHASE_ORDER.ORDER_NUMBER_ALREADY_EXISTS",
        "A purchase order with this number already exists.",
        ErrorType.Conflict);

    public static readonly Error InvalidSupplierId = new(
        "PROCUREMENT.PURCHASE_ORDER.INVALID_SUPPLIER_ID",
        "A supplier identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidSupplierName = new(
        "PROCUREMENT.PURCHASE_ORDER.INVALID_SUPPLIER_NAME",
        "The supplier name snapshot is required and must not exceed 200 characters.",
        ErrorType.Validation);

    public static readonly Error InvalidLineId = new(
        "PROCUREMENT.PURCHASE_ORDER.INVALID_LINE_ID",
        "A purchase order line identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidItemId = new(
        "PROCUREMENT.PURCHASE_ORDER.INVALID_ITEM_ID",
        "An item identifier is required on every purchase order line.",
        ErrorType.Validation);

    public static readonly Error InvalidQuantity = new(
        "PROCUREMENT.PURCHASE_ORDER.INVALID_QUANTITY",
        "Line quantity must be greater than zero and within the supported range.",
        ErrorType.Validation);

    public static readonly Error InvalidUnitPrice = new(
        "PROCUREMENT.PURCHASE_ORDER.INVALID_UNIT_PRICE",
        "Unit price must be zero or greater and within the supported range.",
        ErrorType.Validation);

    public static readonly Error InvalidLineTotal = new(
        "PROCUREMENT.PURCHASE_ORDER.INVALID_LINE_TOTAL",
        "Line total exceeds the supported monetary range.",
        ErrorType.Validation);

    public static readonly Error NoLines = new(
        "PROCUREMENT.PURCHASE_ORDER.NO_LINES",
        "A purchase order must contain at least one line.",
        ErrorType.Validation);

    public static readonly Error InvalidTotalAmount = new(
        "PROCUREMENT.PURCHASE_ORDER.INVALID_TOTAL_AMOUNT",
        "The purchase order total exceeds the supported monetary range.",
        ErrorType.Validation);

    public static readonly Error InvalidExpectedDeliveryDate = new(
        "PROCUREMENT.PURCHASE_ORDER.INVALID_EXPECTED_DELIVERY_DATE",
        "Expected delivery date cannot be earlier than the order date.",
        ErrorType.Validation);

    public static readonly Error InvalidNotes = new(
        "PROCUREMENT.PURCHASE_ORDER.INVALID_NOTES",
        "Purchase order notes must not exceed 1000 characters.",
        ErrorType.Validation);

    public static readonly Error InvalidCreatedAt = new(
        "PROCUREMENT.PURCHASE_ORDER.INVALID_CREATED_AT",
        "A valid creation time is required.",
        ErrorType.Validation);

    public static readonly Error InvalidCreatedBy = new(
        "PROCUREMENT.PURCHASE_ORDER.INVALID_CREATED_BY",
        "The user creating the purchase order is required.",
        ErrorType.Validation);

    public static readonly Error InvalidStatusFilter = new(
        "PROCUREMENT.PURCHASE_ORDER.INVALID_STATUS_FILTER",
        "Status must be Draft, Approved, PartiallyReceived, Received or Cancelled.",
        ErrorType.Validation);

    public static readonly Error SupplierNotFound = new(
        "PROCUREMENT.PURCHASE_ORDER.SUPPLIER_NOT_FOUND",
        "The selected supplier was not found.",
        ErrorType.NotFound);

    public static readonly Error NotFound = new(
        "PROCUREMENT.PURCHASE_ORDER.NOT_FOUND",
        "The purchase order was not found.",
        ErrorType.NotFound);
}
