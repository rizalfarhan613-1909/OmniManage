using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.Modules.Hris.Domain.Errors;

public static class SmartAttendanceErrors
{
    public static readonly Error InvalidId = new(
        "HRIS.ATTENDANCE.INVALID_ID",
        "An attendance log identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidTenantId = new(
        "HRIS.ATTENDANCE.INVALID_TENANT_ID",
        "A tenant identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidEmployeeId = new(
        "HRIS.ATTENDANCE.INVALID_EMPLOYEE_ID",
        "An employee identifier is required.",
        ErrorType.Validation);

    public static readonly Error InvalidClockInUtc = new(
        "HRIS.ATTENDANCE.INVALID_CLOCK_IN_UTC",
        "A valid clock-in time is required.",
        ErrorType.Validation);

    public static readonly Error InvalidClockOutUtc = new(
        "HRIS.ATTENDANCE.INVALID_CLOCK_OUT_UTC",
        "Clock-out time must be valid and later than the clock-in time.",
        ErrorType.Validation);

    public static readonly Error InvalidLatitude = new(
        "HRIS.ATTENDANCE.INVALID_LATITUDE",
        "Latitude must be between -90 and 90 degrees.",
        ErrorType.Validation);

    public static readonly Error InvalidLongitude = new(
        "HRIS.ATTENDANCE.INVALID_LONGITUDE",
        "Longitude must be between -180 and 180 degrees.",
        ErrorType.Validation);

    public static readonly Error InvalidPhotoUrl = new(
        "HRIS.ATTENDANCE.INVALID_PHOTO_URL",
        "Photo URL must not exceed 1000 characters.",
        ErrorType.Validation);

    public static readonly Error InvalidStatusTransition = new(
        "HRIS.ATTENDANCE.INVALID_STATUS_TRANSITION",
        "Only a clocked-in attendance log can be clocked out.",
        ErrorType.Conflict);
}
