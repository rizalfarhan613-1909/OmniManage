using System.Security.Cryptography;

namespace OmniManage.Api.Authentication;

/// <summary>
/// Creates an ephemeral RSA signing key for local development when
/// <c>Jwt:PrivateKeyPem</c> is not configured. Committed or shared keys are
/// never generated: the key only lives in memory for the current process, so
/// access tokens issued before a restart stop validating.
/// </summary>
internal static class DevelopmentSigningKey
{
    private const int KeySizeInBits = 2048;

    public static string CreatePrivateKeyPem()
    {
        using var rsa = RSA.Create(KeySizeInBits);

        return rsa.ExportPkcs8PrivateKeyPem();
    }
}
