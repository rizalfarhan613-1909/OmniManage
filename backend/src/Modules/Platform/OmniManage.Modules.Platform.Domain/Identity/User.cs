using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Platform.Domain.Errors;

namespace OmniManage.Modules.Platform.Domain.Identity;

public sealed class User : AggregateRoot<Guid>, ITenantEntity
{
    public const int EmailMaxLength = 320;
    public const int PasswordHashMaxLength = 512;

    private readonly List<UserRole> _userRoles = [];

    private User()
    {
    }

    private User(Guid id, Guid tenantId, string email, string passwordHash)
        : base(id)
    {
        TenantId = tenantId;
        Email = email;
        PasswordHash = passwordHash;
        IsActive = true;
    }

    public Guid TenantId { get; private set; }

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    public static Result<User> Create(Guid id, Guid tenantId, string? email, string? passwordHash)
    {
        if (id == Guid.Empty)
        {
            return UserErrors.InvalidId;
        }

        if (tenantId == Guid.Empty)
        {
            return UserErrors.InvalidTenantId;
        }

        if (!TryNormalizeEmail(email, out var normalizedEmail))
        {
            return UserErrors.InvalidEmail;
        }

        if (string.IsNullOrWhiteSpace(passwordHash) || passwordHash.Length > PasswordHashMaxLength)
        {
            return UserErrors.InvalidPasswordHash;
        }

        return new User(id, tenantId, normalizedEmail, passwordHash);
    }

    public Result AssignRole(Role role)
    {
        ArgumentNullException.ThrowIfNull(role);

        if (role.TenantId != TenantId)
        {
            return UserErrors.RoleTenantMismatch;
        }

        if (_userRoles.Exists(userRole => userRole.RoleId == role.Id))
        {
            return Result.Success();
        }

        _userRoles.Add(new UserRole(TenantId, Id, role.Id));
        return Result.Success();
    }

    public Result RemoveRole(Guid roleId)
    {
        var removed = _userRoles.RemoveAll(userRole => userRole.RoleId == roleId);

        return removed == 0 ? UserErrors.RoleNotAssigned : Result.Success();
    }

    public Result ChangePasswordHash(string? passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash) || passwordHash.Length > PasswordHashMaxLength)
        {
            return UserErrors.InvalidPasswordHash;
        }

        PasswordHash = passwordHash;
        return Result.Success();
    }

    public Result Activate()
    {
        IsActive = true;
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
