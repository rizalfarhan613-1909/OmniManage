using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.AssetMaintenance.Domain.Errors;

namespace OmniManage.Modules.AssetMaintenance.Domain.Assets;

public sealed class Asset : AggregateRoot<Guid>, ITenantEntity
{
    public const int AssetCodeMinLength = 2;
    public const int AssetCodeMaxLength = 50;
    public const int NameMaxLength = 200;
    public const int CategoryMaxLength = 100;
    public const int SerialNumberMaxLength = 100;
    public const int QrCodeUrlMaxLength = 1000;
    public const decimal MaxAmount = 999_999_999_999_999.9999m;

    private Asset()
    {
    }

    private Asset(
        Guid id,
        Guid tenantId,
        string assetCode,
        string name,
        string category,
        string? serialNumber,
        string? qrCodeUrl,
        decimal purchaseValue,
        DateOnly acquisitionDate)
        : base(id)
    {
        TenantId = tenantId;
        AssetCode = assetCode;
        Name = name;
        Category = category;
        SerialNumber = serialNumber;
        QrCodeUrl = qrCodeUrl;
        PurchaseValue = purchaseValue;
        AcquisitionDate = acquisitionDate;
        Status = AssetStatus.Active;
    }

    public Guid TenantId { get; private set; }

    public string AssetCode { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string Category { get; private set; } = string.Empty;

    public string? SerialNumber { get; private set; }

    public string? QrCodeUrl { get; private set; }

    public decimal PurchaseValue { get; private set; }

    public DateOnly AcquisitionDate { get; private set; }

    public AssetStatus Status { get; private set; }

    public static Result<Asset> Create(
        Guid id,
        Guid tenantId,
        string? assetCode,
        string? name,
        string? category,
        string? serialNumber,
        string? qrCodeUrl,
        decimal purchaseValue,
        DateOnly acquisitionDate)
    {
        if (id == Guid.Empty)
        {
            return AssetErrors.InvalidId;
        }

        if (tenantId == Guid.Empty)
        {
            return AssetErrors.InvalidTenantId;
        }

        if (!TryNormalizeAssetCode(assetCode, out var normalizedAssetCode))
        {
            return AssetErrors.InvalidAssetCode;
        }

        if (!TryNormalizeName(name, out var normalizedName))
        {
            return AssetErrors.InvalidName;
        }

        if (!TryNormalizeCategory(category, out var normalizedCategory))
        {
            return AssetErrors.InvalidCategory;
        }

        if (!TryNormalizeOptionalText(serialNumber, SerialNumberMaxLength, out var normalizedSerialNumber))
        {
            return AssetErrors.InvalidSerialNumber;
        }

        if (!TryNormalizeOptionalText(qrCodeUrl, QrCodeUrlMaxLength, out var normalizedQrCodeUrl))
        {
            return AssetErrors.InvalidQrCodeUrl;
        }

        if (purchaseValue is < 0m or > MaxAmount)
        {
            return AssetErrors.InvalidPurchaseValue;
        }

        if (acquisitionDate == default)
        {
            return AssetErrors.InvalidAcquisitionDate;
        }

        return new Asset(
            id,
            tenantId,
            normalizedAssetCode,
            normalizedName,
            normalizedCategory,
            normalizedSerialNumber,
            normalizedQrCodeUrl,
            purchaseValue,
            acquisitionDate);
    }

    public Result SendToMaintenance()
    {
        if (Status != AssetStatus.Active)
        {
            return AssetErrors.InvalidStatusTransition;
        }

        Status = AssetStatus.UnderMaintenance;

        return Result.Success();
    }

    public Result ReturnFromMaintenance()
    {
        if (Status != AssetStatus.UnderMaintenance)
        {
            return AssetErrors.InvalidStatusTransition;
        }

        Status = AssetStatus.Active;

        return Result.Success();
    }

    public Result Retire()
    {
        if (Status == AssetStatus.Retired)
        {
            return AssetErrors.InvalidStatusTransition;
        }

        Status = AssetStatus.Retired;

        return Result.Success();
    }

    private static bool TryNormalizeAssetCode(string? value, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim().ToUpperInvariant();

        if (candidate.Length is < AssetCodeMinLength or > AssetCodeMaxLength)
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

    private static bool TryNormalizeCategory(string? value, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim();

        if (candidate.Length > CategoryMaxLength)
        {
            return false;
        }

        normalized = candidate;
        return true;
    }

    private static bool TryNormalizeOptionalText(string? value, int maxLength, out string? normalized)
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
}
