using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.Modules.Platform.Domain.Errors;

public static class TenantModuleErrors
{
    public static readonly Error InvalidTenantId = new(
        "PLATFORM.TENANT_MODULE.INVALID_TENANT_ID",
        "A tenant identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidModuleCode = new(
        "PLATFORM.TENANT_MODULE.INVALID_MODULE_CODE",
        "Module code must be 3 to 50 uppercase letters, digits or underscores.",
        ErrorType.Validation);

    public static readonly Error InvalidChangedBy = new(
        "PLATFORM.TENANT_MODULE.INVALID_CHANGED_BY",
        "The user changing the module license is required.",
        ErrorType.Validation);

    public static readonly Error ChangeReasonRequired = new(
        "PLATFORM.TENANT_MODULE.CHANGE_REASON_REQUIRED",
        "A reason is required when changing a module license.",
        ErrorType.Validation);

    public static readonly Error ChangeReasonTooLong = new(
        "PLATFORM.TENANT_MODULE.CHANGE_REASON_TOO_LONG",
        "The module license change reason must not exceed 500 characters.",
        ErrorType.Validation);

    public static readonly Error InvalidActivationTime = new(
        "PLATFORM.TENANT_MODULE.INVALID_ACTIVATION_TIME",
        "A valid activation time is required.",
        ErrorType.Validation);

    public static readonly Error InvalidExpiry = new(
        "PLATFORM.TENANT_MODULE.INVALID_EXPIRY",
        "Module expiry must be later than the activation time.",
        ErrorType.Validation);

    public static readonly Error AlreadyActive = new(
        "PLATFORM.TENANT_MODULE.ALREADY_ACTIVE",
        "The module is already active for this tenant.",
        ErrorType.Conflict);

    public static readonly Error AlreadyInactive = new(
        "PLATFORM.TENANT_MODULE.ALREADY_INACTIVE",
        "The module is already inactive for this tenant.",
        ErrorType.Conflict);

    public static readonly Error NotActive = new(
        "PLATFORM.TENANT_MODULE.NOT_ACTIVE",
        "The module must be active before its expiry can be changed.",
        ErrorType.Conflict);
}
