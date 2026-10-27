using Microsoft.EntityFrameworkCore;
using OmniManage.Modules.Projects.Domain.Abstractions;
using OmniManage.Modules.Projects.Domain.Projects;
using OmniManage.Modules.Projects.Infrastructure.Persistence;

namespace OmniManage.Modules.Projects.Infrastructure.Persistence.Repositories;

public sealed class ProjectRepository(ProjectsDbContext dbContext) : IProjectRepository
{
    public Task<bool> ExistsAsync(Guid projectId, CancellationToken cancellationToken = default) =>
        dbContext.Projects.AnyAsync(project => project.Id == projectId, cancellationToken);

    public async Task<(IReadOnlyList<Project> Projects, long TotalItems)> ListAsync(
        ProjectListCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        IQueryable<Project> query = dbContext.Projects.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            var term = criteria.Search.Trim().ToLowerInvariant();

            query = query.Where(project =>
                project.Code.ToLower().Contains(term)
                || project.Name.ToLower().Contains(term));
        }

        if (criteria.CustomerId is { } customerId)
        {
            query = query.Where(project => project.CustomerId == customerId);
        }

        if (criteria.Status is { } status)
        {
            query = query.Where(project => project.Status == status);
        }

        var totalItems = await query.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var projects = await ApplySort(query, criteria)
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return (projects, totalItems);
    }

    private static IQueryable<Project> ApplySort(
        IQueryable<Project> query,
        ProjectListCriteria criteria)
    {
        var option = criteria.Sort.Count == 0
            ? new ProjectSortOption(ProjectSortField.Code, false)
            : criteria.Sort[0];

        IOrderedQueryable<Project> ordered = option.Field switch
        {
            ProjectSortField.Name => option.Descending
                ? query.OrderByDescending(project => project.Name)
                : query.OrderBy(project => project.Name),
            ProjectSortField.StartDate => option.Descending
                ? query.OrderByDescending(project => project.StartDate)
                : query.OrderBy(project => project.StartDate),
            ProjectSortField.Code => option.Descending
                ? query.OrderByDescending(project => project.Code)
                : query.OrderBy(project => project.Code),
            _ => query.OrderBy(project => project.Code),
        };

        return ordered.ThenBy(project => project.Id);
    }
}
