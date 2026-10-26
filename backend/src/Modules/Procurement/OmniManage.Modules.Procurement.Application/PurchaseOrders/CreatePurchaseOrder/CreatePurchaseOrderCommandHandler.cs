using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Procurement.Domain.Abstractions;
using OmniManage.Modules.Procurement.Domain.Errors;
using OmniManage.Modules.Procurement.Domain.PurchaseOrders;

namespace OmniManage.Modules.Procurement.Application.PurchaseOrders.CreatePurchaseOrder;

public sealed class CreatePurchaseOrderCommandHandler(
    IPurchaseOrderRepository purchaseOrders,
    ISupplierRepository suppliers,
    ITenantContext tenantContext,
    ICurrentUser currentUser,
    IGuidGenerator guidGenerator,
    TimeProvider clock)
    : ICommandHandler<CreatePurchaseOrderCommand, PurchaseOrderDto>
{
    public async Task<Result<PurchaseOrderDto>> Handle(
        CreatePurchaseOrderCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var supplier = await suppliers
            .GetByIdAsync(command.SupplierId, cancellationToken)
            .ConfigureAwait(false);

        if (supplier is null)
        {
            return PurchaseOrderErrors.SupplierNotFound;
        }

        if (!supplier.IsActive)
        {
            return SupplierErrors.Inactive;
        }

        var lines = command.Lines?
            .Select(line => new PurchaseOrderLineInput(
                guidGenerator.NewGuid(),
                line.ItemId,
                line.Quantity,
                line.UnitPrice))
            .ToArray();

        var created = PurchaseOrder.Create(
            guidGenerator.NewGuid(),
            tenantContext.TenantId,
            command.OrderNumber,
            supplier.Id,
            supplier.Name,
            command.ExpectedDeliveryDate,
            command.Notes,
            lines,
            clock.GetUtcNow(),
            currentUser.UserId);

        if (created.IsFailure)
        {
            return created.Error;
        }

        var purchaseOrder = created.Value;

        if (await purchaseOrders
                .OrderNumberExistsAsync(purchaseOrder.OrderNumber, cancellationToken)
                .ConfigureAwait(false))
        {
            return PurchaseOrderErrors.OrderNumberAlreadyExists;
        }

        await purchaseOrders.AddAsync(purchaseOrder, cancellationToken).ConfigureAwait(false);
        await purchaseOrders.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return PurchaseOrderDto.From(purchaseOrder);
    }
}
