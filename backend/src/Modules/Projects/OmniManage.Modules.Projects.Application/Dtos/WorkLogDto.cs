using System.Globalization;
using OmniManage.Modules.Projects.Domain.WorkLogs;

namespace OmniManage.Modules.Projects.Application.Dtos;

public sealed record WorkLogDto(
    Guid Id,
    Guid ProjectTaskId,
    Guid EmployeeId,
    DateOnly LoggedDate,
    string HoursSpent,
    string Description)
{
    internal static WorkLogDto From(WorkLog workLog)
    {
        ArgumentNullException.ThrowIfNull(workLog);

        return new WorkLogDto(
            workLog.Id,
            workLog.ProjectTaskId,
            workLog.EmployeeId,
            workLog.LoggedDate,
            workLog.HoursSpent.ToString("0.00", CultureInfo.InvariantCulture),
            workLog.Description);
    }
}
