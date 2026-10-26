using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Procurement.Domain.Errors;

namespace OmniManage.Modules.Procurement.Domain.Suppliers;

/// <summary>
/// Vendor master owned by Procurement. Kept inside the module so purchase
/// orders never read another module's tables (Rules.md §2).
/// </summary>
public sealed class Supplier : AggregateRoot<Guid>, ITenantEntity, IAuditable
{
    public const int CodeMinLength = 2;
    public const int CodeMaxLength = 50;
    public const int NameMaxLength = 200;
    public const int ContactNameMaxLength = 100;
    public const int EmailMaxLength = 320;
    public const int PhoneMaxLength = 50;
    public const int PaymentTermsDaysMax = 365;

    private Supplier()
    {
    }

    private Supplier(
        Guid id,
        Guid tenantId,
        string code,
        string name,
        string? contactName,
        string? email,
        string? phone,
        int paymentTermsDays,
        DateTimeOffset createdAt,
        Guid createdBy)
        : base(id)
    {
        TenantId = tenantId;
        Code = code;
        Name = name;
        ContactName = contactName;
        Email = email;
        Phone = phone;
        PaymentTermsDays = paymentTermsDays;
        IsActive = true;
        CreatedAt = createdAt;
        CreatedBy = createdBy;
    }

    public Guid TenantId { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? ContactName { get; private set; }

    public string? Email { get; private set; }

    public string? Phone { get; private set; }

    public int PaymentTermsDays { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public Guid CreatedBy { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public Guid? UpdatedBy { get; private set; }

    public static Result<Supplier> Create(
        Guid id,
        Guid tenantId,
        string? code,
        string? name,
        string? contactName,
        string? email,
        string? phone,
        int paymentTermsDays,
        DateTimeOffset createdAt,
        Guid createdBy)
    {
        if (id == Guid.Empty)
        {
            return SupplierErrors.InvalidId;
        }

        if (tenantId == Guid.Empty)
        {
            return SupplierErrors.InvalidTenantId;
        }

        if (!TryNormalizeCode(code, out var normalizedCode))
        {
            return SupplierErrors.InvalidCode;
        }

        if (!TryNormalizeName(name, out var normalizedName))
        {
            return SupplierErrors.InvalidName;
        }

        if (!TryNormalizeOptional(contactName, ContactNameMaxLength, out var normalizedContactName))
        {
            return SupplierErrors.InvalidContactName;
        }

        if (!TryNormalizeEmail(email, out var normalizedEmail))
        {
            return SupplierErrors.InvalidEmail;
        }

        if (!TryNormalizeOptional(phone, PhoneMaxLength, out var normalizedPhone))
        {
            return SupplierErrors.InvalidPhone;
        }

        if (paymentTermsDays is < 0 or > PaymentTermsDaysMax)
        {
            return SupplierErrors.InvalidPaymentTermsDays;
        }

        if (createdAt == default)
        {
            return SupplierErrors.InvalidCreatedAt;
        }

        if (createdBy == Guid.Empty)
        {
            return SupplierErrors.InvalidCreatedBy;
        }

        return new Supplier(
            id,
            tenantId,
            normalizedCode,
            normalizedName,
            normalizedContactName,
            normalizedEmail,
            normalizedPhone,
            paymentTermsDays,
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

        if (candidate.Length is < CodeMinLength or > CodeMaxLength)
        {
            return false;
        }

        if (!char.IsAsciiLetterOrDigit(candidate[0]))
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

    private static bool TryNormalizeOptional(
        string? value,
        int maxLength,
        out string? normalized)
    {
        normalized = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var candidate = value.Trim();

        if (candidate.Length > maxLength)
        {
            return false;
        }

        normalized = candidate;
        return true;
    }

    private static bool TryNormalizeEmail(string? value, out string? normalized)
    {
        normalized = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var candidate = value.Trim();

        if (candidate.Length > EmailMaxLength
            || candidate.Any(char.IsWhiteSpace)
            || candidate.Count(character => character == '@') != 1)
        {
            return false;
        }

        var parts = candidate.Split('@');

        if (parts[0].Length == 0 || parts[1].Length == 0)
        {
            return false;
        }

        normalized = candidate;
        return true;
    }
}
