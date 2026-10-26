using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.Modules.Platform.Domain.Errors;

public static class PermissionErrors
{
    public static readonly Error InvalidCode = new(
        "PLATFORM.PERMISSION.INVALID_CODE",
        "Permission code must use the format 'module.resource.action' in lowercase snake case.",
        ErrorType.Validation);
}
