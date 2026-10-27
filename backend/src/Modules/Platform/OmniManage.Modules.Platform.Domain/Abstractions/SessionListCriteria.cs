using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.Modules.Platform.Domain.Abstractions;

public sealed record ActiveSessionProjection(
    Guid SessionId,
    Guid UserId,
    string Email,
    IReadOnlyList<string> Roles,
    string IpAddress,
    string? UserAgent,
    DateTimeOffset StartedAt,
    DateTimeOffset LastSeenAt);

public sealed record SessionListCriteria(int Skip, int Take, string? Search);

public static class SessionErrors
{
    public static readonly Error InvalidSessionId = new(
        "PLATFORM.SESSION.INVALID_ID",
        "A session identifier is required.",
        ErrorType.Validation);

    public static readonly Error NotFound = new(
        "PLATFORM.SESSION.NOT_FOUND",
        "The session was not found or has already ended.",
        ErrorType.NotFound);
}
