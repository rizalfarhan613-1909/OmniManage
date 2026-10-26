using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.AssetMaintenance.Domain.Abstractions;
using OmniManage.Modules.AssetMaintenance.Domain.Assets;
using OmniManage.Modules.AssetMaintenance.Domain.Errors;

namespace OmniManage.Modules.AssetMaintenance.Application.Assets.CreateAsset;

public sealed class CreateAssetCommandHandler(
    IAssetRepository assets,
    ITenantContext tenantContext,
    IGuidGenerator guidGenerator)
    : ICommandHandler<CreateAssetCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreateAssetCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var created = Asset.Create(
            guidGenerator.NewGuid(),
            tenantContext.TenantId,
            command.AssetCode,
            command.Name,
            command.Category,
            command.SerialNumber,
            command.QrCodeUrl,
            command.PurchaseValue,
            command.AcquisitionDate);

        if (created.IsFailure)
        {
            return created.Error;
        }

        var asset = created.Value;

        if (await assets
                .AssetCodeExistsAsync(asset.AssetCode, cancellationToken)
                .ConfigureAwait(false))
        {
            return AssetErrors.AssetCodeAlreadyExists;
        }

        await assets
            .AddAsync(asset, cancellationToken)
            .ConfigureAwait(false);
        await assets
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return asset.Id;
    }
}
