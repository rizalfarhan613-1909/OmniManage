using OmniManage.BuildingBlocks.Application;

namespace OmniManage.Modules.Platform.Application.Auth;

public sealed record RefreshTokenCommand(
    string? RefreshToken,
    string? DeviceId = null,
    string? IpAddress = null,
    string? UserAgent = null) : ICommand<LoginResponse>;
