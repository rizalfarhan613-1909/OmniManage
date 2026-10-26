using System.Security.Cryptography;
using FluentAssertions;
using OmniManage.Modules.Platform.Infrastructure.Auth;

namespace OmniManage.Modules.Platform.UnitTests.Auth;

public sealed class JwtSigningKeyTests
{
    [Fact]
    public void IsValidPrivateKeyPem_WithRsaPrivateKey_ReturnsTrue()
    {
        using var rsa = RSA.Create(2048);
        var privateKeyPem = rsa.ExportPkcs8PrivateKeyPem();

        var isValid = JwtSigningKey.IsValidPrivateKeyPem(privateKeyPem);

        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValidPrivateKeyPem_WithEmptyValue_ReturnsFalse()
    {
        var isValid = JwtSigningKey.IsValidPrivateKeyPem("   ");

        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValidPrivateKeyPem_WithNonPemValue_ReturnsFalse()
    {
        var isValid = JwtSigningKey.IsValidPrivateKeyPem("not-a-private-key");

        isValid.Should().BeFalse();
    }
}
