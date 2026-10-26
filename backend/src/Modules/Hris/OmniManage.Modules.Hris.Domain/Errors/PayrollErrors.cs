using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.Modules.Hris.Domain.Errors;

public static class PayrollErrors
{
    public static readonly Error InvalidId = new(
        "HRIS.PAYROLL.INVALID_ID",
        "A payroll run identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidTenantId = new(
        "HRIS.PAYROLL.INVALID_TENANT_ID",
        "A tenant identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidPeriodCode = new(
        "HRIS.PAYROLL.INVALID_PERIOD_CODE",
        "Payroll period code must be 2 to 20 characters starting with a letter or digit and containing only letters, digits, hyphens, underscores or periods.",
        ErrorType.Validation);

    public static readonly Error InvalidTotalBasicSalary = new(
        "HRIS.PAYROLL.INVALID_TOTAL_BASIC_SALARY",
        "Total basic salary must not be negative or exceed the supported monetary range.",
        ErrorType.Validation);

    public static readonly Error InvalidTotalAllowances = new(
        "HRIS.PAYROLL.INVALID_TOTAL_ALLOWANCES",
        "Total allowances must not be negative or exceed the supported monetary range.",
        ErrorType.Validation);

    public static readonly Error InvalidTotalDeductions = new(
        "HRIS.PAYROLL.INVALID_TOTAL_DEDUCTIONS",
        "Total deductions must not be negative or exceed the supported monetary range.",
        ErrorType.Validation);

    public static readonly Error InvalidNetPayTotal = new(
        "HRIS.PAYROLL.INVALID_NET_PAY_TOTAL",
        "Net pay total must not be negative or exceed the supported monetary range.",
        ErrorType.Validation);

    public static readonly Error InvalidProcessedAtUtc = new(
        "HRIS.PAYROLL.INVALID_PROCESSED_AT_UTC",
        "A valid processing time is required to finalize a payroll run.",
        ErrorType.Validation);

    public static readonly Error InvalidStatusTransition = new(
        "HRIS.PAYROLL.INVALID_STATUS_TRANSITION",
        "The requested payroll status transition is not allowed from the current status.",
        ErrorType.Conflict);

    public static readonly Error NotFound = new(
        "HRIS.PAYROLL.NOT_FOUND",
        "The payroll run does not exist in the current tenant.",
        ErrorType.NotFound);
}
