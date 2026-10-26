using FluentValidation;
using OmniManage.Modules.Projects.Domain.WorkLogs;

namespace OmniManage.Modules.Projects.Application.WorkLogs.RecordWorkLog;

public sealed class RecordWorkLogValidator : AbstractValidator<RecordWorkLogCommand>
{
    public RecordWorkLogValidator()
    {
        RuleFor(command => command.ProjectTaskId)
            .NotEmpty();

        RuleFor(command => command.EmployeeId)
            .NotEmpty();

        RuleFor(command => command.LoggedDate)
            .NotEmpty();

        RuleFor(command => command.HoursSpent)
            .GreaterThan(0m)
            .LessThanOrEqualTo(WorkLog.MaxHoursPerLog);

        RuleFor(command => command.Description)
            .NotEmpty()
            .MaximumLength(WorkLog.DescriptionMaxLength);
    }
}
