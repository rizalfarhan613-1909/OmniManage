using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.Modules.Platform.Domain.Errors;

public static class LicensingErrors
{
    public static readonly Error ModuleNotLicensed = new(
        "PLATFORM.MODULE_NOT_LICENSED",
        "The requested module is not active for this organization.",
        ErrorType.Forbidden);

    public static readonly Error LicenseUnavailable = new(
        "PLATFORM.LICENSE_UNAVAILABLE",
        "License service unavailable.",
        ErrorType.Failure);
}
