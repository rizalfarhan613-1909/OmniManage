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

    public async Task<(IReadOnlyList<PayrollRun> PayrollRuns, long TotalItems)> ListAsync(
        PayrollRunListCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        IQueryable<PayrollRun> query = dbContext.PayrollRuns.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            var term = criteria.Search.Trim().ToLowerInvariant();

            query = query.Where(payrollRun => payrollRun.PeriodCode.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(criteria.PeriodCode))
        {
            var periodCode = criteria.PeriodCode.Trim().ToUpperInvariant();

            query = query.Where(payrollRun => payrollRun.PeriodCode == periodCode);
        }

        if (criteria.Status is { } status)
        {
            query = query.Where(payrollRun => payrollRun.Status == status);
        }

        var totalItems = await query.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var payrollRuns = await ApplySort(query, criteria)
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return (payrollRuns, totalItems);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);

    private static IQueryable<PayrollRun> ApplySort(
        IQueryable<PayrollRun> query,
        PayrollRunListCriteria criteria)
    {
        var option = criteria.Sort.Count == 0
            ? new PayrollRunSortOption(PayrollRunSortField.PeriodCode, true)
            : criteria.Sort[0];

        IOrderedQueryable<PayrollRun> ordered = option.Field switch
        {
            PayrollRunSortField.ProcessedAtUtc => option.Descending
                ? query.OrderByDescending(payrollRun => payrollRun.ProcessedAtUtc)
                : query.OrderBy(payrollRun => payrollRun.ProcessedAtUtc),
            PayrollRunSortField.NetPayTotal => option.Descending
                ? query.OrderByDescending(payrollRun => payrollRun.NetPayTotal)
                : query.OrderBy(payrollRun => payrollRun.NetPayTotal),
            PayrollRunSortField.PeriodCode => option.Descending
                ? query.OrderByDescending(payrollRun => payrollRun.PeriodCode)
                : query.OrderBy(payrollRun => payrollRun.PeriodCode),
            _ => query.OrderByDescending(payrollRun => payrollRun.PeriodCode),
        };

        return ordered.ThenBy(payrollRun => payrollRun.Id);
    }
}
