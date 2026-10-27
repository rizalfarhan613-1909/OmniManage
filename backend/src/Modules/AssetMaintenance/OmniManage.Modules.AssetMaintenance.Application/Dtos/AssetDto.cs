using System.Globalization;
using OmniManage.Modules.AssetMaintenance.Domain.Assets;

namespace OmniManage.Modules.AssetMaintenance.Application.Dtos;

public sealed record AssetDto(
    Guid Id,
    string AssetCode,
    string Name,
    string Category,
    string? SerialNumber,
    string? QrCodeUrl,
    string PurchaseValue,
    DateOnly AcquisitionDate,
    string Status)
{
    internal static AssetDto From(Asset asset)
    {
        ArgumentNullException.ThrowIfNull(asset);

        return new AssetDto(
            asset.Id,
            asset.AssetCode,
            asset.Name,
            asset.Category,
            asset.SerialNumber,
            asset.QrCodeUrl,
            asset.PurchaseValue.ToString("0.0000", CultureInfo.InvariantCulture),
            asset.AcquisitionDate,
            asset.Status.ToString());
    }
}
