using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Platform.Domain.Errors;

namespace OmniManage.Modules.Platform.Domain.Identity;

/// <summary>
/// A member of the platform realm (System Superadmin). Platform users are not
/// tenant-scoped and are intentionally isolated from <see cref="User"/> rows so
/// the two identity realms in Security.md §2.1 can never share tokens.
/// </summary>
public sealed class PlatformUser : AggregateRoot<Guid>
{
    public const int EmailMaxLength = 320;
    public const int PasswordHashMaxLength = 512;

    private PlatformUser()
    {
    }

    private PlatformUser(Guid id, string email, string passwordHash, DateTimeOffset createdAt)
        : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
        IsActive = true;
        IsMfaEnabled = true;
        CreatedAt = createdAt;
    }

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public bool IsMfaEnabled { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static Result<PlatformUser> Create(
        Guid id,
        string? email,
        string? passwordHash,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            return PlatformUserErrors.InvalidId;
        }

        if (!TryNormalizeEmail(email, out var normalizedEmail))
        {
            return PlatformUserErrors.InvalidEmail;
        }

        if (string.IsNullOrWhiteSpace(passwordHash) || passwordHash.Length > PasswordHashMaxLength)
        {
            return PlatformUserErrors.InvalidPasswordHash;
        }

        if (createdAt == default)
        {
            return PlatformUserErrors.InvalidCreatedAt;
        }

        return new PlatformUser(id, normalizedEmail, passwordHash, createdAt);
    }

    public Result ChangePasswordHash(string? passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash) || passwordHash.Length > PasswordHashMaxLength)
        {
            return PlatformUserErrors.InvalidPasswordHash;
        }

        PasswordHash = passwordHash;
        return Result.Success();
    }

    public Result Deactivate()
    {
        IsActive = false;
        return Result.Success();
    }

    private static bool TryNormalizeEmail(string? value, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim().ToLowerInvariant();
        var atIndex = candidate.IndexOf('@');

        if (candidate.Length > EmailMaxLength
            || atIndex <= 0
            || atIndex == candidate.Length - 1
            || candidate.Any(char.IsWhiteSpace))
        {
            return false;
        }

        normalized = candidate;
        return true;
    }
}
