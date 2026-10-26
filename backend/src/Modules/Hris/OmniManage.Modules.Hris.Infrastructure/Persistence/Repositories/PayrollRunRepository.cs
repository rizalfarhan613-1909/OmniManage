using Microsoft.EntityFrameworkCore;
using OmniManage.Modules.Hris.Domain.Abstractions;
using OmniManage.Modules.Hris.Domain.Payroll;
using OmniManage.Modules.Hris.Infrastructure.Persistence;

namespace OmniManage.Modules.Hris.Infrastructure.Persistence.Repositories;

public sealed class PayrollRunRepository(HrisDbContext dbContext) : IPayrollRunRepository
{
    public Task<PayrollRun?> GetForUpdateAsync(
        Guid payrollRunId,
        CancellationToken cancellationToken = default) =>
        dbContext.PayrollRuns.FirstOrDefaultAsync(
            payrollRun => payrollRun.Id == payrollRunId,
            cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
