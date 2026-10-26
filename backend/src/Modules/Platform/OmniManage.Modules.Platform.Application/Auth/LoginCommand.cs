using OmniManage.BuildingBlocks.Application;

namespace OmniManage.Modules.Platform.Application.Auth;

public sealed record LoginCommand(
    string? TenantCode,
    string? Email,
    string? Password,
    string? DeviceId = null,
    string? IpAddress = null,
    string? UserAgent = null) : ICommand<LoginResponse>;
