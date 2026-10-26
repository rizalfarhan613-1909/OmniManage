using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.Modules.Platform.Domain.Errors;

public static class AuthErrors
{
    public static readonly Error InvalidCredentials = new(
        "AUTH.INVALID_CREDENTIALS",
        "Invalid credentials.",
        ErrorType.Unauthorized);

    public static readonly Error Unauthenticated = new(
        "AUTH.UNAUTHENTICATED",
        "Authentication is required.",
        ErrorType.Unauthorized);

    public static readonly Error TokenExpired = new(
        "AUTH.TOKEN_EXPIRED",
        "The access token has expired.",
        ErrorType.Unauthorized);

    public static readonly Error Forbidden = new(
        "AUTH.FORBIDDEN",
        "You do not have permission to perform this action.",
        ErrorType.Forbidden);
}
