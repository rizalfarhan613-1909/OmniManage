using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.Modules.Platform.Domain.Errors;

public static class PlatformUserErrors
{
    public static readonly Error InvalidId = new(
        "PLATFORM.PLATFORM_USER.INVALID_ID",
        "A platform user identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidEmail = new(
        "PLATFORM.PLATFORM_USER.INVALID_EMAIL",
        "A valid email address is required.",
        ErrorType.Validation);

    public static readonly Error InvalidPasswordHash = new(
        "PLATFORM.PLATFORM_USER.INVALID_PASSWORD_HASH",
        "A password hash is required.",
        ErrorType.Validation);

    public static readonly Error InvalidCreatedAt = new(
        "PLATFORM.PLATFORM_USER.INVALID_CREATED_AT",
        "A creation timestamp is required.",
        ErrorType.Validation);
}
