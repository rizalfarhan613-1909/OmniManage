using OmniManage.BuildingBlocks.Application;

namespace OmniManage.Modules.Projects.Application.Tasks.CreateProjectTask;

public sealed record CreateProjectTaskCommand(
    Guid ProjectId,
    string? Title,
    Guid? AssigneeEmployeeId,
    decimal EstimatedHours) : ICommand<Guid>;
