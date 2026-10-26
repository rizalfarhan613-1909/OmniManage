using OmniManage.Modules.Hris.Domain.Payroll;

namespace OmniManage.Modules.Hris.Domain.Abstractions;

public interface IPayrollRunRepository
{
    Task<PayrollRun?> GetForUpdateAsync(Guid payrollRunId, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
