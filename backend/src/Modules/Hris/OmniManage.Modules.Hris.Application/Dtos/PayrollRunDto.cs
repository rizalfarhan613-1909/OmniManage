using System.Globalization;
using OmniManage.Modules.Hris.Domain.Payroll;

namespace OmniManage.Modules.Hris.Application.Dtos;

public sealed record PayrollRunDto(
    Guid Id,
    string PeriodCode,
    DateTimeOffset? ProcessedAtUtc,
    string TotalBasicSalary,
    string TotalAllowances,
    string TotalDeductions,
    string NetPayTotal,
    string Status)
{
    internal static PayrollRunDto From(PayrollRun payrollRun)
    {
        ArgumentNullException.ThrowIfNull(payrollRun);

        return new PayrollRunDto(
            payrollRun.Id,
            payrollRun.PeriodCode,
            payrollRun.ProcessedAtUtc,
            FormatAmount(payrollRun.TotalBasicSalary),
            FormatAmount(payrollRun.TotalAllowances),
            FormatAmount(payrollRun.TotalDeductions),
            FormatAmount(payrollRun.NetPayTotal),
            payrollRun.Status.ToString());
    }

    private static string FormatAmount(decimal amount) =>
        amount.ToString("0.0000", CultureInfo.InvariantCulture);
}
