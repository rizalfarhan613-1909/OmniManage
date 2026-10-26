using OmniManage.BuildingBlocks.Application;

namespace OmniManage.Modules.Projects.Application.WorkLogs.RecordWorkLog;

public sealed record RecordWorkLogCommand(
    Guid ProjectTaskId,
    Guid EmployeeId,
    DateOnly LoggedDate,
    decimal HoursSpent,
    string? Description,
    bool CompleteTask) : ICommand<Guid>;
