namespace OmniManage.Modules.Projects.Presentation.Controllers;

public sealed record CreateProjectTaskRequest(
    Guid ProjectId,
    string? Title,
    Guid? AssigneeEmployeeId,
    decimal EstimatedHours);
