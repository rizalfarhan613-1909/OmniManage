namespace OmniManage.Modules.Platform.Application.Auth;

public sealed record AuthUserDto(Guid Id, string Email, IReadOnlyList<string> Roles);
