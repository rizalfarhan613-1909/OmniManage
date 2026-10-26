using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using OmniManage.Modules.Platform.Application.Auth;

namespace OmniManage.Modules.Platform.Infrastructure.Auth;

public sealed class JwtTokenGenerator(
    JwtSigningKey signingKey,
    IOptions<JwtOptions> options,
    TimeProvider clock) : IJwtTokenGenerator
{
    private static readonly JsonWebTokenHandler TokenHandler = new();

    public AccessToken Generate(
        Guid userId,
        Guid tenantId,
        IReadOnlyCollection<string> roles,
        Guid sessionId)
    {
        ArgumentNullException.ThrowIfNull(roles);

        var jwtOptions = options.Value;
        var issuedAt = clock.GetUtcNow();
        var expiresAt = issuedAt.AddMinutes(jwtOptions.AccessTokenMinutes);

        var claims = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["sub"] = userId.ToString(),
            ["tenant_id"] = tenantId.ToString(),
            ["sid"] = sessionId.ToString(),
            ["jti"] = Guid.NewGuid().ToString(),
            ["roles"] = roles.ToArray(),
            ["amr"] = new[] { "pwd" },
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = jwtOptions.Issuer,
            Audience = jwtOptions.Audience,
            IssuedAt = issuedAt.UtcDateTime,
            NotBefore = issuedAt.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = signingKey.Credentials,
            Claims = claims,
        };

        return new AccessToken(TokenHandler.CreateToken(descriptor), expiresAt);
    }
}
