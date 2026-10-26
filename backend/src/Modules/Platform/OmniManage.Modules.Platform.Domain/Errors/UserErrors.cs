using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.Modules.Platform.Domain.Errors;

public static class UserErrors
{
    public static readonly Error InvalidId = new(
        "PLATFORM.USER.INVALID_ID",
        "A user identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidTenantId = new(
        "PLATFORM.USER.INVALID_TENANT_ID",
        "A tenant identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidEmail = new(
        "PLATFORM.USER.INVALID_EMAIL",
        "A valid email address is required.",
        ErrorType.Validation);

    public static readonly Error InvalidPasswordHash = new(
        "PLATFORM.USER.INVALID_PASSWORD_HASH",
        "A password hash is required.",
        ErrorType.Validation);

    public static readonly Error RoleTenantMismatch = new(
        "PLATFORM.USER.ROLE_TENANT_MISMATCH",
        "The role does not belong to the user's tenant.",
        ErrorType.Conflict);

    public static readonly Error RoleNotAssigned = new(
        "PLATFORM.USER.ROLE_NOT_ASSIGNED",
        "The role is not assigned to this user.",
        ErrorType.NotFound);
}
