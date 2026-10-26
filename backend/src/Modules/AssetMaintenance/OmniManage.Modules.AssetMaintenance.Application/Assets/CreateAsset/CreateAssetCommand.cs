using OmniManage.BuildingBlocks.Application;

namespace OmniManage.Modules.AssetMaintenance.Application.Assets.CreateAsset;

public sealed record CreateAssetCommand(
    string? AssetCode,
    string? Name,
    string? Category,
    string? SerialNumber,
    string? QrCodeUrl,
    decimal PurchaseValue,
    DateOnly AcquisitionDate) : ICommand<Guid>;
