using OmniManage.Modules.Projects.Domain.Tasks;

namespace OmniManage.Modules.Projects.Domain.Abstractions;

public sealed record ProjectTaskListCriteria(
    int Skip,
    int Take,
    string? Search,
    Guid? ProjectId,
    Guid? AssigneeEmployeeId,
    ProjectTaskStatus? Status,
    IReadOnlyList<ProjectTaskSortOption> Sort);

public sealed record ProjectTaskSortOption(ProjectTaskSortField Field, bool Descending);

public enum ProjectTaskSortField
{
    Title = 1,
    Status = 2,
    EstimatedHours = 3,
}
