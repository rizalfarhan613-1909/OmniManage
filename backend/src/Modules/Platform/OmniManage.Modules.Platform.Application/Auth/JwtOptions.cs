namespace OmniManage.Modules.Platform.Application.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "https://auth.omnimanage.local";

    public string Audience { get; init; } = "omnimanage-tenant";

    public string KeyId { get; init; } = "tenant-signing-key-1";

    public string PrivateKeyPem { get; init; } = string.Empty;

    public int AccessTokenMinutes { get; init; } = 15;
}
