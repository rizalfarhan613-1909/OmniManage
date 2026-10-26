using Microsoft.Extensions.Logging;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Application.IntegrationEvents;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Projects.Application.IntegrationEvents;
using OmniManage.Modules.Projects.Contracts.IntegrationEvents;
using OmniManage.Modules.Projects.Domain.Abstractions;
using OmniManage.Modules.Projects.Domain.Errors;
using OmniManage.Modules.Projects.Domain.Tasks;
using OmniManage.Modules.Projects.Domain.WorkLogs;

namespace OmniManage.Modules.Projects.Application.WorkLogs.RecordWorkLog;

public sealed class RecordWorkLogCommandHandler(
    IProjectTaskRepository projectTasks,
    IWorkLogRepository workLogs,
    IProjectsOutbox outbox,
    IIntegrationEventDispatcher dispatcher,
    ITenantContext tenantContext,
    IGuidGenerator guidGenerator,
    TimeProvider clock,
    ILogger<RecordWorkLogCommandHandler> logger)
    : ICommandHandler<RecordWorkLogCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        RecordWorkLogCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var projectTask = await projectTasks
            .GetForUpdateAsync(command.ProjectTaskId, cancellationToken)
            .ConfigureAwait(false);

        if (projectTask is null)
        {
            return ProjectTaskErrors.NotFound;
        }

        var progress = projectTask.RecordProgress(command.HoursSpent);

        if (progress.IsFailure)
        {
            return progress.Error;
        }

        if (command.CompleteTask)
        {
            var completion = projectTask.Complete();

            if (completion.IsFailure)
            {
                return completion.Error;
            }
        }

        var created = WorkLog.Create(
            guidGenerator.NewGuid(),
            tenantContext.TenantId,
            projectTask.Id,
            command.EmployeeId,
            command.LoggedDate,
            command.HoursSpent,
            command.Description);

        if (created.IsFailure)
        {
            return created.Error;
        }

        var workLog = created.Value;
        TaskCompletedIntegrationEvent? integrationEvent = null;

        if (projectTask.Status == ProjectTaskStatus.Done)
        {
            var now = clock.GetUtcNow();

            integrationEvent = new TaskCompletedIntegrationEvent(
                guidGenerator.NewGuid(),
                tenantContext.TenantId,
                now.UtcDateTime,
                projectTask.Id,
                projectTask.ProjectId,
                command.EmployeeId,
                projectTask.Title,
                now.UtcDateTime);
        }

        // The work log, the task progress and the outbox row are committed
        // together by the module's single SaveChanges call.
        await workLogs
            .AddAsync(workLog, cancellationToken)
            .ConfigureAwait(false);

        if (integrationEvent is not null)
        {
            await outbox
                .EnqueueAsync(integrationEvent, cancellationToken)
                .ConfigureAwait(false);
        }

        await projectTasks
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        if (integrationEvent is not null)
        {
            await IntegrationEventDispatch
                .DispatchAfterCommitAsync(outbox, dispatcher, integrationEvent, logger, cancellationToken)
                .ConfigureAwait(false);
        }

        return workLog.Id;
    }
}
