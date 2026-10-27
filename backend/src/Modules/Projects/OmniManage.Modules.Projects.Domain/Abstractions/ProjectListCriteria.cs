using OmniManage.Modules.Projects.Domain.Projects;

namespace OmniManage.Modules.Projects.Domain.Abstractions;

public sealed record ProjectListCriteria(
    int Skip,
    int Take,
    string? Search,
    Guid? CustomerId,
    ProjectStatus? Status,
    IReadOnlyList<ProjectSortOption> Sort);

public sealed record ProjectSortOption(ProjectSortField Field, bool Descending);

public enum ProjectSortField
{
    Code = 1,
    Name = 2,
    StartDate = 3,
}
