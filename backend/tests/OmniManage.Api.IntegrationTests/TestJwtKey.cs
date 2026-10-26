using System.Security.Cryptography;

namespace OmniManage.Api.IntegrationTests;

internal static class TestJwtKey
{
    public const string Issuer = "https://auth.integration-tests.omnimanage";
    public const string Audience = "omnimanage-tenant";

    // Microsoft.IdentityModel caches signature providers across the process. Each factory gets its
    // own key id and key pair so disposing one host's RSA key cannot break another host.
    public static string CreateKeyId() => $"integration-test-key-{Guid.NewGuid():N}";

    public static string CreatePrivateKeyPem()
    {
        using var rsa = RSA.Create(2048);

        return rsa.ExportPkcs8PrivateKeyPem();
    }
}
