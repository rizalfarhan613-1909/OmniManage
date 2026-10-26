using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Hris.Domain.Errors;

namespace OmniManage.Modules.Hris.Domain.Payroll;

public sealed class PayrollRun : AggregateRoot<Guid>, ITenantEntity
{
    public const int PeriodCodeMinLength = 2;
    public const int PeriodCodeMaxLength = 20;
    public const decimal MaxAmount = 999_999_999_999_999.9999m;

    private PayrollRun()
    {
    }

    private PayrollRun(
        Guid id,
        Guid tenantId,
        string periodCode,
        decimal totalBasicSalary,
        decimal totalAllowances,
        decimal totalDeductions,
        decimal netPayTotal)
        : base(id)
    {
        TenantId = tenantId;
        PeriodCode = periodCode;
        TotalBasicSalary = totalBasicSalary;
        TotalAllowances = totalAllowances;
        TotalDeductions = totalDeductions;
        NetPayTotal = netPayTotal;
        Status = PayrollStatus.Draft;
    }

    public Guid TenantId { get; private set; }

    public string PeriodCode { get; private set; } = string.Empty;

    public DateTimeOffset? ProcessedAtUtc { get; private set; }

    public decimal TotalBasicSalary { get; private set; }

    public decimal TotalAllowances { get; private set; }

    public decimal TotalDeductions { get; private set; }

    public decimal NetPayTotal { get; private set; }

    public PayrollStatus Status { get; private set; }

    public static Result<PayrollRun> Create(
        Guid id,
        Guid tenantId,
        string? periodCode,
        decimal totalBasicSalary,
        decimal totalAllowances,
        decimal totalDeductions)
    {
        if (id == Guid.Empty)
        {
            return PayrollErrors.InvalidId;
        }

        if (tenantId == Guid.Empty)
        {
            return PayrollErrors.InvalidTenantId;
        }

        if (!TryNormalizePeriodCode(periodCode, out var normalizedPeriodCode))
        {
            return PayrollErrors.InvalidPeriodCode;
        }

        if (totalBasicSalary is < 0m or > MaxAmount)
        {
            return PayrollErrors.InvalidTotalBasicSalary;
        }

        if (totalAllowances is < 0m or > MaxAmount)
        {
            return PayrollErrors.InvalidTotalAllowances;
        }

        if (totalDeductions is < 0m or > MaxAmount)
        {
            return PayrollErrors.InvalidTotalDeductions;
        }

        decimal netPayTotal;

        try
        {
            netPayTotal = decimal.Round(
                totalBasicSalary + totalAllowances - totalDeductions,
                4,
                MidpointRounding.AwayFromZero);
        }
        catch (OverflowException)
        {
            return PayrollErrors.InvalidNetPayTotal;
        }

        if (netPayTotal is < 0m or > MaxAmount)
        {
            return PayrollErrors.InvalidNetPayTotal;
        }

        return new PayrollRun(
            id,
            tenantId,
            normalizedPeriodCode,
            totalBasicSalary,
            totalAllowances,
            totalDeductions,
            netPayTotal);
    }

    public Result Finalize(DateTimeOffset processedAtUtc)
    {
        if (Status != PayrollStatus.Draft)
        {
            return PayrollErrors.InvalidStatusTransition;
        }

        if (processedAtUtc == default)
        {
            return PayrollErrors.InvalidProcessedAtUtc;
        }

        ProcessedAtUtc = processedAtUtc;
        Status = PayrollStatus.Finalized;

        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status != PayrollStatus.Draft)
        {
            return PayrollErrors.InvalidStatusTransition;
        }

        Status = PayrollStatus.Cancelled;

        return Result.Success();
    }

    private static bool TryNormalizePeriodCode(string? value, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim().ToUpperInvariant();

        if (candidate.Length is < PeriodCodeMinLength or > PeriodCodeMaxLength)
        {
            return false;
        }

        if (!char.IsAsciiLetterOrDigit(candidate[0]))
        {
            return false;
        }

        foreach (var character in candidate)
        {
            if (!char.IsAsciiLetterOrDigit(character) && character is not ('-' or '_' or '.'))
            {
                return false;
            }
        }

        normalized = candidate;
        return true;
    }
}
