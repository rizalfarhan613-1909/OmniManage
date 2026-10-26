using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Projects.Domain.Abstractions;
using OmniManage.Modules.Projects.Domain.Errors;
using OmniManage.Modules.Projects.Domain.Tasks;

namespace OmniManage.Modules.Projects.Application.Tasks.CreateProjectTask;

public sealed class CreateProjectTaskCommandHandler(
    IProjectRepository projects,
    IProjectTaskRepository projectTasks,
    ITenantContext tenantContext,
    IGuidGenerator guidGenerator)
    : ICommandHandler<CreateProjectTaskCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreateProjectTaskCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var projectExists = await projects
            .ExistsAsync(command.ProjectId, cancellationToken)
            .ConfigureAwait(false);

        if (!projectExists)
        {
            return ProjectErrors.NotFound;
        }

        var created = ProjectTask.Create(
            guidGenerator.NewGuid(),
            tenantContext.TenantId,
            command.ProjectId,
            command.Title,
            command.AssigneeEmployeeId,
            command.EstimatedHours);

        if (created.IsFailure)
        {
            return created.Error;
        }

        var projectTask = created.Value;

        await projectTasks
            .AddAsync(projectTask, cancellationToken)
            .ConfigureAwait(false);
        await projectTasks
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return projectTask.Id;
    }
}
