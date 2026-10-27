using OmniManage.Modules.Projects.Domain.Tasks;

namespace OmniManage.Modules.Projects.Domain.Abstractions;

public interface IProjectTaskRepository
{
    Task AddAsync(ProjectTask task, CancellationToken cancellationToken = default);

    Task<ProjectTask?> GetForUpdateAsync(Guid taskId, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<ProjectTask> Tasks, long TotalItems)> ListAsync(
        ProjectTaskListCriteria criteria,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
