namespace OmniManage.Modules.AssetMaintenance.Presentation.Controllers;

public sealed record CreateAssetRequest(
    string? AssetCode,
    string? Name,
    string? Category,
    string? SerialNumber,
    string? QrCodeUrl,
    decimal PurchaseValue,
    DateOnly AcquisitionDate);
