using Microsoft.EntityFrameworkCore;
using OmniManage.Modules.Projects.Domain.Abstractions;
using OmniManage.Modules.Projects.Domain.WorkLogs;
using OmniManage.Modules.Projects.Infrastructure.Persistence;

namespace OmniManage.Modules.Projects.Infrastructure.Persistence.Repositories;

public sealed class WorkLogRepository(ProjectsDbContext dbContext) : IWorkLogRepository
{
    public Task AddAsync(WorkLog workLog, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workLog);

        return dbContext.WorkLogs.AddAsync(workLog, cancellationToken).AsTask();
    }

    public async Task<(IReadOnlyList<WorkLog> WorkLogs, long TotalItems)> ListAsync(
        WorkLogListCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        IQueryable<WorkLog> query = dbContext.WorkLogs.AsNoTracking();

        if (criteria.TaskId is { } taskId)
        {
            query = query.Where(workLog => workLog.ProjectTaskId == taskId);
        }

        if (criteria.EmployeeId is { } employeeId)
        {
            query = query.Where(workLog => workLog.EmployeeId == employeeId);
        }

        if (criteria.FromDate is { } fromDate)
        {
            query = query.Where(workLog => workLog.LoggedDate >= fromDate);
        }

        if (criteria.ToDate is { } toDate)
        {
            query = query.Where(workLog => workLog.LoggedDate <= toDate);
        }

        var totalItems = await query.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var workLogs = await ApplySort(query, criteria)
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return (workLogs, totalItems);
    }

    private static IQueryable<WorkLog> ApplySort(
        IQueryable<WorkLog> query,
        WorkLogListCriteria criteria)
    {
        var option = criteria.Sort.Count == 0
            ? new WorkLogSortOption(WorkLogSortField.LoggedDate, true)
            : criteria.Sort[0];

        IOrderedQueryable<WorkLog> ordered = option.Field switch
        {
            WorkLogSortField.HoursSpent => option.Descending
                ? query.OrderByDescending(workLog => workLog.HoursSpent)
                : query.OrderBy(workLog => workLog.HoursSpent),
            WorkLogSortField.LoggedDate => option.Descending
                ? query.OrderByDescending(workLog => workLog.LoggedDate)
                : query.OrderBy(workLog => workLog.LoggedDate),
            _ => query.OrderByDescending(workLog => workLog.LoggedDate),
        };

        return ordered.ThenBy(workLog => workLog.Id);
    }
}
