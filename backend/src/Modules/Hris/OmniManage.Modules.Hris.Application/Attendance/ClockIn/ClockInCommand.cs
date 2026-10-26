using OmniManage.BuildingBlocks.Application;

namespace OmniManage.Modules.Hris.Application.Attendance.ClockIn;

public sealed record ClockInCommand(
    Guid EmployeeId,
    decimal Latitude,
    decimal Longitude,
    string? PhotoUrl,
    string? AttendanceMode,
    string? WifiSsid) : ICommand<Guid>
{
    public const string DefaultAttendanceMode = "GPS_PHOTO";
}
