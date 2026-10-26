using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.MasterData.Domain.Errors;

namespace OmniManage.Modules.MasterData.Domain.BusinessPartners;

public sealed class BusinessPartner : AggregateRoot<Guid>, ITenantEntity, IAuditable
{
    public const int CodeMinLength = 2;
    public const int CodeMaxLength = 50;
    public const int NameMaxLength = 200;
    public const int TaxIdMaxLength = 50;
    public const decimal CreditLimitMax = 999_999_999_999_999.9999m;

    private BusinessPartner()
    {
    }

    private BusinessPartner(
        Guid id,
        Guid tenantId,
        string code,
        string name,
        PartnerType partnerType,
        string? taxId,
        decimal creditLimit,
        DateTimeOffset createdAt,
        Guid createdBy)
        : base(id)
    {
        TenantId = tenantId;
        Code = code;
        Name = name;
        PartnerType = partnerType;
        TaxId = taxId;
        CreditLimit = creditLimit;
        IsActive = true;
        CreatedAt = createdAt;
        CreatedBy = createdBy;
    }

    public Guid TenantId { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public PartnerType PartnerType { get; private set; }

    // ASSUMPTION: field-level encryption (Security.md §5.2) is not yet available in BuildingBlocks.
    // The value is flagged for redaction now; at-rest encryption must be added with the field encryptor.
    [Sensitive]
    public string? TaxId { get; private set; }

    public decimal CreditLimit { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public Guid CreatedBy { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public Guid? UpdatedBy { get; private set; }

    public static Result<BusinessPartner> Create(
        Guid id,
        Guid tenantId,
        string? code,
        string? name,
        string? partnerType,
        string? taxId,
        decimal creditLimit,
        DateTimeOffset createdAt,
        Guid createdBy)
    {
        if (id == Guid.Empty)
        {
            return BusinessPartnerErrors.InvalidId;
        }

        if (tenantId == Guid.Empty)
        {
            return BusinessPartnerErrors.InvalidTenantId;
        }

        if (!TryNormalizeCode(code, out var normalizedCode))
        {
            return BusinessPartnerErrors.InvalidCode;
        }

        if (!TryNormalizeName(name, out var normalizedName))
        {
            return BusinessPartnerErrors.InvalidName;
        }

        if (!TryParsePartnerType(partnerType, out var parsedPartnerType))
        {
            return BusinessPartnerErrors.InvalidPartnerType;
        }

        if (!TryNormalizeTaxId(taxId, out var normalizedTaxId))
        {
            return BusinessPartnerErrors.InvalidTaxId;
        }

        if (creditLimit is < 0 or > CreditLimitMax)
        {
            return BusinessPartnerErrors.InvalidCreditLimit;
        }

        if (createdAt == default)
        {
            return BusinessPartnerErrors.InvalidCreatedAt;
        }

        if (createdBy == Guid.Empty)
        {
            return BusinessPartnerErrors.InvalidCreatedBy;
        }

        return new BusinessPartner(
            id,
            tenantId,
            normalizedCode,
            normalizedName,
            parsedPartnerType,
            normalizedTaxId,
            creditLimit,
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
            if (!char.IsAsciiLetterOrDigit(character) && character is not ('-' or '_'))
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

    private static bool TryParsePartnerType(string? value, out PartnerType partnerType)
    {
        partnerType = default;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim();

        if (candidate.All(char.IsAsciiDigit)
            || !Enum.TryParse(candidate, ignoreCase: true, out partnerType))
        {
            return false;
        }

        return Enum.IsDefined(partnerType);
    }

    private static bool TryNormalizeTaxId(string? value, out string? normalized)
    {
        normalized = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var candidate = value.Trim();

        if (candidate.Length > TaxIdMaxLength)
        {
            return false;
        }

        normalized = candidate;
        return true;
    }
}
