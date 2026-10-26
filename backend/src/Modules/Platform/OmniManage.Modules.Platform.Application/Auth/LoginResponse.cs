namespace OmniManage.Modules.Platform.Application.Auth;

public sealed record LoginResponse(
    string AccessToken,
    string TokenType,
    DateTimeOffset ExpiresAt,
    AuthSessionDto Session,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);
