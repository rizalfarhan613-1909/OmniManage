using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Hris.Domain.Errors;

namespace OmniManage.Modules.Hris.Domain.Attendance;

public sealed class SmartAttendanceLog : AggregateRoot<Guid>, ITenantEntity
{
    public const int PhotoUrlMaxLength = 1000;
    public const decimal MinLatitude = -90m;
    public const decimal MaxLatitude = 90m;
    public const decimal MinLongitude = -180m;
    public const decimal MaxLongitude = 180m;

    private SmartAttendanceLog()
    {
    }

    private SmartAttendanceLog(
        Guid id,
        Guid tenantId,
        Guid employeeId,
        DateTimeOffset clockInUtc,
        decimal latitude,
        decimal longitude,
        string? photoUrl)
        : base(id)
    {
        TenantId = tenantId;
        EmployeeId = employeeId;
        ClockInUtc = clockInUtc;
        Latitude = latitude;
        Longitude = longitude;
        PhotoUrl = photoUrl;
        Status = AttendanceStatus.ClockedIn;
    }

    public Guid TenantId { get; private set; }

    public Guid EmployeeId { get; private set; }

    public DateTimeOffset ClockInUtc { get; private set; }

    public DateTimeOffset? ClockOutUtc { get; private set; }

    public decimal Latitude { get; private set; }

    public decimal Longitude { get; private set; }

    public string? PhotoUrl { get; private set; }

    public AttendanceStatus Status { get; private set; }

    public static Result<SmartAttendanceLog> Create(
        Guid id,
        Guid tenantId,
        Guid employeeId,
        DateTimeOffset clockInUtc,
        decimal latitude,
        decimal longitude,
        string? photoUrl)
    {
        if (id == Guid.Empty)
        {
            return SmartAttendanceErrors.InvalidId;
        }

        if (tenantId == Guid.Empty)
        {
            return SmartAttendanceErrors.InvalidTenantId;
        }

        if (employeeId == Guid.Empty)
        {
            return SmartAttendanceErrors.InvalidEmployeeId;
        }

        if (clockInUtc == default)
        {
            return SmartAttendanceErrors.InvalidClockInUtc;
        }

        if (latitude is < MinLatitude or > MaxLatitude)
        {
            return SmartAttendanceErrors.InvalidLatitude;
        }

        if (longitude is < MinLongitude or > MaxLongitude)
        {
            return SmartAttendanceErrors.InvalidLongitude;
        }

        if (!TryNormalizePhotoUrl(photoUrl, out var normalizedPhotoUrl))
        {
            return SmartAttendanceErrors.InvalidPhotoUrl;
        }

        return new SmartAttendanceLog(
            id,
            tenantId,
            employeeId,
            clockInUtc,
            latitude,
            longitude,
            normalizedPhotoUrl);
    }

    public Result ClockOut(DateTimeOffset clockOutUtc)
    {
        if (Status != AttendanceStatus.ClockedIn)
        {
            return SmartAttendanceErrors.InvalidStatusTransition;
        }

        if (clockOutUtc == default || clockOutUtc <= ClockInUtc)
        {
            return SmartAttendanceErrors.InvalidClockOutUtc;
        }

        ClockOutUtc = clockOutUtc;
        Status = AttendanceStatus.ClockedOut;

        return Result.Success();
    }

    private static bool TryNormalizePhotoUrl(string? value, out string? normalized)
    {
        normalized = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var candidate = value.Trim();

        if (candidate.Length > PhotoUrlMaxLength)
        {
            return false;
        }

        normalized = candidate;
        return true;
    }
}
