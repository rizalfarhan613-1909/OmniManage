using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Manufacturing.Domain.Abstractions;
using OmniManage.Modules.Manufacturing.Domain.Errors;
using OmniManage.Modules.Manufacturing.Domain.WorkOrders;

namespace OmniManage.Modules.Manufacturing.Application.WorkOrders.CreateWorkOrder;

public sealed class CreateWorkOrderCommandHandler(
    IWorkOrderRepository workOrders,
    IBillOfMaterialRepository billOfMaterials,
    ITenantContext tenantContext,
    ICurrentUser currentUser,
    IGuidGenerator guidGenerator,
    TimeProvider clock)
    : ICommandHandler<CreateWorkOrderCommand, WorkOrderDto>
{
    public async Task<Result<WorkOrderDto>> Handle(
        CreateWorkOrderCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var billOfMaterial = await billOfMaterials
            .GetByIdAsync(command.BillOfMaterialId, cancellationToken)
            .ConfigureAwait(false);

        if (billOfMaterial is null)
        {
            return WorkOrderErrors.BillOfMaterialNotFound;
        }

        if (!billOfMaterial.IsActive)
        {
            return BillOfMaterialErrors.Inactive;
        }

        var created = WorkOrder.Create(
            guidGenerator.NewGuid(),
            tenantContext.TenantId,
            command.OrderNumber,
            billOfMaterial.ProductItemId,
            billOfMaterial.Id,
            billOfMaterial.Code,
            command.QuantityPlanned,
            command.DueDate,
            command.Notes,
            clock.GetUtcNow(),
            currentUser.UserId);

        if (created.IsFailure)
        {
            return created.Error;
        }

        var workOrder = created.Value;

        if (await workOrders
                .OrderNumberExistsAsync(workOrder.OrderNumber, cancellationToken)
                .ConfigureAwait(false))
        {
            return WorkOrderErrors.OrderNumberAlreadyExists;
        }

        await workOrders.AddAsync(workOrder, cancellationToken).ConfigureAwait(false);
        await workOrders.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return WorkOrderDto.From(workOrder);
    }
}
