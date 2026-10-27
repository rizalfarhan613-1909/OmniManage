using OmniManage.Modules.Hris.Domain.Attendance;

namespace OmniManage.Modules.Hris.Domain.Abstractions;

public sealed record AttendanceLogListCriteria(
    int Skip,
    int Take,
    Guid? EmployeeId,
    AttendanceStatus? Status,
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc,
    IReadOnlyList<AttendanceLogSortOption> Sort);

public sealed record AttendanceLogSortOption(AttendanceLogSortField Field, bool Descending);

public enum AttendanceLogSortField
{
    ClockInUtc = 1,
    EmployeeId = 2,
    Status = 3,
}
