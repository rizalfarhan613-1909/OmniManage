using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Contracts.Security;
using OmniManage.Modules.Platform.Contracts.Authorization;
using OmniManage.Modules.Hris.Application.Attendance.ClockIn;
using OmniManage.Modules.Hris.Application.Dtos;
using OmniManage.Modules.Hris.Application.Queries;

namespace OmniManage.Modules.Hris.Presentation.Controllers;

[ApiController]
[Route("api/v1/hris")]
public sealed class AttendanceController(ISender sender, TimeProvider clock) : ApiControllerBase(clock)
{
    [HttpPost("attendance/clock-in")]
    [RequireModule(ModuleCodes.HrisAttendance)]
    [HasPermission(Permissions.Hris.AttendanceClockIn)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ClockIn(
        [FromBody] ClockInRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await sender
            .Send(
                new ClockInCommand(
                    request.EmployeeId,
                    request.Latitude,
                    request.Longitude,
                    request.PhotoUrl,
                    request.AttendanceMode,
                    request.WifiSsid),
                cancellationToken)
            .ConfigureAwait(false);

        return FromCreatedResult(
            result,
            attendanceLogId => $"/api/v1/hris/attendance/{attendanceLogId:D}");
    }

    [HttpGet("attendance")]
    [RequireModule(ModuleCodes.HrisAttendance)]
    [HasPermission(Permissions.Hris.AttendanceRead)]
    public async Task<IActionResult> GetAttendanceLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTimeOffset? fromUtc = null,
        [FromQuery] DateTimeOffset? toUtc = null,
        [FromQuery] string? sort = null)
    {
        var query = new GetAttendanceLogsQuery
        {
            PageNumber = page,
            PageSize = pageSize,
            EmployeeId = employeeId,
            Status = status,
            FromUtc = fromUtc,
            ToUtc = toUtc,
            Sort = sort,
        };

        var result = await sender.Send(query, CancellationToken.None);
        return FromPagedResult(result);
    }
}
