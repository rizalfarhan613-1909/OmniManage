using OmniManage.BuildingBlocks.Contracts;

namespace OmniManage.Modules.Hris.Contracts.IntegrationEvents;

/// <summary>
/// Raised when a payroll run is finalized. Finance consumes this event to
/// auto-post the payroll journal; notifications and ESS consume it to expose
/// payslips. Immutable; new fields are additive within schema version 1.
/// </summary>
public sealed record PayrollFinalizedIntegrationEvent(
    Guid EventId,
    Guid TenantId,
    DateTime OccurredOnUtc,
    Guid PayrollRunId,
    string PeriodCode,
    decimal TotalBasicSalary,
    decimal TotalAllowances,
    decimal TotalDeductions,
    decimal TotalKasbonDeducted,
    decimal TotalTaxPPh21,
    decimal NetPayTotal) : IIntegrationEvent
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion => CurrentSchemaVersion;

    public string? CorrelationId => null;
}
