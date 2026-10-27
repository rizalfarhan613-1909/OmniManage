using System.Globalization;
using OmniManage.Modules.Hris.Domain.Attendance;

namespace OmniManage.Modules.Hris.Application.Dtos;

public sealed record AttendanceLogDto(
    Guid Id,
    Guid EmployeeId,
    DateTimeOffset ClockInUtc,
    DateTimeOffset? ClockOutUtc,
    string Latitude,
    string Longitude,
    string? PhotoUrl,
    string Status)
{
    internal static AttendanceLogDto From(SmartAttendanceLog log)
    {
        ArgumentNullException.ThrowIfNull(log);

        return new AttendanceLogDto(
            log.Id,
            log.EmployeeId,
            log.ClockInUtc,
            log.ClockOutUtc,
            log.Latitude.ToString("0.000000", CultureInfo.InvariantCulture),
            log.Longitude.ToString("0.000000", CultureInfo.InvariantCulture),
            log.PhotoUrl,
            log.Status.ToString());
    }
}
