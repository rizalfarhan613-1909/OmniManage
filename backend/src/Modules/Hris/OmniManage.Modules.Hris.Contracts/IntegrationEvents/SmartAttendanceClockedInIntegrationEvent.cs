using OmniManage.BuildingBlocks.Contracts;

namespace OmniManage.Modules.Hris.Contracts.IntegrationEvents;

/// <summary>
/// Raised when an employee clocks in through Smart Attendance. Carries the
/// captured geo-location, photo proof reference and network context so
/// consumers do not need to call back into HRIS. Immutable; new fields are
/// additive within schema version 1.
/// </summary>
public sealed record SmartAttendanceClockedInIntegrationEvent(
    Guid EventId,
    Guid TenantId,
    DateTime OccurredOnUtc,
    Guid AttendanceRecordId,
    Guid EmployeeId,
    DateTime ClockedInAtUtc,
    string AttendanceMode,
    double? Latitude,
    double? Longitude,
    string? PhotoFileKey,
    string? WifiSsid) : IIntegrationEvent
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion => CurrentSchemaVersion;

    public string? CorrelationId => null;
}
