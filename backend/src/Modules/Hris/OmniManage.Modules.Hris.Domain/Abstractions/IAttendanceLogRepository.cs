using OmniManage.Modules.Hris.Domain.Attendance;

namespace OmniManage.Modules.Hris.Domain.Abstractions;

public interface IAttendanceLogRepository
{
    Task AddAsync(SmartAttendanceLog attendanceLog, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
