using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OmniManage.BuildingBlocks.Contracts;
using OmniManage.BuildingBlocks.Contracts.Security;
using OmniManage.Modules.Platform.Application.Auth;

namespace OmniManage.Modules.Platform.Presentation.Controllers;

[ApiController]
[Route("api/v1/auth")]
[AllowWithoutModule]
public sealed class AuthController(ISender sender, TimeProvider clock) : ApiControllerBase(clock)
{
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await sender
            .Send(
                new LoginCommand(
                    request.TenantCode,
                    request.Email,
                    request.Password,
                    GetDeviceId(),
                    GetIpAddress(),
                    GetUserAgent()),
                cancellationToken)
            .ConfigureAwait(false);

        return FromResult(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await sender
            .Send(
                new RefreshTokenCommand(
                    request.RefreshToken,
                    GetDeviceId(),
                    GetIpAddress(),
                    GetUserAgent()),
                cancellationToken)
            .ConfigureAwait(false);

        return FromResult(result);
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LogoutResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await sender
            .Send(
                new LogoutCommand(request.RefreshToken, GetIpAddress(), GetUserAgent()),
                cancellationToken)
            .ConfigureAwait(false);

        return FromResult(result);
    }

    [HttpGet("session")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<AuthSessionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSession(CancellationToken cancellationToken)
    {
        var result = await sender
            .Send(new GetSessionQuery(), cancellationToken)
            .ConfigureAwait(false);

        return FromResult(result);
    }

    private string? GetDeviceId()
    {
        var deviceId = Request.Headers["X-Device-Id"].ToString();

        return string.IsNullOrWhiteSpace(deviceId) ? null : deviceId;
    }

    private string? GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString();

    private string? GetUserAgent()
    {
        var userAgent = Request.Headers.UserAgent.ToString();

        return string.IsNullOrWhiteSpace(userAgent) ? null : userAgent;
    }
}
