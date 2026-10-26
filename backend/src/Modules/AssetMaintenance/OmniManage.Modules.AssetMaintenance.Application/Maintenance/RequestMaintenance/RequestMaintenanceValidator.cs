using FluentValidation;
using OmniManage.Modules.AssetMaintenance.Domain.Maintenance;

namespace OmniManage.Modules.AssetMaintenance.Application.Maintenance.RequestMaintenance;

public sealed class RequestMaintenanceValidator : AbstractValidator<RequestMaintenanceCommand>
{
    private const string RequestNumberPattern = "^[A-Za-z0-9][A-Za-z0-9._-]*$";

    public RequestMaintenanceValidator()
    {
        RuleFor(command => command.AssetId)
            .NotEmpty();

        RuleFor(command => command.RequestNumber)
            .NotEmpty()
            .Length(MaintenanceOrder.RequestNumberMinLength, MaintenanceOrder.RequestNumberMaxLength)
            .Matches(RequestNumberPattern)
            .WithMessage(
                "Maintenance request number must be 2 to 50 characters starting with a letter or digit and containing only letters, digits, hyphens, underscores or periods.");

        RuleFor(command => command.ScheduledDate)
            .Must(scheduledDate => scheduledDate != default)
            .When(command => command.ScheduledDate.HasValue)
            .WithMessage("The scheduled date must be a valid date when supplied.");

        RuleFor(command => command.Description)
            .NotEmpty()
            .MaximumLength(MaintenanceOrder.DescriptionMaxLength);

        RuleFor(command => command.MaintenanceType)
            .MaximumLength(50)
            .When(command => !string.IsNullOrWhiteSpace(command.MaintenanceType));

        RuleFor(command => command.Priority)
            .MaximumLength(20)
            .When(command => !string.IsNullOrWhiteSpace(command.Priority));
    }
}
