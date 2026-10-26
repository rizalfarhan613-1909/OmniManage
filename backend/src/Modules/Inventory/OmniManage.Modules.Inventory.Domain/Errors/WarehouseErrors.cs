using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.Modules.Inventory.Domain.Errors;

public static class WarehouseErrors
{
    public static readonly Error InvalidId = new(
        "INVENTORY.WAREHOUSE.INVALID_ID",
        "A warehouse identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidTenantId = new(
        "INVENTORY.WAREHOUSE.INVALID_TENANT_ID",
        "A tenant identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidCode = new(
        "INVENTORY.WAREHOUSE.INVALID_CODE",
        "Warehouse code must be 2 to 50 characters starting with a letter or digit and containing only letters, digits, hyphens, underscores or periods.",
        ErrorType.Validation);

    public static readonly Error InvalidName = new(
        "INVENTORY.WAREHOUSE.INVALID_NAME",
        "Warehouse name is required and must not exceed 200 characters.",
        ErrorType.Validation);

    public static readonly Error InvalidAddress = new(
        "INVENTORY.WAREHOUSE.INVALID_ADDRESS",
        "Warehouse address must not exceed 500 characters.",
        ErrorType.Validation);

    public static readonly Error InvalidCreatedAt = new(
        "INVENTORY.WAREHOUSE.INVALID_CREATED_AT",
        "A valid creation time is required.",
        ErrorType.Validation);

    public static readonly Error InvalidCreatedBy = new(
        "INVENTORY.WAREHOUSE.INVALID_CREATED_BY",
        "The user creating the warehouse is required.",
        ErrorType.Validation);

    public static readonly Error InvalidUpdatedAt = new(
        "INVENTORY.WAREHOUSE.INVALID_UPDATED_AT",
        "A valid update time is required.",
        ErrorType.Validation);

    public static readonly Error InvalidUpdatedBy = new(
        "INVENTORY.WAREHOUSE.INVALID_UPDATED_BY",
        "The user updating the warehouse is required.",
        ErrorType.Validation);

    public static readonly Error CodeAlreadyExists = new(
        "INVENTORY.WAREHOUSE.CODE_ALREADY_EXISTS",
        "A warehouse with this code already exists.",
        ErrorType.Conflict);

    public static readonly Error NotFound = new(
        "INVENTORY.WAREHOUSE.NOT_FOUND",
        "The warehouse was not found.",
        ErrorType.NotFound);

    public static readonly Error Inactive = new(
        "INVENTORY.WAREHOUSE.INACTIVE",
        "Stock movements cannot be recorded against an inactive warehouse.",
        ErrorType.Validation);
}
