using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Finance.Domain.Errors;

namespace OmniManage.Modules.Finance.Domain.Gl;

public sealed class GlAccount : AggregateRoot<Guid>, ITenantEntity, IAuditable
{
    public const int CodeMaxLength = 20;
    public const int NameMaxLength = 200;

    private GlAccount()
    {
    }

    private GlAccount(
        Guid id,
        Guid tenantId,
        string accountCode,
        string accountName,
        GlAccountType type,
        bool isActive,
        DateTimeOffset createdAt,
        Guid createdBy)
        : base(id)
    {
        TenantId = tenantId;
        AccountCode = accountCode;
        AccountName = accountName;
        Type = type;
        IsActive = isActive;
        CreatedAt = createdAt;
        CreatedBy = createdBy;
    }

    public Guid TenantId { get; private set; }

    public string AccountCode { get; private set; } = string.Empty;

    public string AccountName { get; private set; } = string.Empty;

    public GlAccountType Type { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public Guid CreatedBy { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public Guid? UpdatedBy { get; private set; }

    public static Result<GlAccount> Create(
        Guid id,
        Guid tenantId,
        string? accountCode,
        string? accountName,
        GlAccountType type,
        bool isActive,
        DateTimeOffset createdAt,
        Guid createdBy)
    {
        if (id == Guid.Empty)
        {
            return GlAccountErrors.InvalidId;
        }

        if (tenantId == Guid.Empty)
        {
            return GlAccountErrors.InvalidTenantId;
        }

        if (!TryNormalizeCode(accountCode, out var normalizedCode))
        {
            return GlAccountErrors.InvalidCode;
        }

        if (!TryNormalizeName(accountName, out var normalizedName))
        {
            return GlAccountErrors.InvalidName;
        }

        if (!Enum.IsDefined(type))
        {
            return GlAccountErrors.InvalidType;
        }

        if (createdAt == default)
        {
            return GlAccountErrors.InvalidCreatedAt;
        }

        if (createdBy == Guid.Empty)
        {
            return GlAccountErrors.InvalidCreatedBy;
        }

        return new GlAccount(
            id,
            tenantId,
            normalizedCode,
            normalizedName,
            type,
            isActive,
            createdAt,
            createdBy);
    }

    private static bool TryNormalizeCode(string? value, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim().ToUpperInvariant();

        if (candidate.Length > CodeMaxLength || !char.IsAsciiLetterOrDigit(candidate[0]))
        {
            return false;
        }

        foreach (var character in candidate)
        {
            if (!char.IsAsciiLetterOrDigit(character) && character is not ('-' or '_' or '.'))
            {
                return false;
            }
        }

        normalized = candidate;
        return true;
    }

    private static bool TryNormalizeName(string? value, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim();

        if (candidate.Length > NameMaxLength)
        {
            return false;
        }

        normalized = candidate;
        return true;
    }
}
