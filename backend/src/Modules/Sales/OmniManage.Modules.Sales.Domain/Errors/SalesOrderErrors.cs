using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Sales.Domain.Orders;

namespace OmniManage.Modules.Sales.Domain.Errors;

public static class SalesOrderErrors
{
    public static readonly Error InvalidId = new(
        "SALES.ORDER.INVALID_ID",
        "A sales order identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidTenantId = new(
        "SALES.ORDER.INVALID_TENANT_ID",
        "A tenant identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidOrderNumber = new(
        "SALES.ORDER.INVALID_ORDER_NUMBER",
        "Sales order number is required and must not exceed 50 characters.",
        ErrorType.Validation);

    public static readonly Error OrderNumberAlreadyExists = new(
        "SALES.ORDER.ORDER_NUMBER_ALREADY_EXISTS",
        "A sales order with this number already exists.",
        ErrorType.Conflict);

    public static readonly Error InvalidCustomerId = new(
        "SALES.ORDER.INVALID_CUSTOMER_ID",
        "A customer identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidLineId = new(
        "SALES.ORDER.INVALID_LINE_ID",
        "A sales order line identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidItemId = new(
        "SALES.ORDER.INVALID_ITEM_ID",
        "An item identifier is required on every sales order line.",
        ErrorType.Validation);

    public static readonly Error InvalidQuantity = new(
        "SALES.ORDER.INVALID_QUANTITY",
        "Line quantity must be greater than zero and within the supported range.",
        ErrorType.Validation);

    public static readonly Error InvalidUnitPrice = new(
        "SALES.ORDER.INVALID_UNIT_PRICE",
        "Unit price must be zero or greater and within the supported range.",
        ErrorType.Validation);

    public static readonly Error InvalidLineTotal = new(
        "SALES.ORDER.INVALID_LINE_TOTAL",
        "Line total exceeds the supported monetary range.",
        ErrorType.Validation);

    public static readonly Error NoLines = new(
        "SALES.ORDER.NO_LINES",
        "A sales order must contain at least one line.",
        ErrorType.Validation);

    public static readonly Error InvalidTotalAmount = new(
        "SALES.ORDER.INVALID_TOTAL_AMOUNT",
        "The sales order total exceeds the supported monetary range.",
        ErrorType.Validation);

    public static readonly Error InvalidCreatedAt = new(
        "SALES.ORDER.INVALID_CREATED_AT",
        "A valid creation time is required.",
        ErrorType.Validation);

    public static readonly Error InvalidCreatedBy = new(
        "SALES.ORDER.INVALID_CREATED_BY",
        "The user creating the sales order is required.",
        ErrorType.Validation);

    public static readonly Error InvalidUpdatedAt = new(
        "SALES.ORDER.INVALID_UPDATED_AT",
        "A valid update time is required.",
        ErrorType.Validation);

    public static readonly Error InvalidUpdatedBy = new(
        "SALES.ORDER.INVALID_UPDATED_BY",
        "The user updating the sales order is required.",
        ErrorType.Validation);

    public static readonly Error InvalidEventId = new(
        "SALES.ORDER.INVALID_EVENT_ID",
        "A domain event identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidStatusFilter = new(
        "SALES.ORDER.INVALID_STATUS_FILTER",
        "Status must be Draft, Confirmed or Cancelled.",
        ErrorType.Validation);

    public static readonly Error InvalidState = new(
        "SALES.ORDER.INVALID_STATE",
        "Only draft sales orders can be confirmed.",
        ErrorType.Conflict);

    public static readonly Error NotFound = new(
        "SALES.ORDER.NOT_FOUND",
        "The sales order was not found.",
        ErrorType.NotFound);
}
