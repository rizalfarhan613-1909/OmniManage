namespace OmniManage.Modules.Platform.Application.Auth;

public sealed record AccessToken(string Token, DateTimeOffset ExpiresAt);
