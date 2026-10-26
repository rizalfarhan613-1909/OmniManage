using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.Modules.MasterData.Domain.Errors;

public static class ItemErrors
{
    public static readonly Error InvalidId = new(
        "MASTERDATA.ITEM.INVALID_ID",
        "An item identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidTenantId = new(
        "MASTERDATA.ITEM.INVALID_TENANT_ID",
        "A tenant identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidSku = new(
        "MASTERDATA.ITEM.INVALID_SKU",
        "SKU must be 2 to 50 characters starting with a letter or digit and containing only letters, digits, hyphens, underscores or periods.",
        ErrorType.Validation);

    public static readonly Error InvalidName = new(
        "MASTERDATA.ITEM.INVALID_NAME",
        "Item name is required and must not exceed 200 characters.",
        ErrorType.Validation);

    public static readonly Error InvalidUomCode = new(
        "MASTERDATA.ITEM.INVALID_UOM_CODE",
        "Unit of measure code must be 2 to 10 letters or digits.",
        ErrorType.Validation);

    public static readonly Error InvalidCategory = new(
        "MASTERDATA.ITEM.INVALID_CATEGORY",
        "Item category must not exceed 100 characters.",
        ErrorType.Validation);

    public static readonly Error InvalidBasePrice = new(
        "MASTERDATA.ITEM.INVALID_BASE_PRICE",
        "Base price must be between 0 and 999,999,999,999,999.9999.",
        ErrorType.Validation);

    public static readonly Error InvalidCostPrice = new(
        "MASTERDATA.ITEM.INVALID_COST_PRICE",
        "Cost price must be between 0 and 999,999,999,999,999.9999.",
        ErrorType.Validation);

    public static readonly Error InvalidCreatedAt = new(
        "MASTERDATA.ITEM.INVALID_CREATED_AT",
        "A valid creation time is required.",
        ErrorType.Validation);

    public static readonly Error InvalidCreatedBy = new(
        "MASTERDATA.ITEM.INVALID_CREATED_BY",
        "The user creating the item is required.",
        ErrorType.Validation);

    public static readonly Error SkuAlreadyExists = new(
        "MASTERDATA.ITEM.SKU_ALREADY_EXISTS",
        "An item with this SKU already exists.",
        ErrorType.Conflict);
}
