using Microsoft.Extensions.Logging;
using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Application.IntegrationEvents;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Hris.Application.IntegrationEvents;
using OmniManage.Modules.Hris.Contracts.IntegrationEvents;
using OmniManage.Modules.Hris.Domain.Abstractions;
using OmniManage.Modules.Hris.Domain.Attendance;
using OmniManage.Modules.Hris.Domain.Errors;

namespace OmniManage.Modules.Hris.Application.Attendance.ClockIn;

public sealed class ClockInCommandHandler(
    IEmployeeRepository employees,
    IAttendanceLogRepository attendanceLogs,
    IHrisOutbox outbox,
    IIntegrationEventDispatcher dispatcher,
    ITenantContext tenantContext,
    IGuidGenerator guidGenerator,
    TimeProvider clock,
    ILogger<ClockInCommandHandler> logger)
    : ICommandHandler<ClockInCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        ClockInCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var employeeExists = await employees
            .ExistsAsync(command.EmployeeId, cancellationToken)
            .ConfigureAwait(false);

        if (!employeeExists)
        {
            return EmployeeErrors.NotFound;
        }

        var now = clock.GetUtcNow();

        var created = SmartAttendanceLog.Create(
            guidGenerator.NewGuid(),
            tenantContext.TenantId,
            command.EmployeeId,
            now,
            command.Latitude,
            command.Longitude,
            command.PhotoUrl);

        if (created.IsFailure)
        {
            return created.Error;
        }

        var attendanceLog = created.Value;

        var integrationEvent = new SmartAttendanceClockedInIntegrationEvent(
            guidGenerator.NewGuid(),
            tenantContext.TenantId,
            now.UtcDateTime,
            attendanceLog.Id,
            attendanceLog.EmployeeId,
            attendanceLog.ClockInUtc.UtcDateTime,
            NormalizeAttendanceMode(command.AttendanceMode),
            (double)attendanceLog.Latitude,
            (double)attendanceLog.Longitude,
            attendanceLog.PhotoUrl,
            string.IsNullOrWhiteSpace(command.WifiSsid) ? null : command.WifiSsid.Trim());

        // The attendance row and the outbox row are committed together.
        await attendanceLogs
            .AddAsync(attendanceLog, cancellationToken)
            .ConfigureAwait(false);
        await outbox
            .EnqueueAsync(integrationEvent, cancellationToken)
            .ConfigureAwait(false);
        await attendanceLogs
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        await IntegrationEventDispatch
            .DispatchAfterCommitAsync(outbox, dispatcher, integrationEvent, logger, cancellationToken)
            .ConfigureAwait(false);

        return attendanceLog.Id;
    }

    private static string NormalizeAttendanceMode(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? ClockInCommand.DefaultAttendanceMode
            : value.Trim();
}
