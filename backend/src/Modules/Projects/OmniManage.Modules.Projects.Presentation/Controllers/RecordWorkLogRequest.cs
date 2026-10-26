namespace OmniManage.Modules.Projects.Presentation.Controllers;

public sealed record RecordWorkLogRequest(
    Guid EmployeeId,
    DateOnly LoggedDate,
    decimal HoursSpent,
    string? Description,
    bool CompleteTask);
