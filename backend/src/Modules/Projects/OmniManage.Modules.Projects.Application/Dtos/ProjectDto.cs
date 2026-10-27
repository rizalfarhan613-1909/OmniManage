using OmniManage.Modules.Projects.Domain.Projects;

namespace OmniManage.Modules.Projects.Application.Dtos;

public sealed record ProjectDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    Guid CustomerId,
    DateOnly StartDate,
    DateOnly? EndDate,
    string Status)
{
    internal static ProjectDto From(Project project)
    {
        ArgumentNullException.ThrowIfNull(project);

        return new ProjectDto(
            project.Id,
            project.Code,
            project.Name,
            project.Description,
            project.CustomerId,
            project.StartDate,
            project.EndDate,
            project.Status.ToString());
    }
}
