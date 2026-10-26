using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.Modules.AssetMaintenance.Domain.Errors;

public static class AssetErrors
{
    public static readonly Error InvalidId = new(
        "ASSETS.ASSET.INVALID_ID",
        "An asset identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidTenantId = new(
        "ASSETS.ASSET.INVALID_TENANT_ID",
        "A tenant identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidAssetCode = new(
        "ASSETS.ASSET.INVALID_ASSET_CODE",
        "Asset code must be 2 to 50 characters starting with a letter or digit and containing only letters, digits, hyphens, underscores or periods.",
        ErrorType.Validation);

    public static readonly Error InvalidName = new(
        "ASSETS.ASSET.INVALID_NAME",
        "Asset name is required and must not exceed 200 characters.",
        ErrorType.Validation);

    public static readonly Error InvalidCategory = new(
        "ASSETS.ASSET.INVALID_CATEGORY",
        "Asset category is required and must not exceed 100 characters.",
        ErrorType.Validation);

    public static readonly Error InvalidSerialNumber = new(
        "ASSETS.ASSET.INVALID_SERIAL_NUMBER",
        "Asset serial number must not exceed 100 characters.",
        ErrorType.Validation);

    public static readonly Error InvalidQrCodeUrl = new(
        "ASSETS.ASSET.INVALID_QR_CODE_URL",
        "Asset QR code URL must not exceed 1000 characters.",
        ErrorType.Validation);

    public static readonly Error InvalidPurchaseValue = new(
        "ASSETS.ASSET.INVALID_PURCHASE_VALUE",
        "Purchase value must not be negative or exceed the supported monetary range.",
        ErrorType.Validation);

    public static readonly Error InvalidAcquisitionDate = new(
        "ASSETS.ASSET.INVALID_ACQUISITION_DATE",
        "A valid acquisition date is required.",
        ErrorType.Validation);

    public static readonly Error InvalidStatusTransition = new(
        "ASSETS.ASSET.INVALID_STATUS_TRANSITION",
        "The requested asset status transition is not allowed from the current status.",
        ErrorType.Conflict);

    public static readonly Error AssetCodeAlreadyExists = new(
        "ASSETS.ASSET.CODE_ALREADY_EXISTS",
        "An asset with this code already exists in the tenant.",
        ErrorType.Conflict);

    public static readonly Error NotFound = new(
        "ASSETS.ASSET.NOT_FOUND",
        "The asset does not exist in the current tenant.",
        ErrorType.NotFound);
}
