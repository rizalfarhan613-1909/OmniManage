using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OmniManage.Modules.Platform.Application.Auth;

namespace OmniManage.Modules.Platform.Infrastructure.Auth;

public sealed class JwtSigningKey : IDisposable
{
    private readonly RSA _rsa;

    public JwtSigningKey(IOptions<JwtOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var jwtOptions = options.Value;
        _rsa = RSA.Create();

        try
        {
            _rsa.ImportFromPem(jwtOptions.PrivateKeyPem);
        }
        catch (Exception exception) when (exception is ArgumentException or CryptographicException)
        {
            _rsa.Dispose();
            throw new InvalidOperationException(
                "Jwt:PrivateKeyPem must contain a valid PEM-encoded RSA private key.",
                exception);
        }

        Key = new RsaSecurityKey(_rsa) { KeyId = jwtOptions.KeyId };
        Credentials = new SigningCredentials(Key, SecurityAlgorithms.RsaSha256);
    }

    public static bool IsValidPrivateKeyPem(string privateKeyPem)
    {
        if (string.IsNullOrWhiteSpace(privateKeyPem))
        {
            return false;
        }

        using var rsa = RSA.Create();

        try
        {
            rsa.ImportFromPem(privateKeyPem);
            return true;
        }
        catch (Exception exception) when (exception is ArgumentException or CryptographicException)
        {
            return false;
        }
    }

    public RsaSecurityKey Key { get; }

    public SigningCredentials Credentials { get; }

    public void Dispose() => _rsa.Dispose();
}
