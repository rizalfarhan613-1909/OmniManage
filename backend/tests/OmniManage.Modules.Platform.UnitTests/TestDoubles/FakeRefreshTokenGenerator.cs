using System.Security.Cryptography;
using System.Text;
using OmniManage.Modules.Platform.Application.Auth;

namespace OmniManage.Modules.Platform.UnitTests.TestDoubles;

internal sealed class FakeRefreshTokenGenerator : IRefreshTokenGenerator
{
    private int _sequence;

    public int GenerateCallCount => _sequence;

    public RefreshTokenValue Generate()
    {
        var token = $"refresh-token-{++_sequence}";

        return new RefreshTokenValue(token, Hash(token));
    }

    public string Hash(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)))
            .ToLowerInvariant();
    }
}
