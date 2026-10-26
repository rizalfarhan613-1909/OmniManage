using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.Modules.AssetMaintenance.Domain.Errors;

public static class MaintenanceOrderErrors
{
    public static readonly Error InvalidId = new(
        "ASSETS.MAINTENANCE_ORDER.INVALID_ID",
        "A maintenance order identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidTenantId = new(
        "ASSETS.MAINTENANCE_ORDER.INVALID_TENANT_ID",
        "A tenant identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidAssetId = new(
        "ASSETS.MAINTENANCE_ORDER.INVALID_ASSET_ID",
        "An asset identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidRequestNumber = new(
        "ASSETS.MAINTENANCE_ORDER.INVALID_REQUEST_NUMBER",
        "Maintenance request number must be 2 to 50 characters starting with a letter or digit and containing only letters, digits, hyphens, underscores or periods.",
        ErrorType.Validation);

    public static readonly Error InvalidScheduledDate = new(
        "ASSETS.MAINTENANCE_ORDER.INVALID_SCHEDULED_DATE",
        "The scheduled date must be a valid date when supplied.",
        ErrorType.Validation);

    public static readonly Error InvalidCompletedDate = new(
        "ASSETS.MAINTENANCE_ORDER.INVALID_COMPLETED_DATE",
        "A valid completion date is required.",
        ErrorType.Validation);

    public static readonly Error InvalidCost = new(
        "ASSETS.MAINTENANCE_ORDER.INVALID_COST",
        "Maintenance cost must not be negative or exceed the supported monetary range.",
        ErrorType.Validation);

    public static readonly Error InvalidDescription = new(
        "ASSETS.MAINTENANCE_ORDER.INVALID_DESCRIPTION",
        "Maintenance description is required and must not exceed 1000 characters.",
        ErrorType.Validation);

    public static readonly Error InvalidStatusTransition = new(
        "ASSETS.MAINTENANCE_ORDER.INVALID_STATUS_TRANSITION",
        "The requested maintenance order status transition is not allowed from the current status.",
        ErrorType.Conflict);

    public static readonly Error RequestNumberAlreadyExists = new(
        "ASSETS.MAINTENANCE_ORDER.REQUEST_NUMBER_ALREADY_EXISTS",
        "A maintenance order with this request number already exists in the tenant.",
        ErrorType.Conflict);
}
