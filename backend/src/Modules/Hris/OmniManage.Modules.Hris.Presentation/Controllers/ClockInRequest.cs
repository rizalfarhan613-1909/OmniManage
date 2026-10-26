namespace OmniManage.Modules.Hris.Presentation.Controllers;

public sealed record ClockInRequest(
    Guid EmployeeId,
    decimal Latitude,
    decimal Longitude,
    string? PhotoUrl,
    string? AttendanceMode,
    string? WifiSsid);
