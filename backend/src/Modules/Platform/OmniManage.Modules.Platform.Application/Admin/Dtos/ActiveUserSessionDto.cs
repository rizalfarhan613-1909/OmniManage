using OmniManage.Modules.Platform.Domain.Abstractions;

namespace OmniManage.Modules.Platform.Application.Admin.Dtos;

public sealed record ActiveUserSessionDto(
    Guid Id,
    Guid UserId,
    string UserName,
    string Email,
    IReadOnlyList<string> Roles,
    string IpAddress,
    string? UserAgent,
    DateTimeOffset StartedAt,
    DateTimeOffset LastSeenAt,
    string Status)
{
    internal static ActiveUserSessionDto From(
        ActiveSessionProjection projection,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(projection);

        var status = now - projection.LastSeenAt <= TimeSpan.FromMinutes(30)
            ? "Active"
            : "Idle";

        return new ActiveUserSessionDto(
            projection.SessionId,
            projection.UserId,
            projection.Email,
            projection.Email,
            projection.Roles,
            projection.IpAddress,
            projection.UserAgent,
            projection.StartedAt,
            projection.LastSeenAt,
            status);
    }
}

public sealed record TerminateSessionResultDto(bool Terminated);
