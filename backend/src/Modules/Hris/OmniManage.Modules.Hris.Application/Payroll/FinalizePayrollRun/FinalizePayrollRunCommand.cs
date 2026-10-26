using OmniManage.BuildingBlocks.Application;

namespace OmniManage.Modules.Hris.Application.Payroll.FinalizePayrollRun;

public sealed record FinalizePayrollRunCommand(Guid PayrollRunId) : ICommand;
