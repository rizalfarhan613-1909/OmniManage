using FluentValidation;
using OmniManage.Modules.Hris.Domain.Attendance;

namespace OmniManage.Modules.Hris.Application.Attendance.ClockIn;

public sealed class ClockInValidator : AbstractValidator<ClockInCommand>
{
    public ClockInValidator()
    {
        RuleFor(command => command.EmployeeId)
            .NotEmpty();

        RuleFor(command => command.Latitude)
            .InclusiveBetween(SmartAttendanceLog.MinLatitude, SmartAttendanceLog.MaxLatitude);

        RuleFor(command => command.Longitude)
            .InclusiveBetween(SmartAttendanceLog.MinLongitude, SmartAttendanceLog.MaxLongitude);

        RuleFor(command => command.PhotoUrl)
            .MaximumLength(SmartAttendanceLog.PhotoUrlMaxLength)
            .When(command => !string.IsNullOrWhiteSpace(command.PhotoUrl));

        RuleFor(command => command.AttendanceMode)
            .MaximumLength(50)
            .When(command => !string.IsNullOrWhiteSpace(command.AttendanceMode));

        RuleFor(command => command.WifiSsid)
            .MaximumLength(100)
            .When(command => !string.IsNullOrWhiteSpace(command.WifiSsid));
    }
}
