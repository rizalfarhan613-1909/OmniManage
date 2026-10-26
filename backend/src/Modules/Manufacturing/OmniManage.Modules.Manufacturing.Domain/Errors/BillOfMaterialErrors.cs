using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.Modules.Manufacturing.Domain.Errors;

public static class BillOfMaterialErrors
{
    public static readonly Error InvalidId = new(
        "MANUFACTURING.BILL_OF_MATERIAL.INVALID_ID",
        "A bill of material identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidTenantId = new(
        "MANUFACTURING.BILL_OF_MATERIAL.INVALID_TENANT_ID",
        "A tenant identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidCode = new(
        "MANUFACTURING.BILL_OF_MATERIAL.INVALID_CODE",
        "Bill of material code is required, must start with a letter or digit and must not exceed 50 characters.",
        ErrorType.Validation);

    public static readonly Error CodeAlreadyExists = new(
        "MANUFACTURING.BILL_OF_MATERIAL.CODE_ALREADY_EXISTS",
        "A bill of material with this code and version already exists.",
        ErrorType.Conflict);

    public static readonly Error InvalidName = new(
        "MANUFACTURING.BILL_OF_MATERIAL.INVALID_NAME",
        "Bill of material name is required and must not exceed 200 characters.",
        ErrorType.Validation);

    public static readonly Error InvalidProductItemId = new(
        "MANUFACTURING.BILL_OF_MATERIAL.INVALID_PRODUCT_ITEM_ID",
        "A finished-good item identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidVersion = new(
        "MANUFACTURING.BILL_OF_MATERIAL.INVALID_VERSION",
        "Bill of material version must be between 1 and 9999.",
        ErrorType.Validation);

    public static readonly Error InvalidNotes = new(
        "MANUFACTURING.BILL_OF_MATERIAL.INVALID_NOTES",
        "Bill of material notes must not exceed 1000 characters.",
        ErrorType.Validation);

    public static readonly Error InvalidComponentId = new(
        "MANUFACTURING.BILL_OF_MATERIAL.INVALID_COMPONENT_ID",
        "A bill of material component identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidComponentItemId = new(
        "MANUFACTURING.BILL_OF_MATERIAL.INVALID_COMPONENT_ITEM_ID",
        "A component item identifier is required on every bill of material line.",
        ErrorType.Validation);

    public static readonly Error InvalidComponentQuantity = new(
        "MANUFACTURING.BILL_OF_MATERIAL.INVALID_COMPONENT_QUANTITY",
        "Component quantity per unit must be greater than zero and within the supported range.",
        ErrorType.Validation);

    public static readonly Error DuplicateComponentItem = new(
        "MANUFACTURING.BILL_OF_MATERIAL.DUPLICATE_COMPONENT_ITEM",
        "A component item can appear only once on a bill of material.",
        ErrorType.Validation);

    public static readonly Error NoComponents = new(
        "MANUFACTURING.BILL_OF_MATERIAL.NO_COMPONENTS",
        "A bill of material must contain at least one component.",
        ErrorType.Validation);

    public static readonly Error InvalidCreatedAt = new(
        "MANUFACTURING.BILL_OF_MATERIAL.INVALID_CREATED_AT",
        "A valid creation time is required.",
        ErrorType.Validation);

    public static readonly Error InvalidCreatedBy = new(
        "MANUFACTURING.BILL_OF_MATERIAL.INVALID_CREATED_BY",
        "The user creating the bill of material is required.",
        ErrorType.Validation);

    public static readonly Error Inactive = new(
        "MANUFACTURING.BILL_OF_MATERIAL.INACTIVE",
        "Work orders cannot be created from an inactive bill of material.",
        ErrorType.Conflict);

    public static readonly Error NotFound = new(
        "MANUFACTURING.BILL_OF_MATERIAL.NOT_FOUND",
        "The bill of material was not found.",
        ErrorType.NotFound);
}
