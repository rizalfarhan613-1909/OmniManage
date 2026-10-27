using System.Globalization;
using OmniManage.Modules.Projects.Domain.Tasks;

namespace OmniManage.Modules.Projects.Application.Dtos;

public sealed record TaskDto(
    Guid Id,
    Guid ProjectId,
    string Title,
    Guid? AssigneeEmployeeId,
    string Status,
    string EstimatedHours,
    string CompletedHours)
{
    internal static TaskDto From(ProjectTask task)
    {
        ArgumentNullException.ThrowIfNull(task);

        return new TaskDto(
            task.Id,
            task.ProjectId,
            task.Title,
            task.AssigneeEmployeeId,
            task.Status.ToString(),
            FormatHours(task.EstimatedHours),
            FormatHours(task.CompletedHours));
    }

    private static string FormatHours(decimal hours) =>
        hours.ToString("0.00", CultureInfo.InvariantCulture);
}
