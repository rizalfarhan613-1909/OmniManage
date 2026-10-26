using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.MasterData.Domain.Abstractions;
using OmniManage.Modules.MasterData.Domain.Errors;
using OmniManage.Modules.MasterData.Domain.Items;

namespace OmniManage.Modules.MasterData.Application.Items.CreateItem;

public sealed class CreateItemCommandHandler(
    IItemRepository items,
    ITenantContext tenantContext,
    ICurrentUser currentUser,
    IGuidGenerator guidGenerator,
    TimeProvider clock)
    : ICommandHandler<CreateItemCommand, ItemDto>
{
    public async Task<Result<ItemDto>> Handle(
        CreateItemCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var created = Item.Create(
            guidGenerator.NewGuid(),
            tenantContext.TenantId,
            command.Sku,
            command.Name,
            command.UomCode,
            command.Category,
            command.BasePrice,
            command.CostPrice,
            command.IsStockTracked,
            clock.GetUtcNow(),
            currentUser.UserId);

        if (created.IsFailure)
        {
            return created.Error;
        }

        var item = created.Value;

        if (await items.SkuExistsAsync(item.Sku, cancellationToken).ConfigureAwait(false))
        {
            return ItemErrors.SkuAlreadyExists;
        }

        await items.AddAsync(item, cancellationToken).ConfigureAwait(false);
        await items.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return ItemDto.From(item);
    }
}
