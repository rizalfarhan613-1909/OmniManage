using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.Modules.Manufacturing.Domain.Errors;

public static class WorkOrderErrors
{
    public static readonly Error InvalidId = new(
        "MANUFACTURING.WORK_ORDER.INVALID_ID",
        "A work order identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidTenantId = new(
        "MANUFACTURING.WORK_ORDER.INVALID_TENANT_ID",
        "A tenant identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidOrderNumber = new(
        "MANUFACTURING.WORK_ORDER.INVALID_ORDER_NUMBER",
        "Work order number is required, must start with a letter or digit and must not exceed 50 characters.",
        ErrorType.Validation);

    public static readonly Error OrderNumberAlreadyExists = new(
        "MANUFACTURING.WORK_ORDER.ORDER_NUMBER_ALREADY_EXISTS",
        "A work order with this number already exists.",
        ErrorType.Conflict);

    public static readonly Error InvalidProductItemId = new(
        "MANUFACTURING.WORK_ORDER.INVALID_PRODUCT_ITEM_ID",
        "A finished-good item identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidBillOfMaterialId = new(
        "MANUFACTURING.WORK_ORDER.INVALID_BILL_OF_MATERIAL_ID",
        "A bill of material identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidBillOfMaterialCode = new(
        "MANUFACTURING.WORK_ORDER.INVALID_BILL_OF_MATERIAL_CODE",
        "The bill of material code snapshot is required and must not exceed 50 characters.",
        ErrorType.Validation);

    public static readonly Error InvalidQuantityPlanned = new(
        "MANUFACTURING.WORK_ORDER.INVALID_QUANTITY_PLANNED",
        "Planned quantity must be greater than zero and within the supported range.",
        ErrorType.Validation);

    public static readonly Error InvalidQuantityProduced = new(
        "MANUFACTURING.WORK_ORDER.INVALID_QUANTITY_PRODUCED",
        "Produced quantity cannot be negative or exceed the planned quantity.",
        ErrorType.Validation);

    public static readonly Error InvalidDueDate = new(
        "MANUFACTURING.WORK_ORDER.INVALID_DUE_DATE",
        "Due date cannot be earlier than the work order creation date.",
        ErrorType.Validation);

    public static readonly Error InvalidNotes = new(
        "MANUFACTURING.WORK_ORDER.INVALID_NOTES",
        "Work order notes must not exceed 1000 characters.",
        ErrorType.Validation);

    public static readonly Error InvalidCreatedAt = new(
        "MANUFACTURING.WORK_ORDER.INVALID_CREATED_AT",
        "A valid creation time is required.",
        ErrorType.Validation);

    public static readonly Error InvalidCreatedBy = new(
        "MANUFACTURING.WORK_ORDER.INVALID_CREATED_BY",
        "The user creating the work order is required.",
        ErrorType.Validation);

    public static readonly Error InvalidStatusFilter = new(
        "MANUFACTURING.WORK_ORDER.INVALID_STATUS_FILTER",
        "Status must be Draft, Released, InProgress, Completed or Cancelled.",
        ErrorType.Validation);

    public static readonly Error BillOfMaterialNotFound = new(
        "MANUFACTURING.WORK_ORDER.BILL_OF_MATERIAL_NOT_FOUND",
        "The selected bill of material was not found.",
        ErrorType.NotFound);

    public static readonly Error NotFound = new(
        "MANUFACTURING.WORK_ORDER.NOT_FOUND",
        "The work order was not found.",
        ErrorType.NotFound);
}
