using Microsoft.EntityFrameworkCore;
using OmniManage.Modules.Projects.Domain.Abstractions;
using OmniManage.Modules.Projects.Domain.Tasks;
using OmniManage.Modules.Projects.Infrastructure.Persistence;

namespace OmniManage.Modules.Projects.Infrastructure.Persistence.Repositories;

public sealed class ProjectTaskRepository(ProjectsDbContext dbContext) : IProjectTaskRepository
{
    public Task AddAsync(ProjectTask task, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);

        return dbContext.Tasks.AddAsync(task, cancellationToken).AsTask();
    }

    public Task<ProjectTask?> GetForUpdateAsync(
        Guid taskId,
        CancellationToken cancellationToken = default) =>
        dbContext.Tasks.FirstOrDefaultAsync(task => task.Id == taskId, cancellationToken);

    public async Task<(IReadOnlyList<ProjectTask> Tasks, long TotalItems)> ListAsync(
        ProjectTaskListCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        IQueryable<ProjectTask> query = dbContext.Tasks.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            var term = criteria.Search.Trim().ToLowerInvariant();

            query = query.Where(task => task.Title.ToLower().Contains(term));
        }

        if (criteria.ProjectId is { } projectId)
        {
            query = query.Where(task => task.ProjectId == projectId);
        }

        if (criteria.AssigneeEmployeeId is { } assigneeEmployeeId)
        {
            query = query.Where(task => task.AssigneeEmployeeId == assigneeEmployeeId);
        }

        if (criteria.Status is { } status)
        {
            query = query.Where(task => task.Status == status);
        }

        var totalItems = await query.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var tasks = await ApplySort(query, criteria)
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return (tasks, totalItems);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);

    private static IQueryable<ProjectTask> ApplySort(
        IQueryable<ProjectTask> query,
        ProjectTaskListCriteria criteria)
    {
        var option = criteria.Sort.Count == 0
            ? new ProjectTaskSortOption(ProjectTaskSortField.Title, false)
            : criteria.Sort[0];

        IOrderedQueryable<ProjectTask> ordered = option.Field switch
        {
            ProjectTaskSortField.Status => option.Descending
                ? query.OrderByDescending(task => task.Status)
                : query.OrderBy(task => task.Status),
            ProjectTaskSortField.EstimatedHours => option.Descending
                ? query.OrderByDescending(task => task.EstimatedHours)
                : query.OrderBy(task => task.EstimatedHours),
            ProjectTaskSortField.Title => option.Descending
                ? query.OrderByDescending(task => task.Title)
                : query.OrderBy(task => task.Title),
            _ => query.OrderBy(task => task.Title),
        };

        return ordered.ThenBy(task => task.Id);
    }
}
