using OmniManage.Modules.Projects.Domain.Projects;

namespace OmniManage.Modules.Projects.Domain.Abstractions;

public interface IProjectRepository
{
    Task<bool> ExistsAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Project> Projects, long TotalItems)> ListAsync(
        ProjectListCriteria criteria,
        CancellationToken cancellationToken = default);
}
