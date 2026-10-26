namespace OmniManage.Modules.Platform.Application.Auth;

public sealed class RefreshTokenOptions
{
    public const string SectionName = "RefreshTokens";

    public int LifetimeDays { get; init; } = 7;
}
