using OmniManage.Modules.Hris.Domain.Payroll;

namespace OmniManage.Modules.Hris.Domain.Abstractions;

public sealed record PayrollRunListCriteria(
    int Skip,
    int Take,
    string? Search,
    string? PeriodCode,
    PayrollStatus? Status,
    IReadOnlyList<PayrollRunSortOption> Sort);

public sealed record PayrollRunSortOption(PayrollRunSortField Field, bool Descending);

public enum PayrollRunSortField
{
    PeriodCode = 1,
    ProcessedAtUtc = 2,
    NetPayTotal = 3,
}
