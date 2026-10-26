using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Contracts.Security;
using OmniManage.Modules.Platform.Contracts.Authorization;
using OmniManage.Modules.Projects.Application.Tasks.CreateProjectTask;
using OmniManage.Modules.Projects.Application.WorkLogs.RecordWorkLog;
using OmniManage.Modules.Projects.Application.Queries;

namespace OmniManage.Modules.Projects.Presentation.Controllers;

[ApiController]
[Route("api/v1/projects/tasks")]
[RequireModule(ModuleCodes.ProjectCore)]
public sealed class TasksController(ISender sender, TimeProvider clock, IMediator mediator) : ApiControllerBase(clock)
{
    private readonly IMediator _mediator = mediator;

    [HttpPost]
    [HasPermission(Permissions.Projects.TaskCreate)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProjectTaskRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await sender
            .Send(
                new CreateProjectTaskCommand(
                    request.ProjectId,
                    request.Title,
                    request.AssigneeEmployeeId,
                    request.EstimatedHours),
                cancellationToken)
            .ConfigureAwait(false);

        return FromCreatedResult(result, projectTaskId => $"/api/v1/projects/tasks/{projectTaskId:D}");
    }

    [HttpPost("{taskId:guid}/work-logs")]
    [HasPermission(Permissions.Projects.WorkLogCreate)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordWorkLog(
        Guid taskId,
        [FromBody] RecordWorkLogRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await sender
            .Send(
                new RecordWorkLogCommand(
                    taskId,
                    request.EmployeeId,
                    request.LoggedDate,
                    request.HoursSpent,
                    request.Description,
                    request.CompleteTask),
                cancellationToken)
            .ConfigureAwait(false);

        return FromCreatedResult(
            result,
            workLogId => $"/api/v1/projects/tasks/{taskId:D}/work-logs/{workLogId:D}");
    }

    [HttpGet]
    [RequireModule(ModuleCodes.ProjectCore)]
    [HasPermission(Permissions.Projects.TaskRead)]
    public async Task<IActionResult> GetTasks([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetProjectTasksQuery { PageNumber = pageNumber, PageSize = pageSize };
        var result = await _mediator.Send(query, CancellationToken.None);
        return Ok(result);
    }
}