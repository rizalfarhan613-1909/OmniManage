using Microsoft.EntityFrameworkCore;
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

    public async Task<(IReadOnlyList<SmartAttendanceLog> Logs, long TotalItems)> ListAsync(
        AttendanceLogListCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        IQueryable<SmartAttendanceLog> query = dbContext.AttendanceLogs.AsNoTracking();

        if (criteria.EmployeeId is { } employeeId)
        {
            query = query.Where(log => log.EmployeeId == employeeId);
        }

        if (criteria.Status is { } status)
        {
            query = query.Where(log => log.Status == status);
        }

        if (criteria.FromUtc is { } fromUtc)
        {
            query = query.Where(log => log.ClockInUtc >= fromUtc);
        }

        if (criteria.ToUtc is { } toUtc)
        {
            query = query.Where(log => log.ClockInUtc <= toUtc);
        }

        var totalItems = await query.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var logs = await ApplySort(query, criteria)
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return (logs, totalItems);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);

    private static IQueryable<SmartAttendanceLog> ApplySort(
        IQueryable<SmartAttendanceLog> query,
        AttendanceLogListCriteria criteria)
    {
        var option = criteria.Sort.Count == 0
            ? new AttendanceLogSortOption(AttendanceLogSortField.ClockInUtc, true)
            : criteria.Sort[0];

        IOrderedQueryable<SmartAttendanceLog> ordered = option.Field switch
        {
            AttendanceLogSortField.EmployeeId => option.Descending
                ? query.OrderByDescending(log => log.EmployeeId)
                : query.OrderBy(log => log.EmployeeId),
            AttendanceLogSortField.Status => option.Descending
                ? query.OrderByDescending(log => log.Status)
                : query.OrderBy(log => log.Status),
            AttendanceLogSortField.ClockInUtc => option.Descending
                ? query.OrderByDescending(log => log.ClockInUtc)
                : query.OrderBy(log => log.ClockInUtc),
            _ => query.OrderByDescending(log => log.ClockInUtc),
        };

        return ordered.ThenBy(log => log.Id);
    }
}
