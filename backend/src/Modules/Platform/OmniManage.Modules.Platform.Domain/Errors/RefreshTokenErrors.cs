using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.Modules.Platform.Domain.Errors;

public static class RefreshTokenErrors
{
    public static readonly Error InvalidId = new(
        "PLATFORM.REFRESH_TOKEN.INVALID_ID",
        "A refresh token identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidTenantId = new(
        "PLATFORM.REFRESH_TOKEN.INVALID_TENANT_ID",
        "A tenant identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidUserId = new(
        "PLATFORM.REFRESH_TOKEN.INVALID_USER_ID",
        "A user identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidSessionId = new(
        "PLATFORM.REFRESH_TOKEN.INVALID_SESSION_ID",
        "A session identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidTokenHash = new(
        "PLATFORM.REFRESH_TOKEN.INVALID_TOKEN_HASH",
        "A valid SHA-256 token hash is required.",
        ErrorType.Validation);

    public static readonly Error InvalidExpiry = new(
        "PLATFORM.REFRESH_TOKEN.INVALID_EXPIRY",
        "The refresh token expiry must be later than its issue time.",
        ErrorType.Validation);

    public static readonly Error Expired = new(
        "PLATFORM.REFRESH_TOKEN.EXPIRED",
        "The refresh token has expired.",
        ErrorType.Unauthorized);

    public static readonly Error AlreadyRevoked = new(
        "PLATFORM.REFRESH_TOKEN.ALREADY_REVOKED",
        "The refresh token has already been revoked.",
        ErrorType.Unauthorized);

    public static readonly Error InvalidRefreshToken = new(
        "AUTH.INVALID_REFRESH_TOKEN",
        "The refresh token is invalid or has expired.",
        ErrorType.Unauthorized);
}
