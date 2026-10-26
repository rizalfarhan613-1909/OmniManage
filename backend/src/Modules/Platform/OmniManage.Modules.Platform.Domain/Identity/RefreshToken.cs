using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Platform.Domain.Errors;

namespace OmniManage.Modules.Platform.Domain.Identity;

public sealed class RefreshToken : AggregateRoot<Guid>, ITenantEntity
{
    public const int TokenHashLength = 64;
    public const int DeviceIdMaxLength = 200;
    public const int IpAddressMaxLength = 45;
    public const int UserAgentMaxLength = 500;
    public const int RevokedReasonMaxLength = 100;

    private RefreshToken()
    {
    }

    private RefreshToken(
        Guid id,
        Guid tenantId,
        Guid userId,
        Guid sessionId,
        string tokenHash,
        string? deviceId,
        string? ipAddress,
        string? userAgent,
        DateTimeOffset issuedAt,
        DateTimeOffset expiresAt)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        SessionId = sessionId;
        TokenHash = tokenHash;
        DeviceId = deviceId;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        IssuedAt = issuedAt;
        ExpiresAt = expiresAt;
    }

    public Guid TenantId { get; private set; }

    public Guid UserId { get; private set; }

    public Guid SessionId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public string? DeviceId { get; private set; }

    public string? IpAddress { get; private set; }

    public string? UserAgent { get; private set; }

    public DateTimeOffset IssuedAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    public string? RevokedReason { get; private set; }

    public Guid? ReplacedBy { get; private set; }

    public static Result<RefreshToken> Issue(
        Guid id,
        Guid tenantId,
        Guid userId,
        Guid sessionId,
        string? tokenHash,
        string? deviceId,
        string? ipAddress,
        string? userAgent,
        DateTimeOffset issuedAt,
        DateTimeOffset expiresAt)
    {
        if (id == Guid.Empty)
        {
            return RefreshTokenErrors.InvalidId;
        }

        if (tenantId == Guid.Empty)
        {
            return RefreshTokenErrors.InvalidTenantId;
        }

        if (userId == Guid.Empty)
        {
            return RefreshTokenErrors.InvalidUserId;
        }

        if (sessionId == Guid.Empty)
        {
            return RefreshTokenErrors.InvalidSessionId;
        }

        if (!IsValidTokenHash(tokenHash))
        {
            return RefreshTokenErrors.InvalidTokenHash;
        }

        if (expiresAt <= issuedAt)
        {
            return RefreshTokenErrors.InvalidExpiry;
        }

        return new RefreshToken(
            id,
            tenantId,
            userId,
            sessionId,
            tokenHash!,
            NormalizeOptional(deviceId, DeviceIdMaxLength),
            NormalizeOptional(ipAddress, IpAddressMaxLength),
            NormalizeOptional(userAgent, UserAgentMaxLength),
            issuedAt,
            expiresAt);
    }

    public bool IsActive(DateTimeOffset now) => RevokedAt is null && ExpiresAt > now;

    public Result Rotate(
        Guid replacementId,
        string? replacementHash,
        DateTimeOffset now,
        DateTimeOffset expiresAt)
    {
        if (RevokedAt is not null)
        {
            return RefreshTokenErrors.AlreadyRevoked;
        }

        if (now >= ExpiresAt)
        {
            return RefreshTokenErrors.Expired;
        }

        if (replacementId == Guid.Empty)
        {
            return RefreshTokenErrors.InvalidId;
        }

        if (!IsValidTokenHash(replacementHash))
        {
            return RefreshTokenErrors.InvalidTokenHash;
        }

        if (expiresAt <= now)
        {
            return RefreshTokenErrors.InvalidExpiry;
        }

        RevokedAt = now;
        RevokedReason = RefreshTokenRevokedReasons.Rotated;
        ReplacedBy = replacementId;
        return Result.Success();
    }

    public Result Revoke(DateTimeOffset now, string reason)
    {
        if (RevokedAt is not null)
        {
            return Result.Success();
        }

        RevokedAt = now;
        RevokedReason = NormalizeOptional(reason, RevokedReasonMaxLength);
        return Result.Success();
    }

    private static bool IsValidTokenHash(string? tokenHash) =>
        tokenHash is { Length: TokenHashLength }
        && tokenHash.All(character => char.IsAsciiHexDigit(character));

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var candidate = value.Trim();

        return candidate.Length <= maxLength ? candidate : candidate[..maxLength];
    }
}
