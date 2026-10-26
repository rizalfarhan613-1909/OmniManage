using OmniManage.BuildingBlocks.Application;

namespace OmniManage.Modules.Platform.Application.Auth;

public sealed record LogoutCommand(
    string? RefreshToken,
    string? IpAddress = null,
    string? UserAgent = null) : ICommand<LogoutResponse>;

public sealed record LogoutResponse(bool Revoked);
