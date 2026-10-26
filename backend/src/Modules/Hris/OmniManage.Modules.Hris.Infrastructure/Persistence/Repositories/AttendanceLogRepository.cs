using OmniManage.Modules.Hris.Domain.Abstractions;
using OmniManage.Modules.Hris.Domain.Attendance;
using OmniManage.Modules.Hris.Infrastructure.Persistence;

namespace OmniManage.Modules.Hris.Infrastructure.Persistence.Repositories;

public sealed class AttendanceLogRepository(HrisDbContext dbContext) : IAttendanceLogRepository
{
    public Task AddAsync(
        SmartAttendanceLog attendanceLog,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(attendanceLog);

        return dbContext.AttendanceLogs.AddAsync(attendanceLog, cancellationToken).AsTask();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
