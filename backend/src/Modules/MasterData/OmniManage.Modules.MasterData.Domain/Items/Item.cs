using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.MasterData.Domain.Errors;

namespace OmniManage.Modules.MasterData.Domain.Items;

public sealed class Item : AggregateRoot<Guid>, ITenantEntity, IAuditable
{
    public const int SkuMinLength = 2;
    public const int SkuMaxLength = 50;
    public const int NameMaxLength = 200;
    public const int UomCodeMinLength = 2;
    public const int UomCodeMaxLength = 10;
    public const int CategoryMaxLength = 100;
    public const decimal PriceMax = 999_999_999_999_999.9999m;

    private Item()
    {
    }

    private Item(
        Guid id,
        Guid tenantId,
        string sku,
        string name,
        string uomCode,
        string? category,
        decimal basePrice,
        decimal costPrice,
        bool isStockTracked,
        DateTimeOffset createdAt,
        Guid createdBy)
        : base(id)
    {
        TenantId = tenantId;
        Sku = sku;
        Name = name;
        UomCode = uomCode;
        Category = category;
        BasePrice = basePrice;
        CostPrice = costPrice;
        IsStockTracked = isStockTracked;
        IsActive = true;
        CreatedAt = createdAt;
        CreatedBy = createdBy;
    }

    public Guid TenantId { get; private set; }

    public string Sku { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string UomCode { get; private set; } = string.Empty;

    public string? Category { get; private set; }

    public decimal BasePrice { get; private set; }

    public decimal CostPrice { get; private set; }

    public bool IsStockTracked { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public Guid CreatedBy { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public Guid? UpdatedBy { get; private set; }

    public static Result<Item> Create(
        Guid id,
        Guid tenantId,
        string? sku,
        string? name,
        string? uomCode,
        string? category,
        decimal basePrice,
        decimal costPrice,
        bool isStockTracked,
        DateTimeOffset createdAt,
        Guid createdBy)
    {
        if (id == Guid.Empty)
        {
            return ItemErrors.InvalidId;
        }

        if (tenantId == Guid.Empty)
        {
            return ItemErrors.InvalidTenantId;
        }

        if (!TryNormalizeSku(sku, out var normalizedSku))
        {
            return ItemErrors.InvalidSku;
        }

        if (!TryNormalizeName(name, out var normalizedName))
        {
            return ItemErrors.InvalidName;
        }

        if (!TryNormalizeUomCode(uomCode, out var normalizedUomCode))
        {
            return ItemErrors.InvalidUomCode;
        }

        if (!TryNormalizeCategory(category, out var normalizedCategory))
        {
            return ItemErrors.InvalidCategory;
        }

        if (basePrice is < 0 or > PriceMax)
        {
            return ItemErrors.InvalidBasePrice;
        }

        if (costPrice is < 0 or > PriceMax)
        {
            return ItemErrors.InvalidCostPrice;
        }

        if (createdAt == default)
        {
            return ItemErrors.InvalidCreatedAt;
        }

        if (createdBy == Guid.Empty)
        {
            return ItemErrors.InvalidCreatedBy;
        }

        return new Item(
            id,
            tenantId,
            normalizedSku,
            normalizedName,
            normalizedUomCode,
            normalizedCategory,
            basePrice,
            costPrice,
            isStockTracked,
            createdAt,
            createdBy);
    }

    private static bool TryNormalizeSku(string? value, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim().ToUpperInvariant();

        if (candidate.Length is < SkuMinLength or > SkuMaxLength)
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

    private static bool TryNormalizeUomCode(string? value, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim().ToUpperInvariant();

        if (candidate.Length is < UomCodeMinLength or > UomCodeMaxLength)
        {
            return false;
        }

        if (!candidate.All(char.IsAsciiLetterOrDigit))
        {
            return false;
        }

        normalized = candidate;
        return true;
    }

    private static bool TryNormalizeCategory(string? value, out string? normalized)
    {
        normalized = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var candidate = value.Trim();

        if (candidate.Length > CategoryMaxLength)
        {
            return false;
        }

        normalized = candidate;
        return true;
    }
}
