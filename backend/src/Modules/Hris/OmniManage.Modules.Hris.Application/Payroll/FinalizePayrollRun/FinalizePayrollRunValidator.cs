using FluentValidation;

namespace OmniManage.Modules.Hris.Application.Payroll.FinalizePayrollRun;

public sealed class FinalizePayrollRunValidator : AbstractValidator<FinalizePayrollRunCommand>
{
    public FinalizePayrollRunValidator()
    {
        RuleFor(command => command.PayrollRunId)
            .NotEmpty();
    }
}
