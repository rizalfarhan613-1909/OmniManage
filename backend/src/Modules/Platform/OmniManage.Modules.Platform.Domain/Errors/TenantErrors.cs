using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Platform.Domain.Tenancy;

namespace OmniManage.Modules.Platform.Domain.Errors;

public static class TenantErrors
{
    public static readonly Error InvalidId = new(
        "PLATFORM.TENANT.INVALID_ID",
        "A tenant identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidCode = new(
        "PLATFORM.TENANT.INVALID_CODE",
        "Tenant code must be 3 to 50 characters using letters, digits, hyphens or underscores.",
        ErrorType.Validation);

    public static readonly Error InvalidName = new(
        "PLATFORM.TENANT.INVALID_NAME",
        "Tenant name is required and must not exceed 200 characters.",
        ErrorType.Validation);

    public static readonly Error InvalidPlanCode = new(
        "PLATFORM.TENANT.INVALID_PLAN_CODE",
        "Plan code must be 2 to 50 characters using letters, digits, hyphens or underscores.",
        ErrorType.Validation);

    public static readonly Error InvalidCreatedAt = new(
        "PLATFORM.TENANT.INVALID_CREATED_AT",
        "Tenant creation time is required.",
        ErrorType.Validation);

    public static Error InvalidStatusTransition(TenantStatus status, string action) => new(
        "PLATFORM.TENANT.INVALID_STATUS_TRANSITION",
        $"Cannot {action} a tenant in the '{status}' status.",
        ErrorType.Conflict);
}
