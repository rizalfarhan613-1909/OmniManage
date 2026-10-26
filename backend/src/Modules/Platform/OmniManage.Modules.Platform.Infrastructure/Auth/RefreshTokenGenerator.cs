using System.Security.Cryptography;
using System.Text;
using OmniManage.Modules.Platform.Application.Auth;

namespace OmniManage.Modules.Platform.Infrastructure.Auth;

public sealed class RefreshTokenGenerator : IRefreshTokenGenerator
{
    private const int TokenByteLength = 32;

    public RefreshTokenValue Generate()
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(TokenByteLength))
            .ToLowerInvariant();

        return new RefreshTokenValue(token, Hash(token));
    }

    public string Hash(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));

        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
