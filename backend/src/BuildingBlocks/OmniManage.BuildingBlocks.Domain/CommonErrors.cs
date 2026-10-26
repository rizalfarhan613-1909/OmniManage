namespace OmniManage.BuildingBlocks.Domain;

/// <summary>
/// Cross-cutting error codes from Rules.md §5.5/§6. These are deliberately not
/// owned by a business module: they describe transport and infrastructure
/// outcomes that any endpoint can produce.
/// </summary>
public static class CommonErrors
{
    public static readonly Error ValidationFailed = new(
        "VALIDATION.FAILED",
        "One or more validation errors occurred.",
        ErrorType.Validation);

    public static readonly Error Unexpected = new(
        "SYSTEM.UNEXPECTED",
        "An unexpected error occurred. Contact support with the trace identifier.",
        ErrorType.Failure);

    public static readonly Error Unavailable = new(
        "SYSTEM.UNAVAILABLE",
        "The service is temporarily unavailable. Try again later.",
        ErrorType.Failure);

    public static readonly Error NotFound = new(
        "SYSTEM.NOT_FOUND",
        "The requested resource was not found.",
        ErrorType.NotFound);
}
