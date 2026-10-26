namespace OmniManage.Modules.Platform.Application.Auth;

public sealed record RefreshTokenValue(string Token, string Hash);

public interface IRefreshTokenGenerator
{
    RefreshTokenValue Generate();

    string Hash(string token);
}
