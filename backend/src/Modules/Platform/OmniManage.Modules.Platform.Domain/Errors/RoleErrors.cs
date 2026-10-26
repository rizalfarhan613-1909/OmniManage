using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.Modules.Platform.Domain.Errors;

public static class RoleErrors
{
    public static readonly Error InvalidId = new(
        "PLATFORM.ROLE.INVALID_ID",
        "A role identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidTenantId = new(
        "PLATFORM.ROLE.INVALID_TENANT_ID",
        "A tenant identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidName = new(
        "PLATFORM.ROLE.INVALID_NAME",
        "Role name is required and must not exceed 100 characters.",
        ErrorType.Validation);

    public static readonly Error PermissionNotGranted = new(
        "PLATFORM.ROLE.PERMISSION_NOT_GRANTED",
        "The permission is not granted to this role.",
        ErrorType.NotFound);
}
