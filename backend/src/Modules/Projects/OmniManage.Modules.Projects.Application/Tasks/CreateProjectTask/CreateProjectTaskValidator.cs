using FluentValidation;
using OmniManage.Modules.Projects.Domain.Tasks;

namespace OmniManage.Modules.Projects.Application.Tasks.CreateProjectTask;

public sealed class CreateProjectTaskValidator : AbstractValidator<CreateProjectTaskCommand>
{
    public CreateProjectTaskValidator()
    {
        RuleFor(command => command.ProjectId)
            .NotEmpty();

        RuleFor(command => command.Title)
            .NotEmpty()
            .MaximumLength(ProjectTask.TitleMaxLength);

        RuleFor(command => command.AssigneeEmployeeId)
            .Must(assigneeEmployeeId => assigneeEmployeeId != Guid.Empty)
            .When(command => command.AssigneeEmployeeId.HasValue)
            .WithMessage("The assignee must be a valid employee identifier.");

        RuleFor(command => command.EstimatedHours)
            .InclusiveBetween(0m, ProjectTask.MaxHours);
    }
}
